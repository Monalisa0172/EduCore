namespace EduCore.API.DTOs.Response;

/// <summary>
/// Resposta do login com o token JWT.
/// </summary>
/// <param name="Token">Token JWT de acesso.</param>
/// <param name="Type">Tipo do token (Bearer).</param>
/// <param name="ExpiresIn">Validade do token, em segundos.</param>
public record LoginResponseDTO(string Token, string Type, int ExpiresIn);
