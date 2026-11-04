using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IProfessorTurmaRepository
{
    Task<List<ProfessorTurmaResponseDTO>> GetAllAsync();
    Task<ProfessorTurmaResponseDTO?> GetByIdAsync(int id);
    Task<ProfessorTurma?> GetEntityByIdAsync(int id);
    Task<ProfessorTurma?> GetByProfessorAndTurmaAsync(int professorId,int turmaId);
    Task AddAsync(ProfessorTurma professorTurma);
    Task UpdateAsync(ProfessorTurma professorTurma);
}