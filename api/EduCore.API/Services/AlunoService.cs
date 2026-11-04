namespace EduCore.API.Services;

using EduCore.API.DTOs.Aluno;
using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;

public class AlunoService
{
    private readonly IAlunoRepository _alunoRepository;
    private readonly IUsuarioRepository _usuarioRepository;


    public AlunoService(IAlunoRepository alunoRepository, IUsuarioRepository usuarioRepository)
    {
        _alunoRepository = alunoRepository;
        _usuarioRepository = usuarioRepository;

    }

    public async Task<List<AlunoResponseDTO>> GetAllAsync()
    {
        return await _alunoRepository.GetAllAsync();
    }

    public async Task<AlunoResponseDTO?> GetByIdAsync(int id)
    {
        return await _alunoRepository.GetByIdAsync(id);
    }

    public async Task<AlunoResponseDTO?> CreateAsync(
    CreateAlunoRequest request)
    {
        var usuarioExiste = await _usuarioRepository.GetByEmailAsync(request.Email);

        if (usuarioExiste != null)
            return null;

        var usuario = new Usuario
        {
            Nome = request.Nome,
            Email = request.Email,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Tipo = TipoUsuarioEnum.Aluno,
            Ativo = true
        };

        await _usuarioRepository.AddAsync(usuario);

        var aluno = new Aluno
        {
            UsuarioId = usuario.Id
        };

        await _alunoRepository.AddAsync(aluno);

        return new AlunoResponseDTO
        {
            Id = aluno.Id,
            UsuarioId = usuario.Id,
            Nome = usuario.Nome ?? string.Empty,
            Email = usuario.Email ?? string.Empty
        };
    }
}