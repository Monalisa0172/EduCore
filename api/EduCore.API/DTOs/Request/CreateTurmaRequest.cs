using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Dados para criar ou editar uma turma.
/// </summary>
public class CreateTurmaRequest
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Range(2000, 2100, ErrorMessage = "Informe um ano letivo entre 2000 e 2100.")]
    public int AnoLetivo { get; set; }
}
