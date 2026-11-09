using System.Net.Http.Json;
using EduCore.Web.Client.Api;

namespace EduCore.Web.Client.Auth.Services;

/// <summary>Dados da conta do usuário logado (rotas /api/me, com token).</summary>
public interface IMinhaContaApiClient
{
    Task<ApiResult> AlterarSenhaAsync(string senhaAtual, string novaSenha, CancellationToken cancellationToken = default);
}

public sealed class MinhaContaApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IMinhaContaApiClient
{
    public Task<ApiResult> AlterarSenhaAsync(string senhaAtual, string novaSenha, CancellationToken cancellationToken = default)
        => SendAsync(
            new HttpRequestMessage(HttpMethod.Put, "api/me/senha") { Content = JsonContent.Create(new { senhaAtual, novaSenha }) },
            cancellationToken);
}
