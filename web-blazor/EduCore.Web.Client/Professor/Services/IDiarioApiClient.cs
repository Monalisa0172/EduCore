using EduCore.Web.Client.Api;
using EduCore.Web.Client.Atribuicoes.Models;
using EduCore.Web.Client.Professor.Models;

namespace EduCore.Web.Client.Professor.Services;

/// <summary>
/// Chamadas da API usadas nas telas do professor (diário de classe).
/// </summary>
public interface IDiarioApiClient
{
    /// <summary>Turmas e disciplinas que o professor logado leciona.</summary>
    Task<ApiResult<IReadOnlyList<AtribuicaoItem>>> MinhasAtribuicoesAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<DiarioInfo>> ObterDiarioAsync(int turmaId, int disciplinaId, CancellationToken cancellationToken = default);

    /// <summary>Avaliações (ativas e inativas) da disciplina na turma que o usuário pode ver.</summary>
    Task<ApiResult<IReadOnlyList<AvaliacaoItem>>> ListarAvaliacoesAsync(int turmaId, int disciplinaId, CancellationToken cancellationToken = default);

    Task<ApiResult<AvaliacaoItem>> CriarAvaliacaoAsync(SalvarAvaliacaoRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<AvaliacaoItem>> AtualizarAvaliacaoAsync(int id, SalvarAvaliacaoRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<AvaliacaoItem>> InativarAvaliacaoAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<AvaliacaoItem>> ReativarAvaliacaoAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<NotasDaAvaliacao>> ObterNotasAsync(int avaliacaoId, CancellationToken cancellationToken = default);

    Task<ApiResult<NotasDaAvaliacao>> LancarNotasAsync(int avaliacaoId, IReadOnlyList<NotaLancamento> notas, CancellationToken cancellationToken = default);

    Task<ApiResult<ChamadaInfo>> ObterChamadaAsync(int turmaId, int disciplinaId, DateOnly data, CancellationToken cancellationToken = default);

    Task<ApiResult<IReadOnlyList<AulaRegistrada>>> ListarAulasAsync(int turmaId, int disciplinaId, CancellationToken cancellationToken = default);

    Task<ApiResult<ChamadaInfo>> RegistrarChamadaAsync(int turmaId, int disciplinaId, DateOnly data, IReadOnlyList<PresencaLancamento> presencas, CancellationToken cancellationToken = default);
}
