using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IDisciplinaRepository
{
    Task<List<DisciplinaResponseDTO>> GetAllAsync();
    Task<DisciplinaResponseDTO?> GetByIdAsync(int id);
    Task<Disciplina?> GetEntityByIdAsync(int id);
    Task<Disciplina?> GetByCodigoAsync(string codigo);
    Task AddAsync(Disciplina disciplina);
    Task UpdateAsync(Disciplina disciplina);
}