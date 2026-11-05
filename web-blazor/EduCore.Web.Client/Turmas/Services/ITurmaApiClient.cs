using EduCore.Web.Client.Api;
using EduCore.Web.Client.Turmas.Models;

namespace EduCore.Web.Client.Turmas.Services;

public interface ITurmaApiClient
{
    /// <summary>
    /// Turmas ativas, da mais recente para a mais antiga e em ordem alfabética (opções de matrícula).
    /// </summary>
    Task<ApiResult<IReadOnlyList<TurmaOpcao>>> GetAtivasAsync(CancellationToken cancellationToken = default);

    /// <summary>Todas as turmas (ativas e inativas), com a quantidade de alunos.</summary>
    Task<ApiResult<IReadOnlyList<TurmaItem>>> ListarAsync(CancellationToken cancellationToken = default);

    Task<ApiResult<TurmaItem>> ObterAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<IReadOnlyList<TurmaAluno>>> ListarAlunosAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>Matricula vários alunos (ids de usuário); devolve a lista atualizada da turma.</summary>
    Task<ApiResult<IReadOnlyList<TurmaAluno>>> MatricularAsync(int id, IReadOnlyCollection<int> usuarioIds, CancellationToken cancellationToken = default);

    /// <summary>Remove o aluno da turma; devolve a lista atualizada.</summary>
    Task<ApiResult<IReadOnlyList<TurmaAluno>>> RemoverAlunoAsync(int id, int usuarioId, CancellationToken cancellationToken = default);

    Task<ApiResult<TurmaItem>> CriarAsync(SalvarTurmaRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<TurmaItem>> AtualizarAsync(int id, SalvarTurmaRequest request, CancellationToken cancellationToken = default);

    Task<ApiResult<TurmaItem>> InativarAsync(int id, CancellationToken cancellationToken = default);

    Task<ApiResult<TurmaItem>> ReativarAsync(int id, CancellationToken cancellationToken = default);
}
