using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Academico;
using EduCore.API.Services.Acesso;

namespace EduCore.API.Services;

/// <summary>
/// Chamada por aula (turma + disciplina + dia), em lote. Regras: só quem leciona a disciplina na turma
/// (ou o admin), turma e disciplina ativas, data dentro do ano letivo e não futura, só alunos matriculados.
/// </summary>
public class ChamadaService
{
    private const string NaoEncontrada = "Turma ou disciplina não encontrada.";

    private readonly IPresencaRepository _presencaRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IDisciplinaRepository _disciplinaRepository;
    private readonly IAcessoAcademico _acesso;
    private readonly CalendarioLetivo _calendario;
    private readonly TimeProvider _relogio;

    public ChamadaService(
        IPresencaRepository presencaRepository,
        ITurmaRepository turmaRepository,
        IDisciplinaRepository disciplinaRepository,
        IAcessoAcademico acesso,
        CalendarioLetivo calendario,
        TimeProvider relogio)
    {
        _presencaRepository = presencaRepository;
        _turmaRepository = turmaRepository;
        _disciplinaRepository = disciplinaRepository;
        _acesso = acesso;
        _calendario = calendario;
        _relogio = relogio;
    }

    private DateOnly Hoje => DateOnly.FromDateTime(_relogio.GetLocalNow().DateTime);

    public async Task<ResultadoOperacao<ChamadaDTO>> ObterAsync(int turmaId, int disciplinaId, DateOnly data)
    {
        var contexto = await CarregarContextoAsync(turmaId, disciplinaId);
        if (contexto is null)
            return ResultadoOperacao<ChamadaDTO>.NaoEncontrado(NaoEncontrada);

        var (turma, disciplina) = contexto.Value;
        var alunos = await _turmaRepository.GetAlunosAsync(turmaId);
        var registros = (await _presencaRepository.GetDaAulaAsync(turmaId, disciplinaId, data)).ToDictionary(p => p.AlunoId, p => p.Presente);

        var lista = alunos
            .Select(a => new ChamadaAlunoDTO
            {
                AlunoId = a.AlunoId,
                Nome = a.Nome,
                Ativo = a.Ativo,
                Presente = registros.TryGetValue(a.AlunoId, out var presente) ? presente : null
            })
            .ToList();

        return ResultadoOperacao<ChamadaDTO>.Ok(new ChamadaDTO
        {
            TurmaId = turma.Id,
            TurmaNome = turma.Nome ?? string.Empty,
            DisciplinaId = disciplina.Id,
            DisciplinaNome = disciplina.Nome ?? string.Empty,
            Data = data,
            Bimestre = _calendario.BimestreDaData(data),
            Registrada = registros.Count > 0,
            PodeRegistrar = turma.Ativo && disciplina.Ativo && ErroDeData(turma, data) is null,
            Presentes = lista.Count(a => a.Presente == true),
            Alunos = lista
        });
    }

    public async Task<ResultadoOperacao<List<AulaRegistradaDTO>>> ListarAulasAsync(int turmaId, int disciplinaId)
    {
        if (await CarregarContextoAsync(turmaId, disciplinaId) is null)
            return ResultadoOperacao<List<AulaRegistradaDTO>>.NaoEncontrado(NaoEncontrada);

        var aulas = await _presencaRepository.GetAulasAsync(turmaId, disciplinaId);

        return ResultadoOperacao<List<AulaRegistradaDTO>>.Ok(aulas
            .Select(a => new AulaRegistradaDTO
            {
                Data = a.Data,
                Bimestre = _calendario.BimestreDaData(a.Data),
                Presentes = a.Presentes,
                Total = a.Total
            })
            .ToList());
    }

