using System.Linq.Expressions;
using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class TurmaRepository
    : ITurmaRepository
{
    // Projecao unica: a contagem de alunos vira um COUNT no SQL (sem carregar as matriculas).
    private static readonly Expression<Func<Turma, TurmaResponseDTO>> ParaDto = x => new TurmaResponseDTO
    {
        Id = x.Id,
        Nome = x.Nome ?? string.Empty,
        AnoLetivo = x.AnoLetivo,
        Ativo = x.Ativo,
        QuantidadeAlunos = x.Matriculas.Count(m => m.Aluno != null && m.Aluno.Usuario != null && m.Aluno.Usuario.Ativo)
    };

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
            .OrderByDescending(x => x.AnoLetivo)
            .ThenBy(x => x.Nome)
            .Select(ParaDto)
            .ToListAsync();
    }

    public async Task<TurmaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _context.Turmas
            .Where(x => x.Id == id)
            .Select(ParaDto)
            .FirstOrDefaultAsync();
    }

    public async Task<Turma?>
        GetEntityByIdAsync(int id)
    {
        return await _context.Turmas
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Turma?>
        GetByNomeEAnoAsync(string nome, int anoLetivo)
    {
        return await _context.Turmas
            .FirstOrDefaultAsync(x =>
                x.Nome == nome &&
                x.AnoLetivo == anoLetivo);
    }

    public async Task<List<TurmaAlunoDTO>>
        GetAlunosAsync(int turmaId)
    {
        return await _context.Matriculas
            .Where(m => m.TurmaId == turmaId && m.Aluno != null && m.Aluno.Usuario != null)
            .Select(m => new TurmaAlunoDTO
            {
                AlunoId = m.Aluno!.Id,
                UsuarioId = m.Aluno.Usuario!.Id,
                Nome = m.Aluno.Usuario.Nome ?? string.Empty,
                Email = m.Aluno.Usuario.Email ?? string.Empty,
                Ativo = m.Aluno.Usuario.Ativo
            })
            .OrderBy(a => a.Nome)
            .ToListAsync();
    }

    public async Task<List<Usuario>>
        GetAlunosAtivosParaMatriculaAsync(IReadOnlyCollection<int> usuarioIds)
    {
        return await _context.Usuarios
            .Include(u => u.Alunos)
                .ThenInclude(a => a.Matriculas)
            .Where(u => usuarioIds.Contains(u.Id) && u.Ativo && u.Tipo == TipoUsuarioEnum.Aluno)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<Matricula?>
        GetMatriculaAsync(int turmaId, int usuarioId)
    {
        return await _context.Matriculas
            .FirstOrDefaultAsync(m =>
                m.TurmaId == turmaId &&
                m.Aluno != null &&
                m.Aluno.UsuarioId == usuarioId);
    }

    public void RemoverMatricula(Matricula matricula)
    {
        _context.Matriculas.Remove(matricula);
    }

    public Task SalvarAsync() => _context.SaveChangesAsync();

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

    public async Task<List<int>>
        GetIdsAtivosAsync(IReadOnlyCollection<int> ids)
    {
        return await _context.Turmas
            .Where(x => x.Ativo && ids.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync();
    }
}
