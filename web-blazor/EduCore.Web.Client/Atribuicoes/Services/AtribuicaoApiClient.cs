using EduCore.Web.Client.Api;
using EduCore.Web.Client.Atribuicoes.Models;
using EduCore.Web.Client.Auth.Services;

namespace EduCore.Web.Client.Atribuicoes.Services;

public sealed class AtribuicaoApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IAtribuicaoApiClient
{
    private const string BasePath = "api/atribuicoes";

    public Task<ApiResult<IReadOnlyList<AtribuicaoItem>>> ListarDaTurmaAsync(int turmaId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<AtribuicaoItem>>($"{BasePath}?turmaId={turmaId}&ativo=true", cancellationToken);

    public Task<ApiResult<AtribuicaoItem>> CriarAsync(CriarAtribuicaoRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<CriarAtribuicaoRequest, AtribuicaoItem>(HttpMethod.Post, BasePath, request, cancellationToken);

    public Task<ApiResult<AtribuicaoItem>> TrocarProfessorAsync(int id, TrocarProfessorRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<TrocarProfessorRequest, AtribuicaoItem>(HttpMethod.Patch, $"{BasePath}/{id}/professor", request, cancellationToken);

    public Task<ApiResult<AtribuicaoItem>> EncerrarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<AtribuicaoItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/inativar"), cancellationToken);

    public async Task<ApiResult<IReadOnlyList<ProfessorOpcao>>> ListarProfessoresAtivosAsync(CancellationToken cancellationToken = default)
    {
        var resultado = await GetAsync<IReadOnlyList<ProfessorOpcao>>("api/professores", cancellationToken);
        if (!resultado.Succeeded)
        {
            return resultado;
        }

        IReadOnlyList<ProfessorOpcao> ativos = resultado.Value!
            .Where(p => p.Ativo)
            .OrderBy(p => p.Nome, StringComparer.CurrentCulture)
            .ToList();

        return ApiResult<IReadOnlyList<ProfessorOpcao>>.Success(ativos);
    }
}
