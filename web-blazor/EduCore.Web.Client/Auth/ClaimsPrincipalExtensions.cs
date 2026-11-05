using System.Security.Claims;

namespace EduCore.Web.Client.Auth;

/// <summary>
/// Dados de exibição do usuário autenticado. Tokens antigos (sem o claim de nome) caem no e-mail.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static string Email(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;

    /// <summary>Nome completo; se o token não trouxer o nome, devolve o e-mail.</summary>
    public static string NomeExibicao(this ClaimsPrincipal user)
    {
        var nome = user.FindFirst(ClaimTypes.Name)?.Value;
        return string.IsNullOrWhiteSpace(nome) ? user.Email() : nome.Trim();
    }

    /// <summary>Primeiro nome, para saudações ("Olá, Ana").</summary>
    public static string PrimeiroNome(this ClaimsPrincipal user)
    {
        var nome = user.NomeExibicao();
        if (nome.Contains('@'))
        {
            return nome;
        }

        var espaco = nome.IndexOf(' ');
        return espaco > 0 ? nome[..espaco] : nome;
    }
}
