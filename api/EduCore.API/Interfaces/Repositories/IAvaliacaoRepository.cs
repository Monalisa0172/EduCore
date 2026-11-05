using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IAvaliacaoRepository
{
    /// <summary>
    /// Lista com nomes resolvidos; filtros nulos são ignorados. Com <paramref name="professorId"/>,
    /// só as avaliações que o professor leciona (atribuição ativa).
    /// </summary>
    Task<List<AvaliacaoResponseDTO>> ListarAsync(int? turmaId, int? disciplinaId, int? bimestre, bool? ativo, int? professorId = null);

    Task<AvaliacaoResponseDTO?> GetByIdAsync(int id);

    Task<Avaliacao?> GetEntityByIdAsync(int id);

    /// <summary>Soma dos pesos das avaliações ativas de turma + disciplina + bimestre.</summary>
    Task<decimal> GetPesoTotalAsync(int turmaId, int disciplinaId, int bimestre, int? avaliacaoIgnoradaId = null);

    Task AddAsync(Avaliacao avaliacao);

    Task SalvarAsync();
}
