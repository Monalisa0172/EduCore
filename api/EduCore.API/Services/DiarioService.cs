using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Academico;
using EduCore.API.Services.Acesso;

namespace EduCore.API.Services;

/// <summary>
/// Diário de classe de uma disciplina na turma: o que as telas do professor precisam para abrir
/// (bimestre vigente, pesos por bimestre de toda a disciplina e o que o usuário leciona ali).
/// </summary>
public class DiarioService
{
    private const string NaoEncontrado = "Turma ou disciplina não encontrada.";

    private readonly IAcessoAcademico _acesso;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IDisciplinaRepository _disciplinaRepository;
    private readonly IAtribuicaoRepository _atribuicaoRepository;
    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly CalendarioLetivo _calendario;
    private readonly TimeProvider _relogio;

    public DiarioService(
        IAcessoAcademico acesso,
        ITurmaRepository turmaRepository,
        IDisciplinaRepository disciplinaRepository,
        IAtribuicaoRepository atribuicaoRepository,
        IAvaliacaoRepository avaliacaoRepository,
        CalendarioLetivo calendario,
        TimeProvider relogio)
    {
        _acesso = acesso;
        _turmaRepository = turmaRepository;
        _disciplinaRepository = disciplinaRepository;
        _atribuicaoRepository = atribuicaoRepository;
        _avaliacaoRepository = avaliacaoRepository;
        _calendario = calendario;
        _relogio = relogio;
    }

    public async Task<ResultadoOperacao<DiarioDTO>> ObterAsync(int turmaId, int disciplinaId)
    {
        // Quem nao leciona nada desta disciplina na turma recebe 404 (nao revela a existencia).
        if (!await _acesso.PodeFazerChamadaAsync(turmaId, disciplinaId))
            return ResultadoOperacao<DiarioDTO>.NaoEncontrado(NaoEncontrado);

        var turma = await _turmaRepository.GetEntityByIdAsync(turmaId);
        var disciplina = await _disciplinaRepository.GetEntityByIdAsync(disciplinaId);
        if (turma is null || disciplina is null)
            return ResultadoOperacao<DiarioDTO>.NaoEncontrado(NaoEncontrado);

        var professorId = await _acesso.ProfessorIdAsync();
        var atribuicoes = _acesso.EhAdmin || professorId is null
            ? await _atribuicaoRepository.GetAtivasDaDisciplinaAsync(turmaId, disciplinaId)
            : await _atribuicaoRepository.GetAtivasDoProfessorAsync(professorId.Value, turmaId, disciplinaId);

        var pesos = await _avaliacaoRepository.GetPesosPorBimestreAsync(turmaId, disciplinaId);
        var resumoTurma = await _turmaRepository.GetByIdAsync(turmaId);
        var hoje = DateOnly.FromDateTime(_relogio.GetLocalNow().DateTime);

        return ResultadoOperacao<DiarioDTO>.Ok(new DiarioDTO
        {
            TurmaId = turma.Id,
            TurmaNome = turma.Nome ?? string.Empty,
            AnoLetivo = turma.AnoLetivo,
            TurmaAtiva = turma.Ativo,
            AlunosAtivos = resumoTurma?.QuantidadeAlunos ?? 0,
            DisciplinaId = disciplina.Id,
            DisciplinaNome = disciplina.Nome ?? string.Empty,
            BimestreVigente = _calendario.BimestreVigente(turma.AnoLetivo, hoje),
            Bimestres = Enumerable.Range(1, CalendarioLetivo.TotalBimestres)
                .Select(b =>
                {
                    var total = pesos.GetValueOrDefault(b);
                    return new DiarioBimestreDTO
                    {
                        Bimestre = b,
                        PesoTotal = total,
                        PesoDisponivel = Math.Max(0, AvaliacaoService.PesoMaximo - total),
                        Encerrado = _calendario.BimestreEncerrado(turma.AnoLetivo, b, hoje)
                    };
                })
                .ToList(),
            Atribuicoes = atribuicoes
                .OrderBy(a => a.SubDisciplinaNome ?? string.Empty, StringComparer.CurrentCulture)
                .Select(a => new DiarioAtribuicaoDTO
                {
                    SubDisciplinaId = a.SubDisciplinaId,
                    SubDisciplinaNome = a.SubDisciplinaNome,
                    ProfessorNome = a.ProfessorNome
                })
                .ToList()
        });
    }
}
