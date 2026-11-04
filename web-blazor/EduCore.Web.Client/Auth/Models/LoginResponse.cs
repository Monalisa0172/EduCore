namespace EduCore.Web.Client.Auth.Models;

/// <summary>
/// Resposta da EduCore API em caso de sucesso (POST api/auth/login, HTTP 200).
/// </summary>
/// <param name="Token">JWT de acesso.</param>
/// <param name="Type">Tipo do token (Bearer).</param>
/// <param name="ExpiresIn">Validade do token, em segundos.</param>
public sealed record LoginResponse(
    string Token,
    string? Type,
    int ExpiresIn);
