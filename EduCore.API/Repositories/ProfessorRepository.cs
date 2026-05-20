using EduCore.API.Data;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Repositories;

public class ProfessorRepository : IProfessorRepository
{
    private readonly AppDbContext _context;

    public ProfessorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProfessorResponseDTO>> GetAllAsync()
    {
        return await _context.Professors
            .Include(x => x.Funcionario)
            .ThenInclude(x => x.Usuario)
            .Select(x => new ProfessorResponseDTO
            {
                Id = x.Id,
                FuncionarioId = x.FuncionarioId,
                UsuarioId = x.Funcionario.UsuarioId,
                Nome = x.Funcionario.Usuario.Nome ?? string.Empty,
                Email = x.Funcionario.Usuario.Email ?? string.Empty,
                Cargo = x.Funcionario.Cargo ?? string.Empty,
                Salario = x.Funcionario.Salario,
                Especialidade = x.Especialidade ?? string.Empty,
                Status = x.Funcionario.Status.ToString(),
                Ativo = x.Funcionario.Ativo
            })
            .ToListAsync();
    }

    public async Task<ProfessorResponseDTO?> GetByIdAsync(int id)
    {
        return await _context.Professors
            .Include(x => x.Funcionario)
            .ThenInclude(x => x.Usuario)
            .Where(x => x.Id == id)
            .Select(x => new ProfessorResponseDTO
            {
                Id = x.Id,
                FuncionarioId = x.FuncionarioId,
                UsuarioId = x.Funcionario.UsuarioId,
                Nome = x.Funcionario.Usuario.Nome ?? string.Empty,
                Email = x.Funcionario.Usuario.Email ?? string.Empty,
                Cargo = x.Funcionario.Cargo ?? string.Empty,
                Salario = x.Funcionario.Salario,
                Especialidade = x.Especialidade ?? string.Empty,
                Status = x.Funcionario.Status.ToString(),
                Ativo = x.Funcionario.Ativo
            })
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(Professor professor)
    {
        await _context.Professors.AddAsync(professor);
        await _context.SaveChangesAsync();
    }
}