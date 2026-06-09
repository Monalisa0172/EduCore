using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

public partial class Notum
{
    [Key]
    public int Id { get; set; }

    public int? AlunoId { get; set; }

    public int? DisciplinaId { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? Valor { get; set; }

    [ForeignKey("AlunoId")]
    [InverseProperty("Nota")]
    public virtual Aluno? Aluno { get; set; }

    [ForeignKey("DisciplinaId")]
    [InverseProperty("Nota")]
    public virtual Disciplina? Disciplina { get; set; }
}
