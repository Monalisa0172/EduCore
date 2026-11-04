namespace EduCore.API.DTOs.Request;

public class CreateProfessorRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public decimal Salario { get; set; }
    public int TipoContrato { get; set; }
    public string Especialidade { get; set; } = string.Empty;
    public DateOnly DataAdmissao { get; set; }
}