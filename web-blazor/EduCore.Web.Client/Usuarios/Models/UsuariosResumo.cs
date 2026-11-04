namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Totais de usuários ativos por perfil (GET api/usuarios/resumo).
/// </summary>
public sealed record UsuariosResumo(int ProfessoresAtivos, int AlunosAtivos);
