using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

public class EsqueciSenhaRequest
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;
}

public class RedefinirSenhaRequest
{
    [Required(ErrorMessage = "Link de redefinição inválido.")]
    [StringLength(100, ErrorMessage = "Link de redefinição inválido.")]
    public string Token { get; set; } = string.Empty;

    // Mesma regra do cadastro de usuario.
    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    public string NovaSenha { get; set; } = string.Empty;
}
