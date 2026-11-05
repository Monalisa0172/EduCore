using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IProfessorRepository
{
    Task<List<ProfessorResponseDTO>> GetAllAsync();
    Task<ProfessorResponseDTO?> GetByIdAsync(int id);
    Task AddAsync(Professor professor);
    Task<Professor?> GetEntityByIdAsync(int id);

    /// <summary>Id do professor do usuário (nulo se o usuário não for professor ou estiver inativo).</summary>
    Task<int?> GetIdAtivoPorUsuarioAsync(int usuarioId);
}