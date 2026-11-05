using EduCore.Web.Client.Api;
using EduCore.Web.Client.Auth.Services;
using EduCore.Web.Client.Turmas.Models;

namespace EduCore.Web.Client.Turmas.Services;

public sealed class TurmaApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), ITurmaApiClient
{
    private const string BasePath = "api/turmas";

    public async Task<ApiResult<IReadOnlyList<TurmaOpcao>>> GetAtivasAsync(CancellationToken cancellationToken = default)
    {
        // A API devolve todas as turmas; so as ativas aceitam matricula.
        var resultado = await GetAsync<IReadOnlyList<TurmaOpcao>>(BasePath, cancellationToken);
        if (!resultado.Succeeded)
        {
            return resultado;
        }

        IReadOnlyList<TurmaOpcao> ativas = resultado.Value!
            .Where(t => t.Ativo)
            .OrderByDescending(t => t.AnoLetivo)
            .ThenBy(t => t.Nome, StringComparer.CurrentCulture)
            .ToList();

        return ApiResult<IReadOnlyList<TurmaOpcao>>.Success(ativas);
    }

    public Task<ApiResult<IReadOnlyList<TurmaItem>>> ListarAsync(CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<TurmaItem>>(BasePath, cancellationToken);

    public Task<ApiResult<TurmaItem>> ObterAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<TurmaItem>($"{BasePath}/{id}", cancellationToken);

    public Task<ApiResult<IReadOnlyList<TurmaAluno>>> ListarAlunosAsync(int id, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<TurmaAluno>>($"{BasePath}/{id}/alunos", cancellationToken);

    public Task<ApiResult<TurmaItem>> CriarAsync(SalvarTurmaRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarTurmaRequest, TurmaItem>(HttpMethod.Post, BasePath, request, cancellationToken);

    public Task<ApiResult<TurmaItem>> AtualizarAsync(int id, SalvarTurmaRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarTurmaRequest, TurmaItem>(HttpMethod.Put, $"{BasePath}/{id}", request, cancellationToken);

    public Task<ApiResult<TurmaItem>> InativarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<TurmaItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/inativar"), cancellationToken);

    public Task<ApiResult<TurmaItem>> ReativarAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<TurmaItem>(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{id}/reativar"), cancellationToken);
}
