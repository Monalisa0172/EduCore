namespace EduCore.API.DTOs.Response;

public class DisciplinaResponseDTO
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? CargaHoraria { get; set; }
    public bool Ativo { get; set; }
}