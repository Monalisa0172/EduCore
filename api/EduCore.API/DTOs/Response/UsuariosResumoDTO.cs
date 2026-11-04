namespace EduCore.API.DTOs.Response;

/// <summary>
/// Totais de usuários ativos por perfil.
/// </summary>
/// <param name="ProfessoresAtivos">Quantidade de professores ativos.</param>
/// <param name="AlunosAtivos">Quantidade de alunos ativos.</param>
public record UsuariosResumoDTO(int ProfessoresAtivos, int AlunosAtivos);
