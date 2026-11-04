namespace EduCore.API.Interfaces.Repositories;

using EduCore.API.DTOs.Aluno;
using EduCore.API.Entities;

public interface IAlunoRepository
{
    Task<List<AlunoResponseDTO>> GetAllAsync();
    Task<AlunoResponseDTO?> GetByIdAsync(int id);
    Task AddAsync(Aluno aluno);
}