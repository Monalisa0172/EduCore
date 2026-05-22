namespace EduCore.API.DTOs.Response;

public class SubDisciplinaResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int DisciplinaId { get; set; }
    public string Disciplina { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}