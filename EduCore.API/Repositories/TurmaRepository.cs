using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class TurmaRepository
    : ITurmaRepository
{
    private readonly AppDbContext _context;

    public TurmaRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TurmaResponseDTO>>
        GetAllAsync()
    {
        return await _context.Turmas
            .Select(x => new TurmaResponseDTO
            {
                Id = x.Id,
                Nome = x.Nome ?? string.Empty,
                AnoLetivo = x.AnoLetivo,
                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<TurmaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _context.Turmas
            .Where(x => x.Id == id)
            .Select(x => new TurmaResponseDTO
            {
                Id = x.Id,
                Nome = x.Nome ?? string.Empty,
                AnoLetivo = x.AnoLetivo,
                Ativo = x.Ativo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<Turma?>
        GetEntityByIdAsync(int id)
    {
        return await _context.Turmas
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Turma?>
        GetByNomeAsync(string nome)
    {
        return await _context.Turmas
            .FirstOrDefaultAsync(x =>
                x.Nome == nome);
    }

    public async Task AddAsync(Turma turma)
    {
        await _context.Turmas
            .AddAsync(turma);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Turma turma)
    {
        _context.Turmas.Update(turma);

        await _context.SaveChangesAsync();
    }
}