namespace EduCore.API.DTOs.Response;

/// <summary>
/// Usuário ativo para listagem no painel administrativo.
/// </summary>
/// <param name="UsuarioId">Id do usuário.</param>
/// <param name="Nome">Nome do usuário.</param>
/// <param name="Email">E-mail de acesso.</param>
/// <param name="Tipo">Perfil: Aluno ou Professor.</param>
/// <param name="Especificacao">Turmas ativas (aluno) ou especialidade (professor).</param>
public record UsuarioListItemDTO(
    int UsuarioId,
    string Nome,
    string Email,
    string Tipo,
    string Especificacao);
