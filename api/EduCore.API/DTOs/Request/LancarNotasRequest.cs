using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Notas de uma avaliação, enviadas de uma vez (a grade inteira da turma).
/// Só os alunos enviados mudam; valor vazio remove o lançamento daquele aluno.
/// </summary>
public class LancarNotasRequest
{
    [Required(ErrorMessage = "Envie ao menos uma nota.")]
    [MinLength(1, ErrorMessage = "Envie ao menos uma nota.")]
    [MaxLength(200, ErrorMessage = "Envie no máximo 200 notas por vez.")]
    public List<NotaLancamentoRequest> Notas { get; set; } = [];
}

public class NotaLancamentoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Aluno inválido.")]
    public int AlunoId { get; set; }

    /// <summary>Nota de 0 a 10 (até 2 casas). Vazio: remove a nota lançada.</summary>
    [Range(typeof(decimal), "0", "10", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "A nota deve estar entre 0 e 10.")]
    public decimal? Valor { get; set; }
}
