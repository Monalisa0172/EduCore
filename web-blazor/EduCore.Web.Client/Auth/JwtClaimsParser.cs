using System.Security.Claims;
using System.Text.Json;

namespace EduCore.Web.Client.Auth;

/// <summary>
/// Lê os claims do payload de um JWT. Não valida assinatura: quem valida o token é a API,
/// aqui os claims servem apenas para adaptar a interface (perfil, e-mail).
/// </summary>
internal static class JwtClaimsParser
{
    // O JwtSecurityTokenHandler da API grava os claims com nomes curtos.
    private static readonly Dictionary<string, string> ShortNames = new(StringComparer.Ordinal)
    {
        ["role"] = ClaimTypes.Role,
        ["nameid"] = ClaimTypes.NameIdentifier,
        ["email"] = ClaimTypes.Email,
        ["unique_name"] = ClaimTypes.Name
    };

    public static IReadOnlyList<Claim>? TryParse(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length != 3)
        {
            return null;
        }

        try
        {
            using var payload = JsonDocument.Parse(Base64UrlDecode(parts[1]));
            var claims = new List<Claim>();

            foreach (var property in payload.RootElement.EnumerateObject())
            {
                var type = ShortNames.GetValueOrDefault(property.Name, property.Name);

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    claims.AddRange(property.Value.EnumerateArray().Select(item => new Claim(type, item.ToString())));
                }
                else
                {
                    claims.Add(new Claim(type, property.Value.ToString()));
                }
            }

            return claims;
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            return null;
        }
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
