using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;

namespace EduCore.API.Interfaces.Services;

public interface IAvaliacaoService
{
    Task<List<AvaliacaoResponseDTO>> GetAllAsync();
    Task<AvaliacaoResponseDTO?> GetByIdAsync(int id);
    Task<List<AvaliacaoResponseDTO>> GetByDisciplinaAsync(int disciplinaId);
    Task<AvaliacaoResponseDTO?> CreateAsync(CreateAvaliacaoRequest request);
    Task<bool> UpdateAsync(int id, UpdateAvaliacaoRequest request);
    Task<bool> DeleteAsync(int id);
}