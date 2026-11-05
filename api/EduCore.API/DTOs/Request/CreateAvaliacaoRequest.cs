using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Nova avaliação de uma disciplina (ou subdisciplina) numa turma.
/// </summary>
public class CreateAvaliacaoRequest : UpdateAvaliacaoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione a turma.")]
    public int TurmaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a disciplina.")]
    public int DisciplinaId { get; set; }
}
