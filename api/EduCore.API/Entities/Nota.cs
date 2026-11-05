using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

/// <summary>
/// Nota do aluno em uma avaliação (0 a 10). Disciplina, turma e bimestre vêm da avaliação.
/// </summary>
[Table("Nota")]
public class Nota
{
    [Key]
    public int Id { get; set; }

    public int AvaliacaoId { get; set; }

    public int AlunoId { get; set; }

    [Column(TypeName = "decimal(4, 2)")]
    public decimal Valor { get; set; }

    [ForeignKey(nameof(AvaliacaoId))]
    [InverseProperty(nameof(Entities.Avaliacao.Notas))]
    public virtual Avaliacao Avaliacao { get; set; } = null!;

    [ForeignKey(nameof(AlunoId))]
    [InverseProperty(nameof(Entities.Aluno.Notas))]
    public virtual Aluno Aluno { get; set; } = null!;
}
