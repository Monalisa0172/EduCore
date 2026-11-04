using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;

namespace EduCore.API.Interfaces.Repositories;

public interface IAvaliacaoRepository
{
    Task<List<AvaliacaoResponseDTO>> GetAllAsync();
    Task<AvaliacaoResponseDTO?> GetByIdAsync(int id);
    Task<List<AvaliacaoResponseDTO>> GetByDisciplinaAsync(int disciplinaId);
    Task<Avaliacao?> GetEntityByIdAsync(int id);
    Task AddAsync(Avaliacao avaliacao);
    Task UpdateAsync(Avaliacao avaliacao);
    Task<decimal> GetPesoTotalByDisciplinaEBimestreAsync(int disciplinaId, BimestreEnum bimestre, int? avaliacaoIgnoradaId = null);
}