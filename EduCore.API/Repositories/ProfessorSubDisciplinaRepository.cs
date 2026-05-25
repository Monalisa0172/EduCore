using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class ProfessorSubDisciplinaRepository : IProfessorSubDisciplinaRepository
{
    private readonly AppDbContext _context;

    public ProfessorSubDisciplinaRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProfessorSubDisciplinaResponseDTO>>
        GetAllAsync()
    {
        return await _context.ProfessorSubDisciplinas
            .Include(x => x.Professor)
                .ThenInclude(x => x.Funcionario)
                    .ThenInclude(x => x.Usuario)
            .Include(x => x.SubDisciplina)
            .Select(x => new ProfessorSubDisciplinaResponseDTO
            {
                Id = x.Id,
                ProfessorId = x.ProfessorId,
                Professor =
                    x.Professor.Funcionario.Usuario.Nome
                    ?? string.Empty,

                SubDisciplinaId = x.SubDisciplinaId,

                SubDisciplina =
                    x.SubDisciplina.Nome
                    ?? string.Empty,

                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<ProfessorSubDisciplinaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _context.ProfessorSubDisciplinas
            .Include(x => x.Professor)
                .ThenInclude(x => x.Funcionario)
                    .ThenInclude(x => x.Usuario)
            .Include(x => x.SubDisciplina)
            .Where(x => x.Id == id)
            .Select(x => new ProfessorSubDisciplinaResponseDTO
            {
                Id = x.Id,
                ProfessorId = x.ProfessorId,

                Professor =
                    x.Professor.Funcionario.Usuario.Nome
                    ?? string.Empty,

                SubDisciplinaId = x.SubDisciplinaId,

                SubDisciplina =
                    x.SubDisciplina.Nome
                    ?? string.Empty,

                Ativo = x.Ativo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ProfessorSubDisciplina?>
    GetByProfessorAndSubDisciplinaAsync(
        int professorId,
        int subDisciplinaId)
    {
        return await _context.ProfessorSubDisciplinas
            .FirstOrDefaultAsync(x =>
                x.ProfessorId == professorId &&
                x.SubDisciplinaId == subDisciplinaId &&
                x.Ativo);
    }

    public async Task AddAsync(
        ProfessorSubDisciplina professorSubDisciplina)
    {
        await _context.ProfessorSubDisciplinas.AddAsync(professorSubDisciplina);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        ProfessorSubDisciplina professorSubDisciplina)
    {
        _context.ProfessorSubDisciplinas.Update(professorSubDisciplina);

        await _context.SaveChangesAsync();
    }

    public async Task<ProfessorSubDisciplina?>
    GetEntityByIdAsync(int id)
    {
        return await _context.ProfessorSubDisciplinas
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}