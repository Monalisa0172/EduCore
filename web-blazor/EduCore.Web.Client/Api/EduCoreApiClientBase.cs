using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EduCore.Web.Client.Auth.Services;

namespace EduCore.Web.Client.Api;

/// <summary>
/// Base dos clientes da EduCore API: anexa o JWT, encerra a sessão em 401 e converte falhas em <see cref="ApiResult"/>.
/// </summary>
/// <remarks>
/// O token é anexado aqui (e não num DelegatingHandler) de propósito: o IHttpClientFactory cria os handlers
/// em outro escopo de DI, então o handler receberia outra instância do provider de autenticação
/// e o aviso de "sessão encerrada" não chegaria à tela.
/// </remarks>
public abstract class EduCoreApiClientBase(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
{
    protected Task<ApiResult<T>> GetAsync<T>(string uri, CancellationToken cancellationToken)
        => SendAsync<T>(new HttpRequestMessage(HttpMethod.Get, uri), cancellationToken);

    protected Task<ApiResult<TResponse>> SendJsonAsync<TBody, TResponse>(
        HttpMethod method,
        string uri,
        TBody body,
        CancellationToken cancellationToken)
        => SendAsync<TResponse>(
            new HttpRequestMessage(method, uri) { Content = JsonContent.Create(body) },
            cancellationToken);

    protected async Task<ApiResult<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var (response, error) = await SendCoreAsync(request, cancellationToken);
        if (error is not null)
        {
            return ApiResult<T>.Failure(error);
        }

        using (response)
        {
            try
            {
                var value = await response!.Content.ReadFromJsonAsync<T>(cancellationToken);
                return value is null
                    ? ApiResult<T>.Failure(ApiErrors.InvalidResponse)
                    : ApiResult<T>.Success(value);
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
                return ApiResult<T>.Failure(ApiErrors.InvalidResponse);
            }
        }
    }

    protected async Task<ApiResult> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var (response, error) = await SendCoreAsync(request, cancellationToken);
        response?.Dispose();

        return error is null ? ApiResult.Success() : ApiResult.Failure(error);
    }

    private async Task<(HttpResponseMessage? Response, ApiError? Error)> SendCoreAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        using (request)
        {
            var token = await tokenStore.GetAsync(cancellationToken);
            if (token is null)
            {
                // Token ausente ou expirado: a tela reage ao aviso e redireciona para o login.
                authenticationNotifier.NotifyAuthenticationChanged();
                return (null, ApiErrors.SessionExpired);
            }

            request.Headers.Authorization = new AuthenticationHeaderValue(token.TokenType, token.AccessToken);

            try
            {
                var response = await httpClient.SendAsync(request, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return (response, null);
                }

                using (response)
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await tokenStore.ClearAsync(cancellationToken);
                        authenticationNotifier.NotifyAuthenticationChanged();
                    }

                    return (null, await ApiErrors.FromResponseAsync(response, cancellationToken));
                }
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return (null, ApiErrors.Timeout);
            }
            catch (HttpRequestException)
            {
                return (null, ApiErrors.Unavailable);
            }
        }
    }
}
