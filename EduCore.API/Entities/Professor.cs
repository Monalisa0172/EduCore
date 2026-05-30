using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Professor")]
public partial class Professor
{
    [Key]
    public int Id { get; set; }

    public int FuncionarioId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Especialidade { get; set; }

    [ForeignKey("FuncionarioId")]
    [InverseProperty("Professor")]
    public virtual Funcionario Funcionario { get; set; } = null!;

    [InverseProperty("Professor")]
    public virtual ICollection<ProfessorSubDisciplina>
        ProfessorSubDisciplinas
    { get; set; }
            = new List<ProfessorSubDisciplina>();

    [InverseProperty("Professor")]
    public virtual ICollection<ProfessorTurma>
        ProfessorTurmas
    { get; set; }
            = new List<ProfessorTurma>();
}