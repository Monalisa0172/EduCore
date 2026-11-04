namespace EduCore.API.Repositories;

using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task AddAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);

        await _context.SaveChangesAsync();
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);

        await _context.SaveChangesAsync();
    }

    public async Task<List<UsuarioListItemDTO>> GetAtivosAsync(
        IReadOnlyCollection<TipoUsuarioEnum> tipos)
    {
        // Projeta so as colunas necessarias; a ordenacao alfabetica fica no banco.
        var usuarios = await _context.Usuarios
            .AsNoTracking()
            .Where(u => u.Ativo && tipos.Contains(u.Tipo))
            .OrderBy(u => u.Nome)
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Tipo,
                Turmas = u.Alunos
                    .SelectMany(a => a.Matriculas)
                    .Where(m => m.Turma != null && m.Turma.Ativo)
                    .OrderBy(m => m.Turma!.Nome)
                    .Select(m => m.Turma!.Nome)
                    .ToList(),
                Especialidade = u.Funcionarios
                    .Where(f => f.Professor != null)
                    .Select(f => f.Professor!.Especialidade)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return usuarios
            .Select(u => new UsuarioListItemDTO(
                u.Id,
                u.Nome ?? string.Empty,
                u.Email ?? string.Empty,
                u.Tipo.ToString(),
                u.Tipo == TipoUsuarioEnum.Aluno
                    ? string.Join(", ", u.Turmas.Where(nome => !string.IsNullOrWhiteSpace(nome)))
                    : u.Especialidade ?? string.Empty))
            .ToList();
    }

    public async Task<int> CountAtivosAsync(TipoUsuarioEnum tipo)
    {
        return await _context.Usuarios
            .CountAsync(u => u.Ativo && u.Tipo == tipo);
    }

    public async Task<Usuario?> GetComPerfilAsync(int id)
    {
        // Split query: evita o produto cartesiano entre matriculas e funcionarios.
        return await _context.Usuarios
            .Include(u => u.Alunos)
                .ThenInclude(a => a.Matriculas)
                    .ThenInclude(m => m.Turma)
            .Include(u => u.Funcionarios)
                .ThenInclude(f => f.Professor)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> EmailEmUsoAsync(string email, int? ignorarUsuarioId = null)
    {
        // A collation do banco (CI) ja torna a comparacao case-insensitive.
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email
                && (ignorarUsuarioId == null || u.Id != ignorarUsuarioId));
    }

    public void RemoverMatriculas(IEnumerable<Matricula> matriculas)
    {
        // Remocao explicita: tirar da colecao so anularia o AlunoId (FK opcional).
        _context.Matriculas.RemoveRange(matriculas);
    }

    public async Task SalvarAlteracoesAsync()
    {
        await _context.SaveChangesAsync();
    }
}