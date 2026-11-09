namespace EduCore.Web.Client.Auth;

/// <summary>
/// Telas de entrada (login e redefinição de senha). Não fazem sentido como "voltar para" depois do login.
/// </summary>
public static class PaginasDeAcesso
{
    private static readonly string[] Caminhos = ["login", "esqueci-senha", "redefinir-senha"];

    /// <summary>
    /// <paramref name="caminhoRelativo"/> é relativo à base do app, com ou sem "/" inicial e query (ex.: "/login?x=1").
    /// </summary>
    public static bool Contem(string? caminhoRelativo)
    {
        if (string.IsNullOrWhiteSpace(caminhoRelativo))
        {
            return false;
        }

        var caminho = caminhoRelativo.Split('?', '#')[0].Trim('/');
        return Caminhos.Contains(caminho, StringComparer.OrdinalIgnoreCase);
    }
}
