namespace EduCore.Web.Client.Shared;

/// <summary>
/// "De onde vim e para onde volto" nas telas de usuário abertas a partir de outra página (ex.: uma turma).
/// </summary>
public static class RetornoNavegacao
{
    /// <summary>
    /// Aceita só caminhos da própria aplicação (evita open redirect) e devolve sem a barra inicial.
    /// </summary>
    public static string? CaminhoInterno(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)
            || !url.StartsWith('/')
            || url.StartsWith("//", StringComparison.Ordinal)
            || url.StartsWith("/\\", StringComparison.Ordinal)
            || !Uri.IsWellFormedUriString(url, UriKind.Relative))
        {
            return null;
        }

        return url.TrimStart('/');
    }

    /// <summary>
    /// Trilha "Início › Turmas › {turma} › {página}" quando a origem é uma turma; nulo nos demais casos.
    /// </summary>
    public static IReadOnlyList<ItemTrilha>? TrilhaDaTurma(string? voltar, string? origem, string paginaAtual)
    {
        var retorno = CaminhoInterno(voltar);
        if (retorno is null || !retorno.StartsWith("turmas/", StringComparison.Ordinal))
        {
            return null;
        }

        var nome = string.IsNullOrWhiteSpace(origem) ? "Turma" : origem.Trim();
        return [ItemTrilha.Inicio, new("Turmas", "turmas"), new(nome, retorno), new(paginaAtual)];
    }
}
