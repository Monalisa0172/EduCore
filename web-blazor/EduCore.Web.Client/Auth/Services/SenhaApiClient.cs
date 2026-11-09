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
                return ApiResult<bool>.Failure(await ApiErrors.FromResponseAsync(resposta, cancellationToken));
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
                : ApiResult.Failure(await ApiErrors.FromResponseAsync(resposta, cancellationToken));
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

    private sealed record RespostaValidacao(bool Valido);
}
