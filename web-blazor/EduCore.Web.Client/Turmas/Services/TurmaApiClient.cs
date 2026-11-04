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
    public async Task<ApiResult<IReadOnlyList<TurmaOpcao>>> GetAtivasAsync(CancellationToken cancellationToken = default)
    {
        // A API devolve todas as turmas; so as ativas aceitam matricula.
        var resultado = await GetAsync<IReadOnlyList<TurmaOpcao>>("api/turma", cancellationToken);
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
}
