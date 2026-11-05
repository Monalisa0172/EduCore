using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Atribui um professor a uma disciplina (ou subdisciplina) de uma turma.
/// </summary>
public class AtribuicaoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o professor.")]
    public int ProfessorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a turma.")]
    public int TurmaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a disciplina.")]
    public int DisciplinaId { get; set; }

    /// <summary>Vazio: o professor responde pela disciplina inteira.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Subdisciplina inválida.")]
    public int? SubDisciplinaId { get; set; }
}

/// <summary>
/// Troca o professor de uma atribuição (ex.: substituição), mantendo turma e disciplina.
/// </summary>
public class TrocarProfessorRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione o professor.")]
    public int ProfessorId { get; set; }
}
