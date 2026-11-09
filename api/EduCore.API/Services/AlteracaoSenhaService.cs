using EduCore.API.DTOs.Request;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Acesso;

namespace EduCore.API.Services;

/// <summary>
/// "Alterar minha senha" com o usuário logado. Exige a senha atual (quem pegar o computador
/// destravado não troca a senha) e invalida links de "esqueci minha senha" ainda abertos.
/// </summary>
public class AlteracaoSenhaService
{
    private const string SenhaAtualIncorreta = "A senha atual está incorreta.";

    private readonly IRedefinicaoSenhaRepository _repositorio;
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly TimeProvider _relogio;

    public AlteracaoSenhaService(IRedefinicaoSenhaRepository repositorio, IUsuarioAtual usuarioAtual, TimeProvider relogio)
    {
        _repositorio = repositorio;
        _usuarioAtual = usuarioAtual;
        _relogio = relogio;
    }

    public async Task<ResultadoOperacao<bool>> AlterarAsync(AlterarSenhaRequest request)
    {
        var usuario = await _repositorio.GetUsuarioAtivoAsync(_usuarioAtual.UsuarioId);
        if (usuario is null)
        {
            return ResultadoOperacao<bool>.NaoEncontrado("Usuário não encontrado ou inativo.");
        }

        if (!Confere(request.SenhaAtual, usuario.Senha))
        {
            return ResultadoOperacao<bool>.Invalido(nameof(AlterarSenhaRequest.SenhaAtual), SenhaAtualIncorreta);
        }

        if (Confere(request.NovaSenha, usuario.Senha))
        {
            return ResultadoOperacao<bool>.Invalido(nameof(AlterarSenhaRequest.NovaSenha), "A nova senha precisa ser diferente da atual.");
        }

        usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

        var agora = _relogio.GetUtcNow().UtcDateTime;
        foreach (var aberta in await _repositorio.GetAbertasAsync(usuario.Id))
        {
            aberta.UsadoEm = agora;
        }

        await _repositorio.SalvarAsync();
        return ResultadoOperacao<bool>.Ok(true);
    }

    // Hash vazio ou fora do formato BCrypt (legado) nunca confere; Verify lancaria excecao.
    private static bool Confere(string senha, string? hash)
        => !string.IsNullOrEmpty(hash) && hash.StartsWith("$2", StringComparison.Ordinal) && BCrypt.Net.BCrypt.Verify(senha, hash);
}
