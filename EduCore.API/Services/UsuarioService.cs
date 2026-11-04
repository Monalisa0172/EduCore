namespace EduCore.API.Services;

using EduCore.API.DTOs.Response;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;

public class UsuarioService
{
    private static readonly TipoUsuarioEnum[] TiposListaveis =
        { TipoUsuarioEnum.Aluno, TipoUsuarioEnum.Professor };

    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(
        IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    /// <summary>
    /// Alunos e/ou professores ativos, em ordem alfabética.
    /// </summary>
    public Task<List<UsuarioListItemDTO>> GetAtivosAsync(TipoUsuarioEnum? tipo)
    {
        var tipos = tipo is null ? TiposListaveis : new[] { tipo.Value };

        return _usuarioRepository.GetAtivosAsync(tipos);
    }

    public static bool TipoListavel(TipoUsuarioEnum tipo)
        => TiposListaveis.Contains(tipo);

    public async Task<UsuariosResumoDTO> GetResumoAsync()
    {
        // Sequencial de proposito: o DbContext nao suporta consultas paralelas.
        var professores = await _usuarioRepository.CountAtivosAsync(TipoUsuarioEnum.Professor);
        var alunos = await _usuarioRepository.CountAtivosAsync(TipoUsuarioEnum.Aluno);

        return new UsuariosResumoDTO(professores, alunos);
    }

    public async Task<bool> InativarAsync(int id)
    {
        var usuario =
            await _usuarioRepository.GetByIdAsync(id);

        if (usuario == null)
            return false;

        usuario.Ativo = false;

        await _usuarioRepository.UpdateAsync(usuario);

        return true;
    }

    public async Task<bool> AtualizarEmailAsync(
    int id,
    string novoEmail)
    {
        var usuario =
            await _usuarioRepository.GetByIdAsync(id);

        if (usuario == null)
            return false;

        var emailExistente =
            await _usuarioRepository.GetByEmailAsync(novoEmail);

        if (emailExistente != null &&
            emailExistente.Id != id)
        {
            return false;
        }

        usuario.Email = novoEmail;

        await _usuarioRepository.UpdateAsync(usuario);

        return true;
    }
}