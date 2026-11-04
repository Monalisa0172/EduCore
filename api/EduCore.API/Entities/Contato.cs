using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Contato")]
public partial class Contato
{
    [Key]
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    public int? Tipo { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Valor { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Contatos")]
    public virtual Usuario? Usuario { get; set; }
}
