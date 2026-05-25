namespace EduCore.API.DTOs.Response;

public class ProfessorSubDisciplinaResponseDTO
{
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public string Professor { get; set; } = string.Empty;
    public int SubDisciplinaId { get; set; }
    public string SubDisciplina { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}