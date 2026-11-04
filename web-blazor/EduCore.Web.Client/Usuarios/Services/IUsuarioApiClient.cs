using EduCore.Web.Client.Api;
using EduCore.Web.Client.Usuarios.Models;

namespace EduCore.Web.Client.Usuarios.Services;

public interface IUsuarioApiClient
{
    Task<ApiResult<UsuariosResumo>> GetResumoAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<IReadOnlyList<UsuarioListItem>>> GetAtivosAsync(
        TipoUsuarioFiltro filtro,
        CancellationToken cancellationToken = default);

    Task<ApiResult<UsuarioDetalhe>> GetByIdAsync(int usuarioId, CancellationToken cancellationToken = default);

    Task<ApiResult<UsuarioDetalhe>> CriarAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<UsuarioDetalhe>> AtualizarAsync(
        int usuarioId,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResult> InativarAsync(int usuarioId, CancellationToken cancellationToken = default);
}
