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

    /// <summary>
    /// Trilha conforme a origem: turma ("Início › Turmas › {turma} › {página}") ou ficha de aluno/professor
    /// ("Início › {pessoa} › {página}"); nulo quando não há origem reconhecida.
    /// </summary>
    public static IReadOnlyList<ItemTrilha>? TrilhaDeRetorno(string? voltar, string? origem, string paginaAtual)
        => TrilhaDeRetorno(voltar, origem, [new(paginaAtual)]);

    /// <summary>Igual à anterior, com mais de um nível depois da origem (ex.: diário › avaliação).</summary>
    public static IReadOnlyList<ItemTrilha>? TrilhaDeRetorno(string? voltar, string? origem, IReadOnlyList<ItemTrilha> depois)
    {
        var retorno = CaminhoInterno(voltar);
        if (retorno is not null
            && (retorno.StartsWith("alunos/", StringComparison.Ordinal) || retorno.StartsWith("professores/", StringComparison.Ordinal)))
        {
            var nome = string.IsNullOrWhiteSpace(origem) ? "Ficha" : origem.Trim();
            return [ItemTrilha.Inicio, new(nome, retorno), .. depois];
        }

        return depois.Count == 1 ? TrilhaDaTurma(voltar, origem, depois[0].Texto) : null;
    }

    /// <summary>Repete a origem num link (voltar/origem), para a trilha não se perder ao navegar.</summary>
    public static string ConsultaDeOrigem(string? voltar, string? origem)
        => CaminhoInterno(voltar) is null
            ? string.Empty
            : $"voltar={Uri.EscapeDataString(voltar!)}&origem={Uri.EscapeDataString(origem ?? string.Empty)}";
}
