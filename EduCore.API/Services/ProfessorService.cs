using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class ProfessorService
{
    private readonly IProfessorRepository _professorRepository;
    private readonly IFuncionarioRepository _funcionarioRepository;
    private readonly IUsuarioRepository _usuarioRepository;

    public ProfessorService(
        IProfessorRepository professorRepository,
        IFuncionarioRepository funcionarioRepository,
        IUsuarioRepository usuarioRepository)
    {
        _professorRepository = professorRepository;
        _funcionarioRepository = funcionarioRepository;
        _usuarioRepository = usuarioRepository;
    }

    public async Task<List<ProfessorResponseDTO>> GetAllAsync()
    {
        return await _professorRepository.GetAllAsync();
    }

    public async Task<ProfessorResponseDTO?> GetByIdAsync(int id)
    {
        return await _professorRepository.GetByIdAsync(id);
    }

    public async Task<ProfessorResponseDTO?> CreateAsync(
        CreateProfessorRequest request)
    {
        var usuarioExiste = await _usuarioRepository
            .GetByEmailAsync(request.Email);

        if (usuarioExiste != null)
            return null;

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Tipo = TipoUsuarioEnum.Professor,
            Ativo = true
        };

        await _usuarioRepository.AddAsync(usuario);

        var funcionario = new Funcionario
        {
            UsuarioId = usuario.Id,
            Cargo = request.Cargo,
            Salario = request.Salario,
            TipoContrato = request.TipoContrato,
            DataAdmissao = request.DataAdmissao,
            Status = StatusFuncionarioEnum.Ativo,
            Ativo = true
        };

        await _funcionarioRepository.AddAsync(funcionario);

        var professor = new Professor
        {
            FuncionarioId = funcionario.Id,
            Especialidade = request.Especialidade
        };

        await _professorRepository.AddAsync(professor);

        return new ProfessorResponseDTO
        {
            Id = professor.Id,
            UsuarioId = usuario.Id,
            FuncionarioId = funcionario.Id,
            Nome = usuario.Nome ?? string.Empty,
            Email = usuario.Email ?? string.Empty,
            Cargo = funcionario.Cargo ?? string.Empty,
            Salario = funcionario.Salario,
            Especialidade = professor.Especialidade ?? string.Empty,
            Ativo = funcionario.Ativo,
            Status = funcionario.Status.ToString(),
        };
    }

    public async Task<bool> UpdateStatusAsync(
    int funcionarioId,
    StatusFuncionarioEnum status)
    {
        var funcionario = await _funcionarioRepository
            .GetByIdAsync(funcionarioId);

        if (funcionario == null)
            return false;

        funcionario.Status = status;

        if (status == StatusFuncionarioEnum.Desligado)
        {
            funcionario.Ativo = false;

            var usuario = await _usuarioRepository
                .GetByIdAsync(funcionario.UsuarioId);

            if (usuario != null)
            {
                usuario.Ativo = false;

                await _usuarioRepository.UpdateAsync(usuario);
            }
        }

        await _funcionarioRepository.UpdateAsync(funcionario);

        return true;
    }
}