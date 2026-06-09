using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class SubDisciplinaRepository
    : ISubDisciplinaRepository
{
    private readonly AppDbContext _context;

    public SubDisciplinaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SubDisciplinaResponseDTO>>
        GetAllAsync()
    {
        return await _context.SubDisciplinas
            .Include(x => x.Disciplina)
            .Select(x => new SubDisciplinaResponseDTO
            {
                Id = x.Id,
                Nome = x.Nome ?? string.Empty,
                Descricao = x.Descricao,
                DisciplinaId = x.DisciplinaId,
                Disciplina = x.Disciplina.Nome ?? string.Empty,
                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<SubDisciplinaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _context.SubDisciplinas
            .Include(x => x.Disciplina)
            .Where(x => x.Id == id)
            .Select(x => new SubDisciplinaResponseDTO
            {
                Id = x.Id,
                Nome = x.Nome ?? string.Empty,
                Descricao = x.Descricao,
                DisciplinaId = x.DisciplinaId,
                Disciplina = x.Disciplina.Nome ?? string.Empty,
                Ativo = x.Ativo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<SubDisciplinaResponseDTO>>
        GetByDisciplinaAsync(int disciplinaId)
    {
        return await _context.SubDisciplinas
            .Include(x => x.Disciplina)
            .Where(x => x.DisciplinaId == disciplinaId)
            .Select(x => new SubDisciplinaResponseDTO
            {
                Id = x.Id,
                Nome = x.Nome ?? string.Empty,
                Descricao = x.Descricao,
                DisciplinaId = x.DisciplinaId,
                Disciplina = x.Disciplina.Nome ?? string.Empty,
                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<SubDisciplina?> GetEntityByIdAsync(
        int id)
    {
        return await _context.SubDisciplinas
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<SubDisciplina?>
        GetByNomeAndDisciplinaAsync(
            string nome,
            int disciplinaId)
    {
        return await _context.SubDisciplinas
            .FirstOrDefaultAsync(x =>
                x.Nome == nome &&
                x.DisciplinaId == disciplinaId);
    }

    public async Task AddAsync(SubDisciplina subDisciplina)
    {
        await _context.SubDisciplinas
            .AddAsync(subDisciplina);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(
        SubDisciplina subDisciplina)
    {
        _context.SubDisciplinas.Update(subDisciplina);

        await _context.SaveChangesAsync();
    }
}