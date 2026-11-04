namespace EduCore.API.DTOs.Response;

public class ProfessorTurmaResponseDTO
{
    public int Id { get; set; }

    public int ProfessorId { get; set; }

    public string Professor { get; set; } = string.Empty;

    public int TurmaId { get; set; }

    public string Turma { get; set; } = string.Empty;

    public bool Ativo { get; set; }
}