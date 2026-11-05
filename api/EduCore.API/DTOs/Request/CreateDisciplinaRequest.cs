using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Dados para criar ou editar uma disciplina.
/// </summary>
public class CreateDisciplinaRequest
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    /// <summary>Código único (ex.: MAT01). Gravado em maiúsculas.</summary>
    [Required(ErrorMessage = "Informe o código.")]
    [StringLength(20, ErrorMessage = "O código deve ter no máximo 20 caracteres.")]
    [RegularExpression(@"^\s*[A-Za-z0-9_-]+\s*$", ErrorMessage = "Use apenas letras, números, hífen ou sublinhado, sem espaços.")]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres.")]
    public string? Descricao { get; set; }

    /// <summary>Carga horária total, em horas.</summary>
    [Range(1, 2000, ErrorMessage = "A carga horária deve estar entre 1 e 2000 horas.")]
    public int CargaHoraria { get; set; }
}
