using EduCore.API.Data;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Academico;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class AcompanhamentoRepository : IAcompanhamentoRepository
{
    private readonly AppDbContext _context;

    public AcompanhamentoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<int?> GetProfessorIdPorUsuarioAsync(int usuarioId)
        => _context.Professors
            .Where(p => p.Funcionario.UsuarioId == usuarioId)
            .Select(p => (int?)p.Id)
            .FirstOrDefaultAsync();

    public async Task<List<AvaliacaoDaTurma>> GetAvaliacoesAsync(int turmaId, int disciplinaId)
    {
        var avaliacoes = await _context.Avaliacoes
            .AsNoTracking()
            .Where(a => a.Ativo && a.TurmaId == turmaId && a.DisciplinaId == disciplinaId)
            .Select(a => new
            {
                a.Id,
                a.Nome,
                a.SubDisciplinaId,
                SubDisciplinaNome = a.SubDisciplina != null ? a.SubDisciplina.Nome : null,
                a.Peso,
                a.DataAplicacao,
                a.Bimestre,
                Notas = a.Notas.Select(n => new NotaDoAluno(n.AlunoId, n.Valor)).ToList()
            })
            .AsSplitQuery()
            .ToListAsync();

        return avaliacoes
            .Select(a => new AvaliacaoDaTurma(a.Id, a.Nome, a.SubDisciplinaId, a.SubDisciplinaNome, a.Peso, a.DataAplicacao, a.Bimestre, a.Notas))
            .ToList();
    }

    public Task<List<PresencaDaTurma>> GetPresencasAsync(int turmaId, int disciplinaId)
        => _context.Presencas
            .AsNoTracking()
            .Where(p => p.TurmaId == turmaId && p.DisciplinaId == disciplinaId)
            .Select(p => new PresencaDaTurma(p.AlunoId, p.Data, p.Presente))
            .ToListAsync();
}
