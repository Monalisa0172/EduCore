using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private readonly AppDbContext _context;

    public AvaliacaoRepository(
        AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvaliacaoResponseDTO>> GetAllAsync()
    {
        return await _context.Avaliacoes
            .Include(x => x.Disciplina)
            .Select(x => new AvaliacaoResponseDTO
            {
                Id = x.Id,
                DisciplinaId = x.DisciplinaId,
                Nome = x.Nome,
                Peso = x.Peso,
                DataAplicacao = x.DataAplicacao,
                Bimestre = (BimestreEnum)x.Bimestre,
                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<AvaliacaoResponseDTO?> GetByIdAsync(int id)
    {
        return await _context.Avaliacoes
            .Include(x => x.Disciplina)
            .Where(x => x.Id == id)
            .Select(x => new AvaliacaoResponseDTO
            {
                Id = x.Id,
                DisciplinaId = x.DisciplinaId,
                Nome = x.Nome,
                Peso = x.Peso,
                DataAplicacao = x.DataAplicacao,
                Bimestre = (BimestreEnum)x.Bimestre,
                Ativo = x.Ativo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<AvaliacaoResponseDTO>> GetByDisciplinaAsync(int disciplinaId)
    {
        return await _context.Avaliacoes
            .Where(x =>
                x.DisciplinaId == disciplinaId &&
                x.Ativo)
            .Select(x => new AvaliacaoResponseDTO
            {
                Id = x.Id,
                DisciplinaId = x.DisciplinaId,
                Nome = x.Nome,
                Peso = x.Peso,
                DataAplicacao = x.DataAplicacao,
                Bimestre = (BimestreEnum)x.Bimestre,
                Ativo = x.Ativo
            })
            .ToListAsync();
    }

    public async Task<Avaliacao?> GetEntityByIdAsync(int id)
    {
        return await _context.Avaliacoes
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Avaliacao avaliacao)
    {
        await _context.Avaliacoes
            .AddAsync(avaliacao);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Avaliacao avaliacao)
    {
        _context.Avaliacoes
            .Update(avaliacao);

        await _context.SaveChangesAsync();
    }

    public async Task<decimal> GetPesoTotalByDisciplinaEBimestreAsync(int disciplinaId, int bimestre)
    {
        return await _context.Avaliacoes
            .Where(x =>
                x.DisciplinaId == disciplinaId &&
                x.Bimestre == bimestre &&
                x.Ativo)
            .SumAsync(x => x.Peso);
    }

    public async Task<decimal>
    GetPesoTotalByDisciplinaEBimestreAsync(
        int disciplinaId,
        BimestreEnum bimestre,
        int? avaliacaoIgnoradaId = null)
    {
        var query = _context.Avaliacoes
            .Where(x =>
                x.Ativo &&
                x.DisciplinaId == disciplinaId &&
                x.Bimestre == (int)bimestre);

        if (avaliacaoIgnoradaId.HasValue)
        {
            query = query.Where(x =>
                x.Id != avaliacaoIgnoradaId.Value);
        }

        return await query.SumAsync(x => x.Peso);
    }
}