using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface ISubDisciplinaRepository
{
    Task<List<SubDisciplinaResponseDTO>> GetAllAsync();
    Task<SubDisciplinaResponseDTO?> GetByIdAsync(int id);
    Task<List<SubDisciplinaResponseDTO>> GetByDisciplinaAsync(int disciplinaId);
    Task<SubDisciplina?> GetEntityByIdAsync(int id);
    Task<SubDisciplina?> GetByNomeAndDisciplinaAsync(string nome, int disciplinaId);
    Task AddAsync(SubDisciplina subDisciplina);
    Task UpdateAsync(SubDisciplina subDisciplina);
}