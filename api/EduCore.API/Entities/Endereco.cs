using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Endereco")]
public partial class Endereco
{
    [Key]
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    [StringLength(150)]
    [Unicode(false)]
    public string? Rua { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Numero { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string? Complemento { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Bairro { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Cidade { get; set; }

    [StringLength(50)]
    [Unicode(false)]
    public string? Estado { get; set; }

    [Column("CEP")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Cep { get; set; }

    [ForeignKey("UsuarioId")]
    [InverseProperty("Enderecos")]
    public virtual Usuario? Usuario { get; set; }
}
