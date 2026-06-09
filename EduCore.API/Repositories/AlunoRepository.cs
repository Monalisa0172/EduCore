namespace EduCore.API.Repositories;

using EduCore.API.Data;
using EduCore.API.DTOs.Aluno;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

public class AlunoRepository : IAlunoRepository
{
    private readonly AppDbContext _context;

    public AlunoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AlunoResponseDTO>> GetAllAsync()
    {
        return await _context.Alunos
            .Include(a => a.Usuario)
            .Select(a => new AlunoResponseDTO
            {
                Id = a.Id,
                UsuarioId = a.UsuarioId ?? 0,
                Nome = a.Usuario!.Nome ?? string.Empty,
                Email = a.Usuario.Email ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<AlunoResponseDTO?> GetByIdAsync(int id)
    {
        return await _context.Alunos
            .Include(a => a.Usuario)
            .Where(a => a.Id == id)
            .Select(a => new AlunoResponseDTO
            {
                Id = a.Id,
                UsuarioId = a.UsuarioId ?? 0,
                Nome = a.Usuario!.Nome ?? string.Empty,
                Email = a.Usuario.Email ?? string.Empty
            })
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Aluno aluno)
    {
        await _context.Alunos.AddAsync(aluno);

        await _context.SaveChangesAsync();
    }
}