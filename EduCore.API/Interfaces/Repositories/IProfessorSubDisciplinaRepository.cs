using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IProfessorSubDisciplinaRepository
{
    Task<List<ProfessorSubDisciplinaResponseDTO>> GetAllAsync();
    Task<ProfessorSubDisciplinaResponseDTO?> GetByIdAsync(int id);
    Task<ProfessorSubDisciplina?> GetEntityByIdAsync(int id);
    Task<ProfessorSubDisciplina?>
    GetByProfessorAndSubDisciplinaAsync(int professorId, int subDisciplinaId);
    Task AddAsync(ProfessorSubDisciplina professorSubDisciplina);
    Task UpdateAsync(ProfessorSubDisciplina professorSubDisciplina);
}