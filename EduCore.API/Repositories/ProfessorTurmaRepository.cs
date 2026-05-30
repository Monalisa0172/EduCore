using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class ProfessorTurmaRepository
    : IProfessorTurmaRepository
{
    private readonly AppDbContext _context;

    public ProfessorTurmaRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProfessorTurmaResponseDTO>>
        GetAllAsync()
    {
        return await _context.ProfessorTurmas
            .Include(x => x.Professor)
                .ThenInclude(x => x.Funcionario)
                    .ThenInclude(x => x.Usuario)
            .Include(x => x.Turma)
            .Select(x => new ProfessorTurmaResponseDTO
            {
                Id = x.Id,

                ProfessorId = x.ProfessorId,

                Professor =
                    x.Professor.Funcionario.Usuario.Nome
                    ?? string.Empty,

                TurmaId = x.TurmaId,

                Turma =
                    x.Turma.Nome
                    ?? string.Empty,

                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<ProfessorTurmaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _context.ProfessorTurmas
            .Include(x => x.Professor)
                .ThenInclude(x => x.Funcionario)
                    .ThenInclude(x => x.Usuario)
            .Include(x => x.Turma)
            .Where(x => x.Id == id)
            .Select(x => new ProfessorTurmaResponseDTO
            {
                Id = x.Id,

                ProfessorId = x.ProfessorId,

                Professor =
                    x.Professor.Funcionario.Usuario.Nome
                    ?? string.Empty,

                TurmaId = x.TurmaId,

                Turma =
                    x.Turma.Nome
                    ?? string.Empty,

                Ativo = x.Ativo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProfessorTurma?>
        GetEntityByIdAsync(int id)
    {
        return await _context.ProfessorTurmas
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<ProfessorTurma?>
        GetByProfessorAndTurmaAsync(
            int professorId,
            int turmaId)
    {
        return await _context.ProfessorTurmas
            .FirstOrDefaultAsync(x =>
                x.ProfessorId == professorId &&
                x.TurmaId == turmaId &&
                x.Ativo);
    }

    public async Task AddAsync(
        ProfessorTurma professorTurma)
    {
        await _context.ProfessorTurmas
            .AddAsync(professorTurma);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        ProfessorTurma professorTurma)
    {
        _context.ProfessorTurmas
            .Update(professorTurma);

        await _context.SaveChangesAsync();
    }
}