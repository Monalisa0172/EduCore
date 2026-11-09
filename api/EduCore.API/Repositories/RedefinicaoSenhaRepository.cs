using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class RedefinicaoSenhaRepository : IRedefinicaoSenhaRepository
{
    private readonly AppDbContext _context;

    public RedefinicaoSenhaRepository(AppDbContext context)
    {
        _context = context;
    }

    // A collation do banco ja e case-insensitive; o e-mail chega normalizado (trim + minusculas).
    public Task<Usuario?> GetUsuarioAtivoPorEmailAsync(string email)
        => _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email && u.Ativo);

    public Task<List<RedefinicaoSenha>> GetAbertasAsync(int usuarioId)
        => _context.RedefinicoesSenha.Where(r => r.UsuarioId == usuarioId && r.UsadoEm == null).ToListAsync();

    public Task<RedefinicaoSenha?> GetPorHashAsync(string tokenHash)
        => _context.RedefinicoesSenha.Include(r => r.Usuario).FirstOrDefaultAsync(r => r.TokenHash == tokenHash);

    public void Adicionar(RedefinicaoSenha redefinicao) => _context.RedefinicoesSenha.Add(redefinicao);

    public Task SalvarAsync() => _context.SaveChangesAsync();
}
