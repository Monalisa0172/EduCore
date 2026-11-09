using System.ComponentModel.DataAnnotations;

namespace EduCore.Web.Client.Auth.Models;

/// <summary>Formulário "Esqueci minha senha".</summary>
public sealed class EsqueciSenhaModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido. Exemplo: nome@escola.com")]
    [MaxLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;
}

/// <summary>Formulário "Nova senha". A regra é a mesma do cadastro (8 a 100 caracteres).</summary>
public sealed class NovaSenhaModel
{
    [Required(ErrorMessage = "Informe a nova senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    public string Senha { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repita a nova senha.")]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não são iguais.")]
    public string Confirmacao { get; set; } = string.Empty;
}
