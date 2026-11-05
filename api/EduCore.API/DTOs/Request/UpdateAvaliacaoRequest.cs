using System.ComponentModel.DataAnnotations;
using EduCore.API.Enums;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Dados editáveis da avaliação. Turma e disciplina não mudam depois de criada.
/// </summary>
public class UpdateAvaliacaoRequest
{
    [Required(ErrorMessage = "Informe o nome da avaliação.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    // Limites lidos em cultura invariante: em pt-BR "0.01" nao e um decimal valido e a validacao estourava (500).
    [Range(typeof(decimal), "0.01", "100", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "O peso deve ser maior que 0 e no máximo 100.")]
    public decimal Peso { get; set; }

    [Required(ErrorMessage = "Informe a data de aplicação.")]
    public DateTime? DataAplicacao { get; set; }

    [EnumDataType(typeof(BimestreEnum), ErrorMessage = "Bimestre inválido: use 1, 2, 3 ou 4.")]
    public BimestreEnum Bimestre { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Subdisciplina inválida.")]
    public int? SubDisciplinaId { get; set; }
}
