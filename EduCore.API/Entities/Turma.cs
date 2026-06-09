using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Turma")]
public partial class Turma
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Nome { get; set; }

    public bool Ativo { get; set; }

    public int AnoLetivo { get; set; }

    [InverseProperty("Turma")]
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    [InverseProperty("Turma")]
    public virtual ICollection<ProfessorTurma> ProfessorTurmas { get; set; } = new List<ProfessorTurma>();
}