namespace EduCore.API.DTOs.Response;

public class TurmaResponseDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }

    public bool Ativo { get; set; }

    /// <summary>Alunos matriculados com acesso ativo.</summary>
    public int QuantidadeAlunos { get; set; }
}

/// <summary>
/// Aluno matriculado numa turma (GET api/turmas/{id}/alunos).
/// </summary>
public class TurmaAlunoDTO
{
    public int AlunoId { get; set; }

    /// <summary>Usado pelo front para abrir o cadastro do aluno.</summary>
    public int UsuarioId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool Ativo { get; set; }
}
