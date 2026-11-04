namespace EduCore.Web.Client.Auth.Models;

/// <summary>
/// Token de acesso persistido no cliente após o login.
/// </summary>
public sealed record AuthToken(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt)
{
    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;
}
