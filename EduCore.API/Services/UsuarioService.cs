namespace EduCore.API.Services;

using EduCore.API.Interfaces.Repositories;

public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(
        IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
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