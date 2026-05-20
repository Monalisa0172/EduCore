using EduCore.API.Data;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class FuncionarioRepository : IFuncionarioRepository
{
    private readonly AppDbContext _context;

    public FuncionarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Funcionario funcionario)
    {
        await _context.Funcionarios.AddAsync(funcionario);
        await _context.SaveChangesAsync();
    }

    public async Task<Funcionario?> GetByIdAsync(int id)
    {
        return await _context.Funcionarios
            .Include(x => x.Usuario)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Funcionario funcionario)
    {
        _context.Funcionarios.Update(funcionario);

        await _context.SaveChangesAsync();
    }
}