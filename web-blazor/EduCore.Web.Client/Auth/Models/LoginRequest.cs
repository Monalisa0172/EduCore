using System.ComponentModel.DataAnnotations;

namespace EduCore.Web.Client.Auth.Models;

/// <summary>
/// Credenciais enviadas para a API de autenticação.
/// </summary>
public sealed class LoginRequest
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido. Exemplo: nome@empresa.com")]
    [MaxLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe sua senha.")]
    [MaxLength(128, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
