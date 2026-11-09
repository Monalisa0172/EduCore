using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

/// <summary>
/// Link de "esqueci minha senha": uso único, com validade. Só o hash (SHA-256) do token fica no banco.
/// </summary>
[Table("RedefinicaoSenha")]
public class RedefinicaoSenha
{
    [Key]
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    /// <summary>SHA-256 do token, em hexadecimal maiúsculo (64 caracteres).</summary>
    [StringLength(64)]
    [Unicode(false)]
    public string TokenHash { get; set; } = string.Empty;

    public DateTime CriadoEm { get; set; }

    public DateTime ExpiraEm { get; set; }

    /// <summary>Preenchido ao usar o link (ou ao ser invalidado por um pedido novo).</summary>
    public DateTime? UsadoEm { get; set; }

    [ForeignKey(nameof(UsuarioId))]
    public virtual Usuario Usuario { get; set; } = null!;
}
