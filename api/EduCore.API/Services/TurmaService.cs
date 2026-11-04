using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class TurmaService
{
    private readonly ITurmaRepository _turmaRepository;

    public TurmaService(ITurmaRepository turmaRepository)
    {
        _turmaRepository = turmaRepository;
    }

    public async Task<List<TurmaResponseDTO>>
        GetAllAsync()
    {
        return await _turmaRepository
            .GetAllAsync();
    }

    public async Task<TurmaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _turmaRepository
            .GetByIdAsync(id);
    }

    public async Task<TurmaResponseDTO?>
        CreateAsync(
            CreateTurmaRequest request)
    {
        var turmaExiste =
            await _turmaRepository
                .GetByNomeAsync(
                    request.Nome);

        if (turmaExiste != null)
            throw new Exception(
                "Turma já cadastrada");

        var turma = new Turma
        {
            Nome = request.Nome,
            AnoLetivo =
                request.AnoLetivo,
            Ativo = true
        };

        await _turmaRepository
            .AddAsync(turma);

        return await _turmaRepository
            .GetByIdAsync(turma.Id);
    }

    public async Task<bool>
        InativarAsync(int id)
    {
        var turma =
            await _turmaRepository
                .GetEntityByIdAsync(id);

        if (turma == null)
            return false;

        turma.Ativo = false;

        await _turmaRepository
            .UpdateAsync(turma);

        return true;
    }
}