using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface ITurmaRepository
{
    Task<List<TurmaResponseDTO>> GetAllAsync();
    Task<TurmaResponseDTO?> GetByIdAsync(int id);
    Task<Turma?> GetEntityByIdAsync(int id);

    /// <summary>Turma com o mesmo nome no mesmo ano letivo (a combinação é única).</summary>
    Task<Turma?> GetByNomeEAnoAsync(string nome, int anoLetivo);

    Task<List<TurmaAlunoDTO>> GetAlunosAsync(int turmaId);
    Task AddAsync(Turma turma);
    Task UpdateAsync(Turma turma);
    Task<List<int>> GetIdsAtivosAsync(IReadOnlyCollection<int> ids);
}
