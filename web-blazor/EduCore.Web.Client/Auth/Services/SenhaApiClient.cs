using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using EduCore.Web.Client.Api;

namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// "Esqueci minha senha": rotas públicas da API (sem token).
/// </summary>
public interface ISenhaApiClient
{
    /// <summary>Pede o link por e-mail. Sucesso não significa que a conta existe (a API responde igual).</summary>
    Task<ApiResult> SolicitarAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>O link ainda vale? Falha quando não foi possível consultar (sem conexão, muitas tentativas).</summary>
    Task<ApiResult<bool>> LinkValidoAsync(string token, CancellationToken cancellationToken = default);

    Task<ApiResult> RedefinirAsync(string token, string novaSenha, CancellationToken cancellationToken = default);
}

public sealed class SenhaApiClient(HttpClient httpClient) : ISenhaApiClient
{
    private const string Base = "api/auth";

    public Task<ApiResult> SolicitarAsync(string email, CancellationToken cancellationToken = default)
        => EnviarAsync(() => httpClient.PostAsJsonAsync($"{Base}/esqueci-senha", new { email = email.Trim() }, cancellationToken), cancellationToken);

    public async Task<ApiResult<bool>> LinkValidoAsync(string token, CancellationToken cancellationToken = default)
    {
        try
        {
            using var resposta = await httpClient.GetAsync($"{Base}/redefinir-senha/validar?token={Uri.EscapeDataString(token)}", cancellationToken);
            if (!resposta.IsSuccessStatusCode)
            {
                return ApiResult<bool>.Failure(await ErroAsync(resposta, cancellationToken));
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaValidacao>(cancellationToken);
            return ApiResult<bool>.Success(corpo?.Valido == true);
        }
        catch (HttpRequestException)
        {
            return ApiResult<bool>.Failure(ApiErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApiResult<bool>.Failure(ApiErrors.Timeout);
        }
    }

    public Task<ApiResult> RedefinirAsync(string token, string novaSenha, CancellationToken cancellationToken = default)
        => EnviarAsync(() => httpClient.PostAsJsonAsync($"{Base}/redefinir-senha", new { token, novaSenha }, cancellationToken), cancellationToken);

    private static async Task<ApiResult> EnviarAsync(Func<Task<HttpResponseMessage>> requisicao, CancellationToken cancellationToken)
    {
        try
        {
            using var resposta = await requisicao();
            return resposta.IsSuccessStatusCode
                ? ApiResult.Success()
                : ApiResult.Failure(await ErroAsync(resposta, cancellationToken));
        }
        catch (HttpRequestException)
        {
            return ApiResult.Failure(ApiErrors.Unavailable);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ApiResult.Failure(ApiErrors.Timeout);
        }
    }

    private static async Task<ApiError> ErroAsync(HttpResponseMessage resposta, CancellationToken cancellationToken)
    {
        if (resposta.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var segundos = resposta.Headers.RetryAfter?.Delta?.TotalSeconds;
            var espera = segundos is > 60
                ? $"cerca de {Math.Ceiling(segundos.Value / 60).ToString(CultureInfo.InvariantCulture)} minuto(s)"
                : segundos is > 0 ? $"{Math.Ceiling(segundos.Value).ToString(CultureInfo.InvariantCulture)} segundo(s)" : "alguns instantes";
            return new ApiError(ApiErrorKind.Unexpected, "Muitas tentativas", $"Por segurança, aguarde {espera} antes de tentar de novo.");
        }

        return await ApiErrors.FromResponseAsync(resposta, cancellationToken);
    }

    private sealed record RespostaValidacao(bool Valido);
}
