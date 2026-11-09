using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IRedefinicaoSenhaRepository
{
    /// <summary>Usuário ativo com o e-mail (comparação sem diferenciar maiúsculas).</summary>
    Task<Usuario?> GetUsuarioAtivoPorEmailAsync(string email);

    /// <summary>Usuário ativo pelo id (rastreado, para gravar a nova senha).</summary>
    Task<Usuario?> GetUsuarioAtivoAsync(int usuarioId);

    /// <summary>Pedidos ainda abertos (não usados) do usuário, rastreados para invalidar.</summary>
    Task<List<RedefinicaoSenha>> GetAbertasAsync(int usuarioId);

    /// <summary>Pedido pelo hash do token, com o usuário (rastreados, para gravar a nova senha).</summary>
    Task<RedefinicaoSenha?> GetPorHashAsync(string tokenHash);

    void Adicionar(RedefinicaoSenha redefinicao);

    Task SalvarAsync();
}
