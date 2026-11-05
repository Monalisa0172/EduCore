using EduCore.Web.Client.Api;
using EduCore.Web.Client.Atribuicoes.Models;

namespace EduCore.Web.Client.Atribuicoes.Services;

public interface IAtribuicaoApiClient
{
    /// <summary>Atribuições ativas da turma.</summary>
    Task<ApiResult<IReadOnlyList<AtribuicaoItem>>> ListarDaTurmaAsync(int turmaId, CancellationToken cancellationToken = default);

    Task<ApiResult<AtribuicaoItem>> CriarAsync(CriarAtribuicaoRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<AtribuicaoItem>> TrocarProfessorAsync(int id, TrocarProfessorRequest request, CancellationToken cancellationToken = default);

    /// <summary>Encerra a atribuição (PATCH inativar); avaliações e notas ficam mantidas.</summary>
    Task<ApiResult<AtribuicaoItem>> EncerrarAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Professores ativos, em ordem alfabética.</summary>
    Task<ApiResult<IReadOnlyList<ProfessorOpcao>>> ListarProfessoresAtivosAsync(CancellationToken cancellationToken = default);
}
