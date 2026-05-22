namespace EduCore.API.DTOs.Request;

public class CreateDisciplinaRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? CargaHoraria { get; set; }
}