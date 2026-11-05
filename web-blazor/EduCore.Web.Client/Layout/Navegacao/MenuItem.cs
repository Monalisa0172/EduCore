using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;

namespace EduCore.Web.Client.Layout.Navegacao;

/// <summary>
/// Item do menu lateral. Telas ainda não implementadas ficam visíveis como "Em breve"
/// (sem link), para o menu nunca levar a uma rota inexistente.
/// </summary>
public sealed record MenuItem(
    string Titulo,
    string Href,
    Icon IconeRepouso,
    Icon IconeAtivo,
    bool Disponivel = true,
    NavLinkMatch Match = NavLinkMatch.Prefix);
