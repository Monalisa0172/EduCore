namespace EduCore.API.DTOs.Response;

/// <summary>
/// Cabeçalho do diário de classe (turma + disciplina): bimestre vigente, pesos já usados em cada
/// bimestre e o que o usuário leciona ali. Base das telas de avaliações, notas e chamada.
/// </summary>
public class DiarioDTO
{
    public int TurmaId { get; set; }

    public string TurmaNome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }

    /// <summary>Turma inativa = histórico: só consulta.</summary>
    public bool TurmaAtiva { get; set; }

    /// <summary>Alunos ativos matriculados (para "5 de 8 notas lançadas").</summary>
    public int AlunosAtivos { get; set; }

    public int DisciplinaId { get; set; }

    public string DisciplinaNome { get; set; } = string.Empty;

    public int BimestreVigente { get; set; }

    /// <summary>Sempre os 4 bimestres, em ordem.</summary>
    public List<DiarioBimestreDTO> Bimestres { get; set; } = [];

    /// <summary>
    /// O que o usuário leciona nesta disciplina da turma (para o admin, todas as atribuições).
    /// Item sem subdisciplina = disciplina inteira.
    /// </summary>
    public List<DiarioAtribuicaoDTO> Atribuicoes { get; set; } = [];
}

public class DiarioBimestreDTO
{
    public int Bimestre { get; set; }

    /// <summary>Soma dos pesos ativos de toda a disciplina no bimestre (todos os professores).</summary>
    public decimal PesoTotal { get; set; }

    /// <summary>Quanto ainda cabe até 100.</summary>
    public decimal PesoDisponivel { get; set; }

    public bool Encerrado { get; set; }
}

public class DiarioAtribuicaoDTO
{
    public int? SubDisciplinaId { get; set; }

    public string? SubDisciplinaNome { get; set; }

    public string ProfessorNome { get; set; } = string.Empty;
}
