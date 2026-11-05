using EduCore.Web.Client.Api;
using EduCore.Web.Client.Disciplinas.Models;

namespace EduCore.Web.Client.Disciplinas.Services;

public interface IDisciplinaApiClient
{
    Task<ApiResult<IReadOnlyList<DisciplinaItem>>> ListarAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<DisciplinaItem>> ObterAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<DisciplinaItem>> CriarAsync(SalvarDisciplinaRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<DisciplinaItem>> AtualizarAsync(int id, SalvarDisciplinaRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<DisciplinaItem>> InativarAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<DisciplinaItem>> ReativarAsync(int id, CancellationToken cancellationToken = default);
}

public interface ISubDisciplinaApiClient
{
    Task<ApiResult<IReadOnlyList<SubDisciplinaItem>>> ListarPorDisciplinaAsync(int disciplinaId, CancellationToken cancellationToken = default);

    Task<ApiResult<SubDisciplinaItem>> CriarAsync(SalvarSubDisciplinaRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<SubDisciplinaItem>> AtualizarAsync(int id, SalvarSubDisciplinaRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<SubDisciplinaItem>> InativarAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<SubDisciplinaItem>> ReativarAsync(int id, CancellationToken cancellationToken = default);
}
