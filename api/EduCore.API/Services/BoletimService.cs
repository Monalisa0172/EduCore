using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Academico;
using EduCore.API.Services.Acesso;

namespace EduCore.API.Services;

/// <summary>
/// Boletim por ano letivo: o aluno vê o próprio (/me); o admin, o de qualquer aluno.
/// </summary>
public class BoletimService
{
    private readonly IBoletimRepository _boletimRepository;
    private readonly IAcessoAcademico _acesso;
    private readonly CalendarioLetivo _calendario;
    private readonly TimeProvider _relogio;

    public BoletimService(
        IBoletimRepository boletimRepository,
        IAcessoAcademico acesso,
        CalendarioLetivo calendario,
        TimeProvider relogio)
    {
        _boletimRepository = boletimRepository;
        _acesso = acesso;
        _calendario = calendario;
        _relogio = relogio;
    }

    /// <summary>Boletim do aluno logado.</summary>
    public async Task<ResultadoOperacao<BoletimDTO>> MeuBoletimAsync(int? anoLetivo)
    {
        var alunoId = await _acesso.AlunoIdAsync();

        return alunoId is null
            ? ResultadoOperacao<BoletimDTO>.NaoEncontrado("Seu cadastro de aluno não foi encontrado. Fale com a secretaria.")
            : await ObterAsync(alunoId.Value, anoLetivo);
    }

    /// <summary>
    /// Boletim de um aluno. Sem ano informado, usa o ano letivo mais recente com matrícula.
    /// </summary>
    public async Task<ResultadoOperacao<BoletimDTO>> ObterAsync(int alunoId, int? anoLetivo)
    {
        var nome = await _boletimRepository.GetNomeDoAlunoAsync(alunoId);
        if (nome is null)
            return ResultadoOperacao<BoletimDTO>.NaoEncontrado("Aluno não encontrado.");

        var turmas = await _boletimRepository.GetTurmasDoAlunoAsync(alunoId);
        if (turmas.Count == 0)
            return ResultadoOperacao<BoletimDTO>.NaoEncontrado("O aluno ainda não está matriculado em nenhuma turma.");

        // Lista ja vem do ano mais recente e, no mesmo ano, a turma ativa primeiro.
        var turma = anoLetivo is null ? turmas[0] : turmas.FirstOrDefault(t => t.AnoLetivo == anoLetivo);
        if (turma is null)
            return ResultadoOperacao<BoletimDTO>.NaoEncontrado($"Não há matrícula no ano letivo {anoLetivo}.");

        var hoje = DateOnly.FromDateTime(_relogio.GetLocalNow().DateTime);
        var dados = await _boletimRepository.CarregarAsync(alunoId, turma.TurmaId);

        return ResultadoOperacao<BoletimDTO>.Ok(new BoletimDTO
        {
            AlunoId = alunoId,
            AlunoNome = nome,
            TurmaId = turma.TurmaId,
            TurmaNome = turma.Nome,
            AnoLetivo = turma.AnoLetivo,
            AnosLetivos = turmas.Select(t => t.AnoLetivo).Distinct().ToList(),
            BimestreVigente = _calendario.BimestreVigente(turma.AnoLetivo, hoje),
            AnoEncerrado = _calendario.AnoEncerrado(turma.AnoLetivo, hoje),
            Disciplinas = CalculadoraBoletim.Calcular(dados, turma.AnoLetivo, _calendario, hoje)
        });
    }
}
