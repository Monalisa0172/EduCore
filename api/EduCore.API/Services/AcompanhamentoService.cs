using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Academico;

namespace EduCore.API.Services;

/// <summary>
/// Acompanhamento do professor pela coordenação: pendências de lançamento no bimestre atual,
/// desempenho de cada turma e a projeção para reforço e reposição. Usa o mesmo cálculo do boletim
/// (CalculadoraBoletim), para os números baterem com o que o aluno vê.
/// </summary>
public class AcompanhamentoService
{
    private readonly IAcompanhamentoRepository _acompanhamentoRepository;
    private readonly IAtribuicaoRepository _atribuicaoRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly CalendarioLetivo _calendario;
    private readonly TimeProvider _relogio;

    public AcompanhamentoService(
        IAcompanhamentoRepository acompanhamentoRepository,
        IAtribuicaoRepository atribuicaoRepository,
        ITurmaRepository turmaRepository,
        CalendarioLetivo calendario,
        TimeProvider relogio)
    {
        _acompanhamentoRepository = acompanhamentoRepository;
        _atribuicaoRepository = atribuicaoRepository;
        _turmaRepository = turmaRepository;
        _calendario = calendario;
        _relogio = relogio;
    }

    public async Task<ResultadoOperacao<AcompanhamentoDTO>> ObterPorUsuarioAsync(int usuarioId)
    {
        var professorId = await _acompanhamentoRepository.GetProfessorIdPorUsuarioAsync(usuarioId);
        if (professorId is null)
            return ResultadoOperacao<AcompanhamentoDTO>.NaoEncontrado("Professor não encontrado.");

        var hoje = DateOnly.FromDateTime(_relogio.GetLocalNow().DateTime);
        var atribuicoes = await _atribuicaoRepository.GetAtivasDoProfessorAsync(professorId.Value);

        var itens = new List<AcompanhamentoItemDTO>();

        // Uma linha por turma + disciplina (subdisciplinas do professor agrupadas); so turmas ativas.
        foreach (var grupo in atribuicoes.Where(a => a.TurmaAtiva).GroupBy(a => (a.TurmaId, a.DisciplinaId)))
        {
            var primeira = grupo.First();
            var subIds = grupo.Where(a => a.SubDisciplinaId.HasValue).Select(a => a.SubDisciplinaId!.Value).ToHashSet();
            var inteira = grupo.Any(a => a.SubDisciplinaId is null);

            var alunos = (await _turmaRepository.GetAlunosAsync(primeira.TurmaId)).Where(a => a.Ativo).ToList();
            var avaliacoes = await _acompanhamentoRepository.GetAvaliacoesAsync(primeira.TurmaId, primeira.DisciplinaId);
            var presencas = await _acompanhamentoRepository.GetPresencasAsync(primeira.TurmaId, primeira.DisciplinaId);

            itens.Add(Montar(primeira, inteira ? [] : grupo.Select(a => a.SubDisciplinaNome!).ToList(),
                inteira ? null : subIds, alunos, avaliacoes, presencas, hoje));
        }

        return ResultadoOperacao<AcompanhamentoDTO>.Ok(new AcompanhamentoDTO
        {
            ProfessorId = professorId.Value,
            Itens = itens
                .OrderByDescending(i => i.AnoLetivo)
                .ThenBy(i => i.TurmaNome, StringComparer.CurrentCulture)
                .ThenBy(i => i.DisciplinaNome, StringComparer.CurrentCulture)
                .ToList()
        });
    }

