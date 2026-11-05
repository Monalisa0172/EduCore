namespace EduCore.API.DTOs.Response;

/// <summary>
/// Chamada de uma aula: todos os alunos da turma, com a presença de cada um (vazia se ainda não registrada).
/// </summary>
public class ChamadaDTO
{
    public int TurmaId { get; set; }

    public string TurmaNome { get; set; } = string.Empty;

    public int DisciplinaId { get; set; }

    public string DisciplinaNome { get; set; } = string.Empty;

    public DateOnly Data { get; set; }

    public int Bimestre { get; set; }

    /// <summary>Já existe chamada registrada para este dia.</summary>
    public bool Registrada { get; set; }

    /// <summary>Falso para turma inativa ou data futura.</summary>
    public bool PodeRegistrar { get; set; }

    public int Presentes { get; set; }

    public List<ChamadaAlunoDTO> Alunos { get; set; } = [];
}

public class ChamadaAlunoDTO
{
    public int AlunoId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    /// <summary>Nulo enquanto a chamada do dia não foi registrada para o aluno.</summary>
    public bool? Presente { get; set; }
}

/// <summary>
/// Aula com chamada registrada (histórico do professor).
/// </summary>
public class AulaRegistradaDTO
{
    public DateOnly Data { get; set; }

    public int Bimestre { get; set; }

    public int Presentes { get; set; }

    public int Total { get; set; }
}
