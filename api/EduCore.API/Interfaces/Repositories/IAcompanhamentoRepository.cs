using EduCore.API.Services.Academico;

namespace EduCore.API.Interfaces.Repositories;

/// <summary>
/// Leituras do acompanhamento do professor (admin): avaliações com as notas de todos os alunos e a chamada da turma.
/// </summary>
public interface IAcompanhamentoRepository
{
    /// <summary>Professor do usuário, ativo ou não (nulo se o usuário não for professor).</summary>
    Task<int?> GetProfessorIdPorUsuarioAsync(int usuarioId);

    Task<List<AvaliacaoDaTurma>> GetAvaliacoesAsync(int turmaId, int disciplinaId);

    Task<List<PresencaDaTurma>> GetPresencasAsync(int turmaId, int disciplinaId);
}
