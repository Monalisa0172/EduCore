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

    public bool Ativo { get; set; }

    public int AnoLetivo { get; set; }

    [InverseProperty(nameof(Matricula.Turma))]
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    [InverseProperty(nameof(Atribuicao.Turma))]
    public virtual ICollection<Atribuicao> Atribuicoes { get; set; } = new List<Atribuicao>();

    [InverseProperty(nameof(Avaliacao.Turma))]
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

    [InverseProperty(nameof(Presenca.Turma))]
    public virtual ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();
}
