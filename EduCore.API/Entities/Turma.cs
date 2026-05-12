using System;
using System.Collections.Generic;
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

    public int? ProfessorId { get; set; }

    [InverseProperty("Turma")]
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();
}
