using System.Linq.Expressions;
using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class AtribuicaoRepository : IAtribuicaoRepository
{
    private static readonly Expression<Func<Atribuicao, AtribuicaoResponseDTO>> ParaDto = a => new AtribuicaoResponseDTO
    {
        Id = a.Id,
        ProfessorId = a.ProfessorId,
        ProfessorNome = a.Professor.Funcionario.Usuario.Nome ?? string.Empty,
        TurmaId = a.TurmaId,
        TurmaNome = a.Turma.Nome ?? string.Empty,
        AnoLetivo = a.Turma.AnoLetivo,
        TurmaAtiva = a.Turma.Ativo,
        DisciplinaId = a.DisciplinaId,
        DisciplinaNome = a.Disciplina.Nome ?? string.Empty,
        SubDisciplinaId = a.SubDisciplinaId,
        SubDisciplinaNome = a.SubDisciplina != null ? a.SubDisciplina.Nome : null,
        Ativo = a.Ativo
    };

    private readonly AppDbContext _context;

    public AtribuicaoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AtribuicaoResponseDTO>> ListarAsync(int? turmaId, int? professorId, int? disciplinaId, bool? ativo)
    {
        var query = _context.Atribuicoes.AsNoTracking();

        if (turmaId.HasValue) query = query.Where(a => a.TurmaId == turmaId);
        if (professorId.HasValue) query = query.Where(a => a.ProfessorId == professorId);
        if (disciplinaId.HasValue) query = query.Where(a => a.DisciplinaId == disciplinaId);
        if (ativo.HasValue) query = query.Where(a => a.Ativo == ativo);

        return await query
            .OrderByDescending(a => a.Turma.AnoLetivo)
            .ThenBy(a => a.Turma.Nome)
            .ThenBy(a => a.Disciplina.Nome)
            .ThenBy(a => a.SubDisciplina != null ? a.SubDisciplina.Nome : string.Empty)
            .Select(ParaDto)
            .ToListAsync();
    }

    public Task<AtribuicaoResponseDTO?> GetByIdAsync(int id)
        => _context.Atribuicoes.AsNoTracking().Where(a => a.Id == id).Select(ParaDto).FirstOrDefaultAsync();

    public Task<Atribuicao?> GetEntityByIdAsync(int id)
        => _context.Atribuicoes.FirstOrDefaultAsync(a => a.Id == id);

    public Task<List<AtribuicaoResponseDTO>> GetAtivasDaDisciplinaAsync(int turmaId, int disciplinaId)
        => _context.Atribuicoes.AsNoTracking()
            .Where(a => a.Ativo && a.TurmaId == turmaId && a.DisciplinaId == disciplinaId)
            .Select(ParaDto)
            .ToListAsync();

    public async Task<List<AtribuicaoResponseDTO>> GetAtivasDoProfessorAsync(int professorId, int? turmaId = null, int? disciplinaId = null)
    {
        var query = _context.Atribuicoes.AsNoTracking().Where(a => a.Ativo && a.ProfessorId == professorId);

        if (turmaId.HasValue) query = query.Where(a => a.TurmaId == turmaId);
        if (disciplinaId.HasValue) query = query.Where(a => a.DisciplinaId == disciplinaId);

        return await query
            .OrderByDescending(a => a.Turma.AnoLetivo)
            .ThenBy(a => a.Turma.Nome)
            .ThenBy(a => a.Disciplina.Nome)
            .Select(ParaDto)
            .ToListAsync();
    }

    public async Task AddAsync(Atribuicao atribuicao)
    {
        await _context.Atribuicoes.AddAsync(atribuicao);
        await _context.SaveChangesAsync();
    }

    public Task SalvarAsync() => _context.SaveChangesAsync();
}
