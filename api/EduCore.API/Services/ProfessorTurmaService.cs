using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class ProfessorTurmaService
{
    private readonly IProfessorTurmaRepository _professorTurmaRepository;
    private readonly IProfessorRepository _professorRepository;
    private readonly ITurmaRepository _turmaRepository;

    public ProfessorTurmaService(
        IProfessorTurmaRepository professorTurmaRepository,
        IProfessorRepository professorRepository,
        ITurmaRepository turmaRepository)
    {
        _professorTurmaRepository = professorTurmaRepository;
        _professorRepository = professorRepository;
        _turmaRepository = turmaRepository;
    }

    public async Task<List<ProfessorTurmaResponseDTO>>
        GetAllAsync()
    {
        return await _professorTurmaRepository
            .GetAllAsync();
    }

    public async Task<ProfessorTurmaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _professorTurmaRepository
            .GetByIdAsync(id);
    }

    public async Task<ProfessorTurmaResponseDTO?>
        CreateAsync(
            CreateProfessorTurmaRequest request)
    {
        var professor =
            await _professorRepository
                .GetEntityByIdAsync(
                    request.ProfessorId);

        if (professor == null)
            throw new Exception(
                "Professor não encontrado");

        var turma =
            await _turmaRepository
                .GetEntityByIdAsync(
                    request.TurmaId);

        if (turma == null)
            throw new Exception(
                "Turma não encontrada");

        var vinculoExiste =
            await _professorTurmaRepository
                .GetByProfessorAndTurmaAsync(
                    request.ProfessorId,
                    request.TurmaId);

        if (vinculoExiste != null)
            throw new Exception(
                "Vínculo já existe");

        var professorTurma =
            new ProfessorTurma
            {
                ProfessorId =
                    request.ProfessorId,

                TurmaId =
                    request.TurmaId,

                Ativo = true
            };

        await _professorTurmaRepository
            .AddAsync(professorTurma);

        return await
            _professorTurmaRepository
                .GetByIdAsync(
                    professorTurma.Id);
    }

    public async Task<bool>
        InativarAsync(int id)
    {
        var vinculo =
            await _professorTurmaRepository
                .GetEntityByIdAsync(id);

        if (vinculo == null)
            return false;

        vinculo.Ativo = false;

        await _professorTurmaRepository
            .UpdateAsync(vinculo);

        return true;
    }
}