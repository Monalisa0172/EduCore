namespace EduCore.API.DTOs.Response;

public class ProfessorResponseDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int FuncionarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public decimal? Salario { get; set; }
    public string Especialidade { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public string Status { get; set; } = string.Empty;
}