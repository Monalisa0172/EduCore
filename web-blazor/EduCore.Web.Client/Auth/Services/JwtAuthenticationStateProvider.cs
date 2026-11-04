using System.Security.Claims;
using EduCore.Web.Client.Auth.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// Monta o usuário autenticado a partir do JWT guardado no <see cref="ITokenStore"/>.
/// </summary>
public sealed class JwtAuthenticationStateProvider(ITokenStore tokenStore)
    : AuthenticationStateProvider, IAuthenticationStateNotifier
{
    private const string AuthenticationType = "EduCoreJwt";

    private static readonly AuthenticationState Anonymous = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        AuthToken? token;

        try
        {
            token = await tokenStore.GetAsync();
        }
        catch (InvalidOperationException)
        {
            // Prerender no servidor: ainda nao ha acesso ao sessionStorage do navegador.
            return Anonymous;
        }

        if (token is null)
        {
            return Anonymous;
        }

        var claims = JwtClaimsParser.TryParse(token.AccessToken);
        if (claims is null)
        {
            await tokenStore.ClearAsync();
            return Anonymous;
        }

        var identity = new ClaimsIdentity(claims, AuthenticationType, ClaimTypes.Email, ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public void NotifyAuthenticationChanged()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
