namespace EduCore.API.DTOs.Request;

public class CreateSubDisciplinaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int DisciplinaId { get; set; }
}