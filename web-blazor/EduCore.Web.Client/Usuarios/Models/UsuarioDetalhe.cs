using EduCore.Web.Client.Auth;

namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Aluno ou professor carregado para edição (GET api/usuarios/{id}).
/// </summary>
public sealed record UsuarioDetalhe(
    int UsuarioId,
    string Nome,
    string Email,
    string Tipo,
    bool Ativo,
    IReadOnlyList<int> TurmaIds,
    DadosProfessor? Professor)
{
    public bool IsAluno => Tipo == Roles.Aluno;
}

/// <summary>
/// Nulo em <see cref="UsuarioDetalhe.Professor"/> quando o professor ainda não tem dados funcionais.
/// </summary>
public sealed record DadosProfessor(
    string Especialidade,
    string Cargo,
    DateOnly DataAdmissao,
    TipoContrato? TipoContrato);
