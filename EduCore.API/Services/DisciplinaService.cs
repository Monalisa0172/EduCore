using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class DisciplinaService
{
    private readonly IDisciplinaRepository _disciplinaRepository;

    public DisciplinaService(
        IDisciplinaRepository disciplinaRepository)
    {
        _disciplinaRepository = disciplinaRepository;
    }

    public async Task<List<DisciplinaResponseDTO>> GetAllAsync()
    {
        return await _disciplinaRepository.GetAllAsync();
    }

    public async Task<DisciplinaResponseDTO?> GetByIdAsync(int id)
    {
        return await _disciplinaRepository.GetByIdAsync(id);
    }

    public async Task<DisciplinaResponseDTO?> CreateAsync(
        CreateDisciplinaRequest request)
    {
        var disciplinaExiste = await _disciplinaRepository
            .GetByCodigoAsync(request.Codigo);

        if (disciplinaExiste != null)
            return null;

        var disciplina = new Disciplina
        {
            Nome = request.Nome,
            Codigo = request.Codigo,
            Descricao = request.Descricao,
            CargaHoraria = request.CargaHoraria,
            Ativo = true
        };

        await _disciplinaRepository.AddAsync(disciplina);

        return new DisciplinaResponseDTO
        {
            Id = disciplina.Id,
            Nome = disciplina.Nome ?? string.Empty,
            Codigo = disciplina.Codigo,
            Descricao = disciplina.Descricao,
            CargaHoraria = disciplina.CargaHoraria,
            Ativo = disciplina.Ativo
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        CreateDisciplinaRequest request)
    {
        var disciplina = await _disciplinaRepository
            .GetEntityByIdAsync(id);

        if (disciplina == null)
            return false;

        disciplina.Nome = request.Nome;
        disciplina.Codigo = request.Codigo;
        disciplina.Descricao = request.Descricao;
        disciplina.CargaHoraria = request.CargaHoraria;

        await _disciplinaRepository.UpdateAsync(disciplina);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var disciplina = await _disciplinaRepository
            .GetEntityByIdAsync(id);

        if (disciplina == null)
            return false;

        disciplina.Ativo = false;

        await _disciplinaRepository.UpdateAsync(disciplina);

        return true;
    }
}