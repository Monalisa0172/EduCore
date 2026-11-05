using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class NotaRepository : INotaRepository
{
    private readonly AppDbContext _context;

    public NotaRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Nota>> GetDaAvaliacaoAsync(int avaliacaoId)
        => _context.Notas.Where(n => n.AvaliacaoId == avaliacaoId).ToListAsync();

    public void Adicionar(Nota nota) => _context.Notas.Add(nota);

    public void Remover(Nota nota) => _context.Notas.Remove(nota);

    public Task SalvarAsync() => _context.SaveChangesAsync();
}
