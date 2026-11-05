namespace EduCore.API.DTOs.Response;

public class AtribuicaoResponseDTO
{
    public int Id { get; set; }

    public int ProfessorId { get; set; }

    public string ProfessorNome { get; set; } = string.Empty;

    public int TurmaId { get; set; }

    public string TurmaNome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }

    public int DisciplinaId { get; set; }

    public string DisciplinaNome { get; set; } = string.Empty;

    public int? SubDisciplinaId { get; set; }

    public string? SubDisciplinaNome { get; set; }

    public bool Ativo { get; set; }
}
