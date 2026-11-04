using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Aluno")]
public partial class Aluno
{
    [Key]
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    [InverseProperty("Aluno")]
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    [InverseProperty("Aluno")]
    public virtual ICollection<Notum> Nota { get; set; } = new List<Notum>();

    [InverseProperty("Aluno")]
    public virtual ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Alunos")]
    public virtual Usuario? Usuario { get; set; }
}
