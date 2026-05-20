using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IProfessorRepository
{
    Task<List<ProfessorResponseDTO>> GetAllAsync();
    Task<ProfessorResponseDTO?> GetByIdAsync(int id);
    Task AddAsync(Professor professor);
}