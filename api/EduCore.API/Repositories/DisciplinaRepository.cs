using System.Linq.Expressions;
using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class DisciplinaRepository : IDisciplinaRepository
{
    // Projecao unica: a contagem de subdisciplinas vira um COUNT no SQL (sem carregar a colecao).
    private static readonly Expression<Func<Disciplina, DisciplinaResponseDTO>> ParaDto = x => new DisciplinaResponseDTO
    {
        Id = x.Id,
        Nome = x.Nome ?? string.Empty,
        Codigo = x.Codigo,
        Descricao = x.Descricao,
        CargaHoraria = x.CargaHoraria,
        Ativo = x.Ativo,
        QuantidadeSubDisciplinas = x.SubDisciplinas.Count(s => s.Ativo)
    };

    private readonly AppDbContext _context;

    public DisciplinaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<DisciplinaResponseDTO>> GetAllAsync()
    {
        return await _context.Disciplinas
            .OrderBy(x => x.Nome)
            .Select(ParaDto)
            .ToListAsync();
    }

    public async Task<DisciplinaResponseDTO?> GetByIdAsync(int id)
    {
        return await _context.Disciplinas
            .Where(x => x.Id == id)
            .Select(ParaDto)
            .FirstOrDefaultAsync();
    }

    public async Task<Disciplina?> GetEntityByIdAsync(int id)
    {
        return await _context.Disciplinas
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Disciplina?> GetByCodigoAsync(string codigo)
    {
        return await _context.Disciplinas
            .FirstOrDefaultAsync(x => x.Codigo == codigo);
    }

    public async Task AddAsync(Disciplina disciplina)
    {
        await _context.Disciplinas.AddAsync(disciplina);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Disciplina disciplina)
    {
        _context.Disciplinas.Update(disciplina);

        await _context.SaveChangesAsync();
    }
}
