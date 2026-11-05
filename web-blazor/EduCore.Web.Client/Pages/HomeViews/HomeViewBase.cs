using System.Security.Claims;
using EduCore.Web.Client.Auth;
using Microsoft.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.HomeViews;

/// <summary>
/// Base das páginas iniciais por perfil: recebe o usuário autenticado já resolvido pela Home.
/// </summary>
public abstract class HomeViewBase : ComponentBase
{
    [Parameter, EditorRequired]
    public ClaimsPrincipal User { get; set; } = default!;

    /// <summary>Primeiro nome para a saudação (cai no e-mail em tokens sem nome).</summary>
    protected string PrimeiroNome => User.PrimeiroNome();
}
