using System.Diagnostics.CodeAnalysis;

namespace EduCore.Web.Client.Api;

public enum ApiErrorKind
{
    Validation,
    SessionExpired,
    Forbidden,
    NotFound,
    Conflict,
    ServerError,
    Unavailable,
    Timeout,
    InvalidResponse,
    Unexpected
}

/// <summary>
/// Erro já traduzido para exibição ao usuário.
/// </summary>
public sealed record ApiError(
    ApiErrorKind Kind,
    string Title,
    string Message,
    IReadOnlyDictionary<string, string[]>? FieldErrors = null);

/// <summary>
/// Resultado de uma chamada à API sem retorno de dados (ex.: inativar).
/// As telas tratam o erro como valor, sem try/catch espalhado.
/// </summary>
public class ApiResult
{
    protected ApiResult(ApiError? error) => Error = error;

    public ApiError? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool Succeeded => Error is null;

    public static ApiResult Success() => new(null);

    public static ApiResult Failure(ApiError error) => new(error);
}

/// <summary>
/// Resultado de uma chamada à API que retorna dados.
/// </summary>
public sealed class ApiResult<T> : ApiResult
{
    private ApiResult(T? value, ApiError? error) : base(error) => Value = value;

    public T? Value { get; }

    public static ApiResult<T> Success(T value) => new(value, null);

    public static new ApiResult<T> Failure(ApiError error) => new(default, error);
}
