using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("SubDisciplina")]
public partial class SubDisciplina
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Nome { get; set; }

    public int? DisciplinaId { get; set; }

    [ForeignKey("DisciplinaId")]
    [InverseProperty("SubDisciplinas")]
    public virtual Disciplina? Disciplina { get; set; }
}
