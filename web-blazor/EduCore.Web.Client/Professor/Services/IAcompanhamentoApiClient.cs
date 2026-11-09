using EduCore.Web.Client.Api;
using EduCore.Web.Client.Auth.Services;
using EduCore.Web.Client.Professor.Models;

namespace EduCore.Web.Client.Professor.Services;

/// <summary>Acompanhamento do professor pela coordenação (admin).</summary>
public interface IAcompanhamentoApiClient
{
    Task<ApiResult<Acompanhamento>> ObterAsync(int usuarioId, CancellationToken cancellationToken = default);
}

public sealed class AcompanhamentoApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IAcompanhamentoApiClient
{
    public Task<ApiResult<Acompanhamento>> ObterAsync(int usuarioId, CancellationToken cancellationToken = default)
        => GetAsync<Acompanhamento>($"api/usuarios/{usuarioId}/acompanhamento", cancellationToken);
}
