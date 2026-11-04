namespace EduCore.Web.Client.Auth;

/// <summary>
/// Perfis emitidos pela EduCore API no claim de role (espelha EduCore.API.Constants.Roles).
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Professor = "Professor";
    public const string Aluno = "Aluno";
    public const string Funcionario = "Funcionario";
}
