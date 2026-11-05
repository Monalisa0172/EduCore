using System.Linq.Expressions;
using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class AvaliacaoRepository : IAvaliacaoRepository
{
    private static readonly Expression<Func<Avaliacao, AvaliacaoResponseDTO>> ParaDto = a => new AvaliacaoResponseDTO
    {
        Id = a.Id,
        TurmaId = a.TurmaId,
        TurmaNome = a.Turma!.Nome ?? string.Empty,
        DisciplinaId = a.DisciplinaId,
        DisciplinaNome = a.Disciplina!.Nome ?? string.Empty,
        SubDisciplinaId = a.SubDisciplinaId,
        SubDisciplinaNome = a.SubDisciplina != null ? a.SubDisciplina.Nome : null,
        Nome = a.Nome,
        Peso = a.Peso,
        DataAplicacao = a.DataAplicacao,
        Bimestre = (BimestreEnum)a.Bimestre,
        Ativo = a.Ativo,
        QuantidadeNotas = a.Notas.Count
    };

    private readonly AppDbContext _context;

    public AvaliacaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AvaliacaoResponseDTO>> ListarAsync(int? turmaId, int? disciplinaId, int? bimestre, bool? ativo, int? professorId = null)
    {
        var query = _context.Avaliacoes.AsNoTracking();

        if (professorId.HasValue)
        {
            query = query.Where(av => _context.Atribuicoes.Any(a =>
                a.Ativo
                && a.ProfessorId == professorId
                && a.TurmaId == av.TurmaId
                && a.DisciplinaId == av.DisciplinaId
                && (a.SubDisciplinaId == null || a.SubDisciplinaId == av.SubDisciplinaId)));
        }

        if (turmaId.HasValue) query = query.Where(a => a.TurmaId == turmaId);
        if (disciplinaId.HasValue) query = query.Where(a => a.DisciplinaId == disciplinaId);
        if (bimestre.HasValue) query = query.Where(a => a.Bimestre == bimestre);
        if (ativo.HasValue) query = query.Where(a => a.Ativo == ativo);

        return await query
            .OrderBy(a => a.Bimestre)
            .ThenBy(a => a.DataAplicacao)
            .ThenBy(a => a.Nome)
            .Select(ParaDto)
            .ToListAsync();
    }

    public Task<AvaliacaoResponseDTO?> GetByIdAsync(int id)
        => _context.Avaliacoes.AsNoTracking().Where(a => a.Id == id).Select(ParaDto).FirstOrDefaultAsync();

    public Task<Avaliacao?> GetEntityByIdAsync(int id)
        => _context.Avaliacoes.FirstOrDefaultAsync(a => a.Id == id);

    public Task<decimal> GetPesoTotalAsync(int turmaId, int disciplinaId, int bimestre, int? avaliacaoIgnoradaId = null)
        => _context.Avaliacoes
            .Where(a => a.Ativo
                && a.TurmaId == turmaId
                && a.DisciplinaId == disciplinaId
                && a.Bimestre == bimestre
                && a.Id != avaliacaoIgnoradaId)
            .SumAsync(a => a.Peso);

    public async Task AddAsync(Avaliacao avaliacao)
    {
        await _context.Avaliacoes.AddAsync(avaliacao);
        await _context.SaveChangesAsync();
    }

    public Task SalvarAsync() => _context.SaveChangesAsync();
}
