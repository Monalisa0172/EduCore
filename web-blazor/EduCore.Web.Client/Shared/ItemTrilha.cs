namespace EduCore.Web.Client.Shared;

/// <summary>
/// Um nível da trilha de navegação (breadcrumb). Sem <paramref name="Href"/>, é a página atual.
/// </summary>
public sealed record ItemTrilha(string Texto, string? Href = null)
{
    public static readonly ItemTrilha Inicio = new("Início", "");
}
