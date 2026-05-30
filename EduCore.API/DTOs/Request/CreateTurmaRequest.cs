namespace EduCore.API.DTOs.Request;

public class CreateTurmaRequest
{
    public string Nome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }
}