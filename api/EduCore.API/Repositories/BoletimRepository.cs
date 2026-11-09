using EduCore.API.Data;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Academico;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class BoletimRepository : IBoletimRepository
{
    private readonly AppDbContext _context;

    public BoletimRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<string?> GetNomeDoAlunoAsync(int alunoId)
        => _context.Alunos.Where(a => a.Id == alunoId).Select(a => a.Usuario!.Nome).FirstOrDefaultAsync();

    public Task<int?> GetAlunoIdPorUsuarioAsync(int usuarioId)
        => _context.Alunos.Where(a => a.UsuarioId == usuarioId).Select(a => (int?)a.Id).FirstOrDefaultAsync();

    public Task<List<TurmaDoAluno>> GetTurmasDoAlunoAsync(int alunoId)
        => _context.Matriculas
            .AsNoTracking()
            .Where(m => m.AlunoId == alunoId)
            .OrderByDescending(m => m.Turma.AnoLetivo)
            .ThenByDescending(m => m.Turma.Ativo)
            .Select(m => new TurmaDoAluno(m.TurmaId, m.Turma.Nome ?? string.Empty, m.Turma.AnoLetivo, m.Turma.Ativo))
            .ToListAsync();

    public async Task<DadosDoBoletim> CarregarAsync(int alunoId, int turmaId)
    {
        var avaliacoes = await _context.Avaliacoes
            .AsNoTracking()
            .Where(a => a.TurmaId == turmaId && a.Ativo)
            .Select(a => new AvaliacaoDoAluno(
                a.Id,
                a.DisciplinaId,
                a.Nome,
                a.SubDisciplina != null ? a.SubDisciplina.Nome : null,
                a.Peso,
                a.DataAplicacao,
                a.Bimestre,
                a.Notas.Where(n => n.AlunoId == alunoId).Select(n => (decimal?)n.Valor).FirstOrDefault()))
            .ToListAsync();

        var presencas = await _context.Presencas
            .AsNoTracking()
            .Where(p => p.TurmaId == turmaId && p.AlunoId == alunoId)
            .Select(p => new PresencaDoAluno(p.DisciplinaId, p.Data, p.Presente))
            .ToListAsync();

        var disciplinaIds = await _context.Atribuicoes
            .Where(a => a.TurmaId == turmaId && a.Ativo)
            .Select(a => a.DisciplinaId)
            .ToListAsync();

        disciplinaIds = disciplinaIds
            .Concat(avaliacoes.Select(a => a.DisciplinaId))
            .Concat(presencas.Select(p => p.DisciplinaId))
            .Distinct()
            .ToList();

        var disciplinas = await _context.Disciplinas
            .AsNoTracking()
            .Where(d => disciplinaIds.Contains(d.Id))
            .Select(d => new DisciplinaDoBoletim(d.Id, d.Nome ?? string.Empty))
            .ToListAsync();

        return new DadosDoBoletim(disciplinas, avaliacoes, presencas);
    }
}
