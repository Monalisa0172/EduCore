namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Corpo do POST api/usuarios.
/// </summary>
public sealed record CriarUsuarioRequest(
    TipoUsuario Tipo,
    string Nome,
    string Email,
    string Senha,
    IReadOnlyList<int> TurmaIds,
    DadosProfessorRequest? Professor);

/// <summary>
/// Corpo do PUT api/usuarios/{id}. <see cref="NovaSenha"/> nula mantém a senha atual.
/// </summary>
public sealed record AtualizarUsuarioRequest(
    string Nome,
    string Email,
    string? NovaSenha,
    IReadOnlyList<int> TurmaIds,
    DadosProfessorRequest? Professor);

public sealed record DadosProfessorRequest(
    string Especialidade,
    string Cargo,
    DateOnly DataAdmissao,
    TipoContrato TipoContrato);
