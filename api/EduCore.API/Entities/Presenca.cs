using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

/// <summary>
/// Registro de chamada: um por aluno, turma, disciplina e dia de aula.
/// </summary>
[Table("Presenca")]
public partial class Presenca
{
    [Key]
    public int Id { get; set; }

    public int TurmaId { get; set; }

    public int DisciplinaId { get; set; }

    public int AlunoId { get; set; }

    public DateOnly Data { get; set; }

    public bool Presente { get; set; }

    [ForeignKey(nameof(TurmaId))]
    [InverseProperty(nameof(Entities.Turma.Presencas))]
    public virtual Turma Turma { get; set; } = null!;

    [ForeignKey(nameof(DisciplinaId))]
    [InverseProperty(nameof(Entities.Disciplina.Presencas))]
    public virtual Disciplina Disciplina { get; set; } = null!;

    [ForeignKey(nameof(AlunoId))]
    [InverseProperty(nameof(Entities.Aluno.Presencas))]
    public virtual Aluno Aluno { get; set; } = null!;
}