    private AcompanhamentoItemDTO Montar(
        AtribuicaoResponseDTO atribuicao,
        List<string> subDisciplinas,
        HashSet<int>? subIdsDoProfessor,
        List<TurmaAlunoDTO> alunos,
        List<AvaliacaoDaTurma> avaliacoes,
        List<PresencaDaTurma> presencas,
        DateOnly hoje)
    {
        var ano = atribuicao.AnoLetivo;
        var vigente = _calendario.BimestreVigente(ano, hoje);
        var idsAtivos = alunos.Select(a => a.AlunoId).ToHashSet();

        // Pendencias: so as avaliacoes do que o professor leciona; ja aplicadas e sem nota de todos.
        var doProfessor = avaliacoes.Where(a => subIdsDoProfessor is null || (a.SubDisciplinaId is { } s && subIdsDoProfessor.Contains(s)));
        var pendentes = doProfessor
            .Where(a => DateOnly.FromDateTime(a.DataAplicacao) <= hoje)
            .Select(a => new AvaliacaoPendenteDTO
            {
                Id = a.Id,
                Nome = a.Nome,
                DataAplicacao = a.DataAplicacao,
                Bimestre = a.Bimestre,
                NotasLancadas = a.Notas.Count(n => idsAtivos.Contains(n.AlunoId))
            })
            .Where(p => p.NotasLancadas < alunos.Count)
            .OrderBy(p => p.DataAplicacao)
            .ToList();

        var ultimaChamada = presencas.Count == 0 ? (DateOnly?)null : presencas.Max(p => p.Data);

        // Desempenho: o mesmo calculo do boletim, aluno a aluno.
        var disciplina = new DisciplinaDoBoletim(atribuicao.DisciplinaId, atribuicao.DisciplinaNome);
        var desempenhos = alunos
            .Select(aluno =>
            {
                var dados = new DadosDoBoletim(
                    [disciplina],
                    avaliacoes.Select(a => new AvaliacaoDoAluno(
                        a.Id, disciplina.Id, a.Nome, a.SubDisciplinaNome, a.Peso, a.DataAplicacao, a.Bimestre,
                        a.Notas.FirstOrDefault(n => n.AlunoId == aluno.AlunoId)?.Valor)).ToList(),
                    presencas.Where(p => p.AlunoId == aluno.AlunoId).Select(p => new PresencaDoAluno(disciplina.Id, p.Data, p.Presente)).ToList());

                return (Aluno: aluno, Boletim: CalculadoraBoletim.Calcular(dados, ano, _calendario, hoje).Single());
            })
            .ToList();

        var medias = desempenhos.Where(d => d.Boletim.MediaAnual.HasValue).Select(d => d.Boletim.MediaAnual!.Value).ToList();
        var registros = presencas.Where(p => idsAtivos.Contains(p.AlunoId)).ToList();

        return new AcompanhamentoItemDTO
        {
            TurmaId = atribuicao.TurmaId,
            TurmaNome = atribuicao.TurmaNome,
            AnoLetivo = ano,
            DisciplinaId = atribuicao.DisciplinaId,
            DisciplinaNome = atribuicao.DisciplinaNome,
            SubDisciplinas = subDisciplinas.OrderBy(s => s, StringComparer.CurrentCulture).ToList(),
            BimestreVigente = vigente,
            AnoEncerrado = _calendario.AnoEncerrado(ano, hoje),
            AlunosAtivos = alunos.Count,
            PesoTotalBimestre = avaliacoes.Where(a => a.Bimestre == vigente).Sum(a => a.Peso),
            AvaliacoesPendentes = pendentes,
            UltimaChamada = ultimaChamada,
            DiasSemChamada = ultimaChamada is { } u ? hoje.DayNumber - u.DayNumber : null,
            AulasNoBimestre = presencas.Select(p => p.Data).Distinct().Count(d => _calendario.BimestreDaData(d) == vigente && d.Year == ano),
            MediaTurma = medias.Count == 0 ? null : Math.Round(medias.Average(), 2, MidpointRounding.AwayFromZero),
            FrequenciaTurma = registros.Count == 0 ? null : Math.Round(registros.Count(p => p.Presente) * 100m / registros.Count, 1, MidpointRounding.AwayFromZero),
            AlunosEmRisco = desempenhos.Count(d => d.Boletim.Nivel == NivelDesempenho.Risco),
            AlunosEmAtencao = desempenhos.Count(d => d.Boletim.Nivel == NivelDesempenho.Atencao),
            AlunosEmAlerta = desempenhos
                .Where(d => d.Boletim.Nivel is NivelDesempenho.Risco or NivelDesempenho.Atencao)
                .Select(d => ParaAluno(d.Aluno, d.Boletim))
                .OrderByDescending(a => a.Nivel)
                .ThenBy(a => a.Nome, StringComparer.CurrentCulture)
                .ToList()
        };
    }

    private static AlunoAcompanhamentoDTO ParaAluno(TurmaAlunoDTO aluno, BoletimDisciplinaDTO boletim)
    {
        var (projecao, nota) = ProjecaoAprovacao.NotaNecessaria(
            boletim.Bimestres.Where(b => b.Bimestre < CalendarioLetivo.TotalBimestres).Select(b => b.Media).ToList(),
            CriteriosAprovacao.MediaMinima);

        return new AlunoAcompanhamentoDTO
        {
            AlunoId = aluno.AlunoId,
            UsuarioId = aluno.UsuarioId,
            Nome = aluno.Nome,
            MediaAnual = boletim.MediaAnual,
            MediaAnualParcial = boletim.MediaAnualParcial,
            Frequencia = boletim.Frequencia,
            Aulas = boletim.Aulas,
            Faltas = boletim.Faltas,
            NivelMedia = CriteriosAprovacao.NivelDaMedia(boletim.MediaAnual),
            NivelFrequencia = CriteriosAprovacao.NivelDaFrequencia(boletim.Frequencia),
            Nivel = boletim.Nivel,
            ProjecaoNota = projecao,
            NotaNecessaria = nota,
            AulasParaRepor = ProjecaoAprovacao.AulasParaRepor(boletim.Aulas, boletim.Faltas, CriteriosAprovacao.FrequenciaMinima)
        };
    }
}
