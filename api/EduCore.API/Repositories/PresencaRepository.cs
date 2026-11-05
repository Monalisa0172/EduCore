using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class PresencaRepository : IPresencaRepository
{
    private readonly AppDbContext _context;

    public PresencaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Presenca>> GetDaAulaAsync(int turmaId, int disciplinaId, DateOnly data)
        => _context.Presencas
            .Where(p => p.TurmaId == turmaId && p.DisciplinaId == disciplinaId && p.Data == data)
            .ToListAsync();

    public async Task<List<(DateOnly Data, int Presentes, int Total)>> GetAulasAsync(int turmaId, int disciplinaId)
    {
        var aulas = await _context.Presencas
            .AsNoTracking()
            .Where(p => p.TurmaId == turmaId && p.DisciplinaId == disciplinaId)
            .GroupBy(p => p.Data)
            .Select(g => new { Data = g.Key, Presentes = g.Count(p => p.Presente), Total = g.Count() })
            .OrderByDescending(a => a.Data)
            .ToListAsync();

        return aulas.Select(a => (a.Data, a.Presentes, a.Total)).ToList();
    }

    public void Adicionar(Presenca presenca) => _context.Presencas.Add(presenca);

    public Task SalvarAsync() => _context.SaveChangesAsync();
}
