using EduCore.Web.Client.Api;
using Microsoft.AspNetCore.Components.Forms;

namespace EduCore.Web.Client.Shared;

/// <summary>
/// Liga os erros por campo devolvidos pela API (ValidationProblemDetails) aos campos do formulário.
/// </summary>
public static class ErrosDeFormulario
{
    /// <summary>
    /// Coloca cada erro embaixo do campo correspondente e devolve o que deve aparecer no topo:
    /// nulo quando todos os erros ficaram nos campos, ou o erro geral (ex.: sem conexão).
    /// </summary>
    public static ApiError? Aplicar(
        ApiError erro,
        ValidationMessageStore mensagens,
        object modelo,
        IReadOnlyCollection<string> campos)
    {
        if (erro.FieldErrors is not { Count: > 0 } errosPorCampo)
        {
            return erro;
        }

        var semCampo = new List<string>();

        foreach (var (chave, textos) in errosPorCampo)
        {
            var campo = campos.FirstOrDefault(c => string.Equals(c, NomeDoCampo(chave), StringComparison.OrdinalIgnoreCase));

            if (campo is null)
            {
                semCampo.AddRange(textos);
            }
            else
            {
                mensagens.Add(new FieldIdentifier(modelo, campo), textos);
            }
        }

        return semCampo.Count == 0
            ? null
            : erro with { Title = "Não foi possível salvar", Message = string.Join(" ", semCampo) };
    }

    // A API pode devolver "Codigo", "codigo" ou "$.codigo" (erro de desserializacao).
    private static string NomeDoCampo(string chave)
        => chave.StartsWith("$.", StringComparison.Ordinal) ? chave[2..] : chave;
}
