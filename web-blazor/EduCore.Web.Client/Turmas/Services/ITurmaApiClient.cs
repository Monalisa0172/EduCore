using EduCore.Web.Client.Api;
using EduCore.Web.Client.Turmas.Models;

namespace EduCore.Web.Client.Turmas.Services;

public interface ITurmaApiClient
{
    /// <summary>
    /// Turmas ativas, da mais recente para a mais antiga e em ordem alfabética.
    /// </summary>
    Task<ApiResult<IReadOnlyList<TurmaOpcao>>> GetAtivasAsync(CancellationToken cancellationToken = default);
}
