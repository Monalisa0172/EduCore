using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class SubDisciplinaService
{
    private readonly ISubDisciplinaRepository
        _subDisciplinaRepository;

    private readonly IDisciplinaRepository
        _disciplinaRepository;

    public SubDisciplinaService(
        ISubDisciplinaRepository subDisciplinaRepository,
        IDisciplinaRepository disciplinaRepository)
    {
        _subDisciplinaRepository = subDisciplinaRepository;
        _disciplinaRepository = disciplinaRepository;
    }

    public async Task<List<SubDisciplinaResponseDTO>>
        GetAllAsync()
    {
        return await _subDisciplinaRepository
            .GetAllAsync();
    }

    public async Task<SubDisciplinaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _subDisciplinaRepository
            .GetByIdAsync(id);
    }

    public async Task<List<SubDisciplinaResponseDTO>>
        GetByDisciplinaAsync(int disciplinaId)
    {
        return await _subDisciplinaRepository
            .GetByDisciplinaAsync(disciplinaId);
    }

    public async Task<SubDisciplinaResponseDTO?>
        CreateAsync(CreateSubDisciplinaRequest request)
    {
        var disciplina =
            await _disciplinaRepository
                .GetEntityByIdAsync(
                    request.DisciplinaId);

        if (disciplina == null)
            return null;

        var subDisciplinaExiste =
            await _subDisciplinaRepository
                .GetByNomeAndDisciplinaAsync(
                    request.Nome,
                    request.DisciplinaId);

        if (subDisciplinaExiste != null)
            return null;

        var subDisciplina = new SubDisciplina
        {
            Nome = request.Nome,
            Descricao = request.Descricao,
            DisciplinaId = request.DisciplinaId,
            Ativo = true
        };

        await _subDisciplinaRepository
            .AddAsync(subDisciplina);

        return new SubDisciplinaResponseDTO
        {
            Id = subDisciplina.Id,
            Nome = subDisciplina.Nome ?? string.Empty,
            Descricao = subDisciplina.Descricao,
            DisciplinaId = disciplina.Id,
            Disciplina = disciplina.Nome ?? string.Empty,
            Ativo = subDisciplina.Ativo
        };
    }

    public async Task<bool> UpdateAsync(
        int id,
        CreateSubDisciplinaRequest request)
    {
        var subDisciplina =
            await _subDisciplinaRepository
                .GetEntityByIdAsync(id);

        if (subDisciplina == null)
            return false;

        subDisciplina.Nome = request.Nome;
        subDisciplina.Descricao = request.Descricao;
        subDisciplina.DisciplinaId =
            request.DisciplinaId;

        await _subDisciplinaRepository
            .UpdateAsync(subDisciplina);

        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var subDisciplina =
            await _subDisciplinaRepository
                .GetEntityByIdAsync(id);

        if (subDisciplina == null)
            return false;

        subDisciplina.Ativo = false;

        await _subDisciplinaRepository
            .UpdateAsync(subDisciplina);

        return true;
    }
}