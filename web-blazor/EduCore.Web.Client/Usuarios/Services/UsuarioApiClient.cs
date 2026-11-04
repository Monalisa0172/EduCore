using EduCore.Web.Client.Api;
using EduCore.Web.Client.Auth.Services;
using EduCore.Web.Client.Usuarios.Models;

namespace EduCore.Web.Client.Usuarios.Services;

public sealed class UsuarioApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IUsuarioApiClient
{
    private const string BasePath = "api/usuarios";

    public Task<ApiResult<UsuariosResumo>> GetResumoAsync(CancellationToken cancellationToken = default)
        => GetAsync<UsuariosResumo>($"{BasePath}/resumo", cancellationToken);

    public Task<ApiResult<IReadOnlyList<UsuarioListItem>>> GetAtivosAsync(
        TipoUsuarioFiltro filtro,
        CancellationToken cancellationToken = default)
    {
        var query = filtro switch
        {
            TipoUsuarioFiltro.Alunos => "?tipo=Aluno",
            TipoUsuarioFiltro.Professores => "?tipo=Professor",
            _ => string.Empty
        };

        return GetAsync<IReadOnlyList<UsuarioListItem>>($"{BasePath}/ativos{query}", cancellationToken);
    }

    public Task<ApiResult<UsuarioDetalhe>> GetByIdAsync(int usuarioId, CancellationToken cancellationToken = default)
        => GetAsync<UsuarioDetalhe>($"{BasePath}/{usuarioId}", cancellationToken);

    public Task<ApiResult<UsuarioDetalhe>> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<CriarUsuarioRequest, UsuarioDetalhe>(HttpMethod.Post, BasePath, request, cancellationToken);

    public Task<ApiResult<UsuarioDetalhe>> AtualizarAsync(
        int usuarioId,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken = default)
        => SendJsonAsync<AtualizarUsuarioRequest, UsuarioDetalhe>(
            HttpMethod.Put, $"{BasePath}/{usuarioId}", request, cancellationToken);

    public Task<ApiResult> InativarAsync(int usuarioId, CancellationToken cancellationToken = default)
        => SendAsync(new HttpRequestMessage(HttpMethod.Patch, $"{BasePath}/{usuarioId}/inativar"), cancellationToken);
}
