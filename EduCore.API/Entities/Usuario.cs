using EduCore.API.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

[Table("Usuario")]
public partial class Usuario
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Nome { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Email { get; set; }

    [StringLength(255)]
    [Unicode(false)]
    public string? Senha { get; set; }

    public TipoUsuarioEnum Tipo { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Contato> Contatos { get; set; } = new List<Contato>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Endereco> Enderecos { get; set; } = new List<Endereco>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Funcionario> Funcionarios { get; set; } = new List<Funcionario>();
}
