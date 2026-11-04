using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace EduCore.Web.Client.Api;

/// <summary>
/// Traduz falhas de comunicação e respostas HTTP de erro em mensagens claras para o usuário.
/// </summary>
public static class ApiErrors
{
    public static readonly ApiError SessionExpired = new(
        ApiErrorKind.SessionExpired,
        "Sessão expirada",
        "Sua sessão expirou. Entre novamente para continuar.");

    public static readonly ApiError Unavailable = new(
        ApiErrorKind.Unavailable,
        "Sem conexão com o servidor",
        "Não foi possível se comunicar com o servidor. Verifique sua conexão e tente novamente.");

    public static readonly ApiError Timeout = new(
        ApiErrorKind.Timeout,
        "O servidor demorou para responder",
        "A solicitação excedeu o tempo limite. Tente novamente em alguns instantes.");

    public static readonly ApiError InvalidResponse = new(
        ApiErrorKind.InvalidResponse,
        "Resposta inesperada do servidor",
        "O servidor retornou dados que não puderam ser lidos. Se o problema persistir, procure o suporte.");

    public static async Task<ApiError> FromResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var problem = await TryReadProblemAsync(response, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => new ApiError(
                ApiErrorKind.Validation,
                "Dados inválidos",
                problem?.UserMessage ?? "Alguns dados enviados não são válidos. Revise e tente novamente.",
                problem?.Errors),

            HttpStatusCode.Unauthorized => SessionExpired,

            HttpStatusCode.Forbidden => new ApiError(
                ApiErrorKind.Forbidden,
                "Acesso negado",
                "Seu perfil não tem permissão para realizar esta ação."),

            HttpStatusCode.NotFound => new ApiError(
                ApiErrorKind.NotFound,
                "Registro não encontrado",
                problem?.UserMessage ?? "O registro não existe mais. Atualize a página e tente novamente."),

            HttpStatusCode.Conflict => new ApiError(
                ApiErrorKind.Conflict,
                "Conflito de dados",
                problem?.UserMessage ?? "O registro foi alterado por outra pessoa. Atualize a página e tente novamente.",
                problem?.Errors),

            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => Timeout,

            >= HttpStatusCode.InternalServerError => new ApiError(
                ApiErrorKind.ServerError,
                "Serviço indisponível",
                "O servidor está com instabilidade. Tente novamente em alguns instantes."),

            _ => new ApiError(
                ApiErrorKind.Unexpected,
                "Não foi possível concluir a operação",
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Ocorreu um erro inesperado (código {0}). Se o problema persistir, procure o suporte.",
                    (int)response.StatusCode))
        };
    }

    public static async Task<ApiProblem?> TryReadProblemAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;
        if (mediaType is null || !mediaType.Contains("json", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<ApiProblem>(cancellationToken);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
