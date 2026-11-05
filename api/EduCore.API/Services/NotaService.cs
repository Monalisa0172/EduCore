using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Acesso;

namespace EduCore.API.Services;

/// <summary>
/// Lançamento de notas por avaliação, em lote. Regras: só quem leciona (ou o admin), avaliação e
/// turma ativas, só alunos matriculados e ativos, nota de 0 a 10 com até 2 casas.
/// </summary>
public class NotaService
{
    private const string NaoEncontrada = "Avaliação não encontrada.";

    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly INotaRepository _notaRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IAcessoAcademico _acesso;

    public NotaService(
        IAvaliacaoRepository avaliacaoRepository,
        INotaRepository notaRepository,
        ITurmaRepository turmaRepository,
        IAcessoAcademico acesso)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _notaRepository = notaRepository;
        _turmaRepository = turmaRepository;
        _acesso = acesso;
    }

    public async Task<ResultadoOperacao<NotasDaAvaliacaoDTO>> ObterAsync(int avaliacaoId)
    {
        var avaliacao = await _avaliacaoRepository.GetByIdAsync(avaliacaoId);
        if (avaliacao is null || !await _acesso.PodeGerenciarAvaliacaoAsync(avaliacao.TurmaId, avaliacao.DisciplinaId, avaliacao.SubDisciplinaId))
            return ResultadoOperacao<NotasDaAvaliacaoDTO>.NaoEncontrado(NaoEncontrada);

        var turma = await _turmaRepository.GetEntityByIdAsync(avaliacao.TurmaId);
        var alunos = await _turmaRepository.GetAlunosAsync(avaliacao.TurmaId);
        var notas = (await _notaRepository.GetDaAvaliacaoAsync(avaliacaoId)).ToDictionary(n => n.AlunoId, n => n.Valor);

        var grade = alunos
            .Select(a => new NotaAlunoDTO
            {
                AlunoId = a.AlunoId,
                Nome = a.Nome,
                Ativo = a.Ativo,
                Nota = notas.TryGetValue(a.AlunoId, out var valor) ? valor : null
            })
            .ToList();

        return ResultadoOperacao<NotasDaAvaliacaoDTO>.Ok(new NotasDaAvaliacaoDTO
        {
            Avaliacao = avaliacao,
            PodeLancar = avaliacao.Ativo && turma is { Ativo: true },
            Lancadas = grade.Count(a => a.Nota.HasValue),
            Alunos = grade
        });
    }

    public async Task<ResultadoOperacao<NotasDaAvaliacaoDTO>> LancarAsync(int avaliacaoId, LancarNotasRequest request)
    {
        var avaliacao = await _avaliacaoRepository.GetEntityByIdAsync(avaliacaoId);
        if (avaliacao is null || !await _acesso.PodeGerenciarAvaliacaoAsync(avaliacao.TurmaId, avaliacao.DisciplinaId, avaliacao.SubDisciplinaId))
            return ResultadoOperacao<NotasDaAvaliacaoDTO>.NaoEncontrado(NaoEncontrada);

        if (!avaliacao.Ativo)
            return Invalido("A avaliação está inativa. Reative-a para lançar notas.");

        var turma = await _turmaRepository.GetEntityByIdAsync(avaliacao.TurmaId);
        if (turma is not { Ativo: true })
            return Invalido("A turma está inativa: as notas dela ficam só para consulta.");

        if (request.Notas.Select(n => n.AlunoId).Distinct().Count() != request.Notas.Count)
            return Invalido("O mesmo aluno aparece mais de uma vez no envio.");

        if (request.Notas.Any(n => n.Valor.HasValue && decimal.Round(n.Valor.Value, 2) != n.Valor.Value))
            return Invalido("Use no máximo 2 casas decimais na nota.");

        var alunosDaTurma = (await _turmaRepository.GetAlunosAsync(turma.Id)).ToDictionary(a => a.AlunoId);

        var foraDaTurma = request.Notas.Where(n => !alunosDaTurma.ContainsKey(n.AlunoId)).ToList();
        if (foraDaTurma.Count > 0)
            return Invalido("Um ou mais alunos não estão matriculados nesta turma. Atualize a lista e tente de novo.");

        var existentes = (await _notaRepository.GetDaAvaliacaoAsync(avaliacaoId)).ToDictionary(n => n.AlunoId);

        var inativos = request.Notas
            .Where(n => !alunosDaTurma[n.AlunoId].Ativo && NotaMuda(existentes, n))
            .Select(n => alunosDaTurma[n.AlunoId].Nome)
            .ToList();
        if (inativos.Count > 0)
            return Invalido($"Aluno inativo não recebe nota: {string.Join(", ", inativos)}.");

        foreach (var item in request.Notas)
        {
            existentes.TryGetValue(item.AlunoId, out var nota);

            if (item.Valor is null)
            {
                if (nota is not null)
                    _notaRepository.Remover(nota);
            }
            else if (nota is null)
            {
                _notaRepository.Adicionar(new Nota { AvaliacaoId = avaliacaoId, AlunoId = item.AlunoId, Valor = item.Valor.Value });
            }
            else
            {
                nota.Valor = item.Valor.Value;
            }
        }

        await _notaRepository.SalvarAsync();

        return await ObterAsync(avaliacaoId);
    }

    private static bool NotaMuda(Dictionary<int, Nota> existentes, NotaLancamentoRequest item)
        => (existentes.TryGetValue(item.AlunoId, out var nota) ? nota.Valor : (decimal?)null) != item.Valor;

    private static ResultadoOperacao<NotasDaAvaliacaoDTO> Invalido(string mensagem)
        => ResultadoOperacao<NotasDaAvaliacaoDTO>.Invalido(nameof(LancarNotasRequest.Notas), mensagem);
}
