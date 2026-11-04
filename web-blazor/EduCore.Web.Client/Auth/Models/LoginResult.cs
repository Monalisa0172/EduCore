namespace EduCore.Web.Client.Auth.Models;

public enum LoginFailureReason
{
    None,
    Validation,
    InvalidCredentials,
    AccessDenied,
    AccountLocked,
    TooManyAttempts,
    ServerError,
    Unavailable,
    Timeout,
    InvalidResponse,
    Unexpected
}

/// <summary>
/// Resultado da tentativa de login, já traduzido para mensagens exibíveis ao usuário.
/// </summary>
public sealed record LoginResult
{
    private static readonly IReadOnlyDictionary<string, string[]> NoFieldErrors =
        new Dictionary<string, string[]>();

    public bool Succeeded { get; private init; }

    public LoginFailureReason Reason { get; private init; }

    public string? Title { get; private init; }

    public string? Message { get; private init; }

    public IReadOnlyDictionary<string, string[]> FieldErrors { get; private init; } = NoFieldErrors;

    public static LoginResult Success() => new() { Succeeded = true };

    public static LoginResult Failure(
        LoginFailureReason reason,
        string title,
        string message,
        IReadOnlyDictionary<string, string[]>? fieldErrors = null) => new()
    {
        Reason = reason,
        Title = title,
        Message = message,
        FieldErrors = fieldErrors ?? NoFieldErrors
    };
}