    public async Task<ResultadoOperacao<ChamadaDTO>> RegistrarAsync(RegistrarChamadaRequest request)
    {
        var contexto = await CarregarContextoAsync(request.TurmaId, request.DisciplinaId);
        if (contexto is null)
            return ResultadoOperacao<ChamadaDTO>.NaoEncontrado(NaoEncontrada);

        var (turma, disciplina) = contexto.Value;
        var data = request.Data!.Value;

        if (!turma.Ativo)
            return Invalido(nameof(RegistrarChamadaRequest.TurmaId), "A turma está inativa: a chamada fica só para consulta.");
        if (!disciplina.Ativo)
            return Invalido(nameof(RegistrarChamadaRequest.DisciplinaId), $"A disciplina {disciplina.Nome} está inativa.");

        var erroData = ErroDeData(turma, data);
        if (erroData is not null)
            return Invalido(nameof(RegistrarChamadaRequest.Data), erroData);

        if (request.Presencas.Select(p => p.AlunoId).Distinct().Count() != request.Presencas.Count)
            return Invalido(nameof(RegistrarChamadaRequest.Presencas), "O mesmo aluno aparece mais de uma vez no envio.");

        var alunosDaTurma = (await _turmaRepository.GetAlunosAsync(turma.Id)).ToDictionary(a => a.AlunoId);
        if (request.Presencas.Any(p => !alunosDaTurma.ContainsKey(p.AlunoId)))
            return Invalido(nameof(RegistrarChamadaRequest.Presencas), "Um ou mais alunos não estão matriculados nesta turma. Atualize a lista e tente de novo.");

        var existentes = (await _presencaRepository.GetDaAulaAsync(turma.Id, disciplina.Id, data)).ToDictionary(p => p.AlunoId);

        // Aluno inativo: mantem o que ja havia, mas nao recebe registro novo nem alteracao.
        var inativos = request.Presencas
            .Where(p => !alunosDaTurma[p.AlunoId].Ativo
                && (!existentes.TryGetValue(p.AlunoId, out var atual) || atual.Presente != p.Presente))
            .Select(p => alunosDaTurma[p.AlunoId].Nome)
            .ToList();
        if (inativos.Count > 0)
            return Invalido(nameof(RegistrarChamadaRequest.Presencas), $"Aluno inativo não recebe presença: {string.Join(", ", inativos)}.");

        foreach (var item in request.Presencas)
        {
            if (existentes.TryGetValue(item.AlunoId, out var presenca))
            {
                presenca.Presente = item.Presente;
            }
            else
            {
                _presencaRepository.Adicionar(new Presenca
                {
                    TurmaId = turma.Id,
                    DisciplinaId = disciplina.Id,
                    AlunoId = item.AlunoId,
                    Data = data,
                    Presente = item.Presente
                });
            }
        }

        await _presencaRepository.SalvarAsync();

        return await ObterAsync(turma.Id, disciplina.Id, data);
    }

    /// <summary>
    /// Turma e disciplina, se existirem e o usuário puder fazer chamada nelas; nulo caso contrário
    /// (não revela a existência para quem não leciona).
    /// </summary>
    private async Task<(Turma Turma, Disciplina Disciplina)?> CarregarContextoAsync(int turmaId, int disciplinaId)
    {
        if (!await _acesso.PodeFazerChamadaAsync(turmaId, disciplinaId))
            return null;

        var turma = await _turmaRepository.GetEntityByIdAsync(turmaId);
        var disciplina = await _disciplinaRepository.GetEntityByIdAsync(disciplinaId);

        return turma is null || disciplina is null ? null : (turma, disciplina);
    }

    private string? ErroDeData(Turma turma, DateOnly data)
    {
        if (data > Hoje)
            return "Não é possível registrar chamada de uma data futura.";

        if (data.Year != turma.AnoLetivo)
            return $"A data precisa ser do ano letivo da turma ({turma.AnoLetivo}).";

        return null;
    }

    private static ResultadoOperacao<ChamadaDTO> Invalido(string campo, string mensagem)
        => ResultadoOperacao<ChamadaDTO>.Invalido(campo, mensagem);
}
