namespace EduCore.API.DTOs.Response;

public class TurmaResponseDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }

    public bool Ativo { get; set; }
}