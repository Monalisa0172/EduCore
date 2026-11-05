using EduCore.Web.Client.Api;
using EduCore.Web.Client.Auth.Services;
using EduCore.Web.Client.Disciplinas.Models;

namespace EduCore.Web.Client.Disciplinas.Services;

public sealed class DisciplinaApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IDisciplinaApiClient
{
    private const string BasePath = "api/disciplinas";

    public Task<ApiResult<IReadOnlyList<DisciplinaItem>>> ListarAsync(CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<DisciplinaItem>>(BasePath, cancellationToken);

    public Task<ApiResult<DisciplinaItem>> ObterAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<DisciplinaItem>($"{BasePath}/{id}", cancellationToken);

    public Task<ApiResult<DisciplinaItem>> CriarAsync(SalvarDisciplinaRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarDisciplinaRequest, DisciplinaItem>(HttpMethod.Post, BasePath, request, cancellationToken);

    public Task<ApiResult<DisciplinaItem>> AtualizarAsync(int id, SalvarDisciplinaRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarDisciplinaRequest, DisciplinaItem>(HttpMethod.Put, $"{BasePath}/{id}", request, cancellationToken);

    public Task<ApiResult<DisciplinaItem>> InativarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<DisciplinaItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/inativar"), cancellationToken);

    public Task<ApiResult<DisciplinaItem>> ReativarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<DisciplinaItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/reativar"), cancellationToken);
}

public sealed class SubDisciplinaApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), ISubDisciplinaApiClient
{
    private const string BasePath = "api/subdisciplinas";

    public Task<ApiResult<IReadOnlyList<SubDisciplinaItem>>> ListarPorDisciplinaAsync(int disciplinaId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<SubDisciplinaItem>>($"{BasePath}/disciplina/{disciplinaId}", cancellationToken);

    public Task<ApiResult<SubDisciplinaItem>> CriarAsync(SalvarSubDisciplinaRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarSubDisciplinaRequest, SubDisciplinaItem>(HttpMethod.Post, BasePath, request, cancellationToken);

    public Task<ApiResult<SubDisciplinaItem>> AtualizarAsync(int id, SalvarSubDisciplinaRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarSubDisciplinaRequest, SubDisciplinaItem>(HttpMethod.Put, $"{BasePath}/{id}", request, cancellationToken);

    public Task<ApiResult<SubDisciplinaItem>> InativarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<SubDisciplinaItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/inativar"), cancellationToken);

    public Task<ApiResult<SubDisciplinaItem>> ReativarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<SubDisciplinaItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/reativar"), cancellationToken);
}
