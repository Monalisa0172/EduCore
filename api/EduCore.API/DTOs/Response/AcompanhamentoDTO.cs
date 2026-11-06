using EduCore.API.Services.Academico;

namespace EduCore.API.DTOs.Response;

/// <summary>
/// Acompanhamento do professor pela coordenação: uma linha por turma e disciplina que ele leciona
/// (turmas ativas), com as pendências do bimestre atual e o desempenho da turma.
/// </summary>
public class AcompanhamentoDTO
{
    public int ProfessorId { get; set; }

    public List<AcompanhamentoItemDTO> Itens { get; set; } = [];
}

public class AcompanhamentoItemDTO
{
    public int TurmaId { get; set; }

    public string TurmaNome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }

    public int DisciplinaId { get; set; }

    public string DisciplinaNome { get; set; } = string.Empty;

    /// <summary>Partes que o professor leciona (vazio = disciplina inteira).</summary>
    public List<string> SubDisciplinas { get; set; } = [];

    public int BimestreVigente { get; set; }

    public bool AnoEncerrado { get; set; }

    public int AlunosAtivos { get; set; }

    // ---------- Pendencias do bimestre atual ----------

    /// <summary>Soma dos pesos da disciplina no bimestre atual (todos os professores; fecha em 100).</summary>
    public decimal PesoTotalBimestre { get; set; }

    /// <summary>Avaliações já aplicadas (do que o professor leciona) sem nota de todos os alunos ativos.</summary>
    public List<AvaliacaoPendenteDTO> AvaliacoesPendentes { get; set; } = [];

    public DateOnly? UltimaChamada { get; set; }

    public int? DiasSemChamada { get; set; }

    public int AulasNoBimestre { get; set; }

    // ---------- Desempenho da turma (ano ate agora) ----------

    /// <summary>Média das médias anuais dos alunos ativos.</summary>
    public decimal? MediaTurma { get; set; }

    /// <summary>Frequência da turma: presenças ÷ registros de chamada dos alunos ativos.</summary>
    public decimal? FrequenciaTurma { get; set; }

    public int AlunosEmRisco { get; set; }

    public int AlunosEmAtencao { get; set; }

    /// <summary>Alunos em risco ou atenção, com a projeção de reforço (nota) e reposição (aulas).</summary>
    public List<AlunoAcompanhamentoDTO> AlunosEmAlerta { get; set; } = [];
}

public class AvaliacaoPendenteDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public DateTime DataAplicacao { get; set; }

    public int Bimestre { get; set; }

    public int NotasLancadas { get; set; }
}

public class AlunoAcompanhamentoDTO
{
    public int AlunoId { get; set; }

    /// <summary>Para abrir a ficha do aluno.</summary>
    public int UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal? MediaAnual { get; set; }

    public bool MediaAnualParcial { get; set; }

    public decimal? Frequencia { get; set; }

    public int Aulas { get; set; }

    public int Faltas { get; set; }

    public NivelDesempenho NivelMedia { get; set; }

    public NivelDesempenho NivelFrequencia { get; set; }

    public NivelDesempenho Nivel { get; set; }

    /// <summary>Nota mínima no 4º bimestre para a média anual chegar ao mínimo.</summary>
    public SituacaoProjecao ProjecaoNota { get; set; }

    public decimal? NotaNecessaria { get; set; }

    /// <summary>Aulas com presença (sem novas faltas) para a frequência voltar ao mínimo.</summary>
    public int AulasParaRepor { get; set; }
}
