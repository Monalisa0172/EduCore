using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Matricula")]
public partial class Matricula
{
    [Key]
    public int Id { get; set; }

    public int? AlunoId { get; set; }

    public int? TurmaId { get; set; }

    [ForeignKey("AlunoId")]
    [InverseProperty("Matriculas")]
    public virtual Aluno? Aluno { get; set; }

    [ForeignKey("TurmaId")]
    [InverseProperty("Matriculas")]
    public virtual Turma? Turma { get; set; }
}
