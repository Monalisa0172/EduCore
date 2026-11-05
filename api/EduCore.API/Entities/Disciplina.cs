using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

[Table("Disciplina")]
public partial class Disciplina
{
    [Key]
    public int Id { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string? Nome { get; set; }

    [StringLength(20)]
    [Unicode(false)]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(255)]
    [Unicode(false)]
    public string? Descricao { get; set; }

    public int? CargaHoraria { get; set; }

    public bool Ativo { get; set; }

    [InverseProperty(nameof(Presenca.Disciplina))]
    public virtual ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();

    [InverseProperty(nameof(SubDisciplina.Disciplina))]
    public virtual ICollection<SubDisciplina> SubDisciplinas { get; set; } = new List<SubDisciplina>();

    [InverseProperty(nameof(Avaliacao.Disciplina))]
    public virtual ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

    [InverseProperty(nameof(Atribuicao.Disciplina))]
    public virtual ICollection<Atribuicao> Atribuicoes { get; set; } = new List<Atribuicao>();
}
