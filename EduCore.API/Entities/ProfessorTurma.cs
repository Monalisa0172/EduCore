using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

[Table("ProfessorTurma")]
public class ProfessorTurma
{
    [Key]
    public int Id { get; set; }

    public int ProfessorId { get; set; }

    public int TurmaId { get; set; }

    public bool Ativo { get; set; }

    [ForeignKey("ProfessorId")]
    public virtual Professor Professor { get; set; } = null!;

    [ForeignKey("TurmaId")]
    public virtual Turma Turma { get; set; } = null!;
}