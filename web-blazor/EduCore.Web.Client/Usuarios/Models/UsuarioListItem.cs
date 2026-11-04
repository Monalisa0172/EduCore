using EduCore.Web.Client.Auth;

namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Usuário ativo exibido no grid do painel administrativo (GET api/usuarios/ativos).
/// </summary>
public sealed record UsuarioListItem(
    int UsuarioId,
    string Nome,
    string Email,
    string Tipo,
    string Especificacao)
{
    public bool IsAluno => Tipo == Roles.Aluno;
}
