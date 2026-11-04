namespace EduCore.Web.Client.Api;

/// <summary>
/// Configuração da EduCore API (seção "EduCoreApi" do appsettings).
/// </summary>
public sealed class EduCoreApiOptions
{
    public const string SectionName = "EduCoreApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string LoginPath { get; set; } = "api/auth/login";

    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Garante que credenciais e token só trafeguem por HTTPS (HTTP é aceito apenas em localhost).
    /// </summary>
    public void Validate()
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException(
                $"Configuração '{SectionName}:BaseUrl' ausente ou inválida.");
        }

        if (uri.Scheme != Uri.UriSchemeHttps && !uri.IsLoopback)
        {
            throw new InvalidOperationException(
                $"'{SectionName}:BaseUrl' deve usar HTTPS para não expor credenciais.");
        }

        if (string.IsNullOrWhiteSpace(LoginPath))
        {
            throw new InvalidOperationException($"Configuração '{SectionName}:LoginPath' não informada.");
        }

        if (TimeoutSeconds is < 5 or > 120)
        {
            throw new InvalidOperationException(
                $"'{SectionName}:TimeoutSeconds' deve estar entre 5 e 120.");
        }
    }
}
