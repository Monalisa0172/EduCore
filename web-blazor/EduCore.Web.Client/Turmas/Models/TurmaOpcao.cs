namespace EduCore.Web.Client.Turmas.Models;

/// <summary>
/// Turma retornada por GET api/turmas, usada como opção de matrícula.
/// </summary>
public sealed record TurmaOpcao(int Id, string Nome, int AnoLetivo, bool Ativo);
