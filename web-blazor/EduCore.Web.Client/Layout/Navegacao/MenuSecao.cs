namespace EduCore.Web.Client.Layout.Navegacao;

/// <summary>
/// Grupo de itens do menu. Sem título, os itens aparecem no topo, sem cabeçalho de seção.
/// </summary>
public sealed record MenuSecao(string? Titulo, IReadOnlyList<MenuItem> Itens);
