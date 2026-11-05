using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

/// <summary>
/// Aluno matriculado em uma turma (único por aluno + turma).
/// </summary>
[Table("Matricula")]
public partial class Matricula
{
    [Key]
    public int Id { get; set; }

    public int AlunoId { get; set; }

    public int TurmaId { get; set; }

    [ForeignKey(nameof(AlunoId))]
    [InverseProperty(nameof(Entities.Aluno.Matriculas))]
    public virtual Aluno Aluno { get; set; } = null!;

    [ForeignKey(nameof(TurmaId))]
    [InverseProperty(nameof(Entities.Turma.Matriculas))]
    public virtual Turma Turma { get; set; } = null!;
}
