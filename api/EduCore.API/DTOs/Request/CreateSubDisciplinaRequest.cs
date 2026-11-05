using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Dados para criar ou editar uma subdisciplina.
/// </summary>
public class CreateSubDisciplinaRequest
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres.")]
    public string? Descricao { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Informe a disciplina.")]
    public int DisciplinaId { get; set; }
}
