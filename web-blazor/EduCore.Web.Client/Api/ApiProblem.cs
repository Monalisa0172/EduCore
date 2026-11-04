namespace EduCore.Web.Client.Api;

/// <summary>
/// Corpo de erro retornado pela API: ProblemDetails (RFC 9457) ou o formato { message } usado em alguns endpoints.
/// </summary>
public sealed class ApiProblem
{
    public string? Title { get; set; }

    public string? Detail { get; set; }

    public string? Message { get; set; }

    public int? Status { get; set; }

    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Texto mais específico disponível para exibir ao usuário.
    /// </summary>
    public string? UserMessage => !string.IsNullOrWhiteSpace(Detail) ? Detail : Message;
}
