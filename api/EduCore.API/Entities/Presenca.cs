using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Presenca")]
public partial class Presenca
{
    [Key]
    public int Id { get; set; }

    public int? AlunoId { get; set; }

    public int? DisciplinaId { get; set; }

    public DateOnly? Data { get; set; }

    public bool? Presente { get; set; }

    [ForeignKey("AlunoId")]
    [InverseProperty("Presencas")]
    public virtual Aluno? Aluno { get; set; }

    [ForeignKey("DisciplinaId")]
    [InverseProperty("Presencas")]
    public virtual Disciplina? Disciplina { get; set; }
}
