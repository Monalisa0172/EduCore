using EduCore.API.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

[Table("Funcionario")]
public partial class Funcionario
{
    [Key]
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public DateOnly DataAdmissao { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Cargo { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? Salario { get; set; }
    public int? TipoContrato { get; set; }
    public StatusFuncionarioEnum Status { get; set; }
    public bool Ativo { get; set; }
    [InverseProperty("Funcionario")]
    public virtual ICollection<Professor> Professors { get; set; } = new List<Professor>();
    [ForeignKey("UsuarioId")]
    [InverseProperty("Funcionarios")]
    public virtual Usuario Usuario { get; set; } = null!;
}
