using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface ITurmaRepository
{
    Task<List<TurmaResponseDTO>> GetAllAsync();
    Task<TurmaResponseDTO?> GetByIdAsync(int id);
    Task<Turma?> GetEntityByIdAsync(int id);
    Task<Turma?> GetByNomeAsync(string nome);
    Task AddAsync(Turma turma);
    Task UpdateAsync(Turma turma);
}