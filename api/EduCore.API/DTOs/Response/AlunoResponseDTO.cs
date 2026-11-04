namespace EduCore.API.DTOs.Aluno;

public class AlunoResponseDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}