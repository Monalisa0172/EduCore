using System.Security.Claims;
using Microsoft.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.HomeViews;

/// <summary>
/// Base das páginas iniciais por perfil: recebe o usuário autenticado já resolvido pela Home.
/// </summary>
public abstract class HomeViewBase : ComponentBase
{
    [Parameter, EditorRequired]
    public ClaimsPrincipal User { get; set; } = default!;

    protected string UserEmail => User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
}
