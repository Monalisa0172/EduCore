using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace EduCore.API.Entities;

/// <summary>
/// Avaliação de uma disciplina (ou subdisciplina) em uma turma e bimestre.
/// Os pesos das avaliações ativas de turma + disciplina + bimestre somam no máximo 100.
/// </summary>
[Table("Avaliacao")]
public class Avaliacao
{
    [Key]
    public int Id { get; set; }

    public int TurmaId { get; set; }

    public int DisciplinaId { get; set; }

    public int? SubDisciplinaId { get; set; }

    [StringLength(100)]
    [Unicode(false)]
    public string Nome { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal Peso { get; set; }

    public DateTime DataAplicacao { get; set; }

    public int Bimestre { get; set; }

    public bool Ativo { get; set; }

    [ForeignKey(nameof(TurmaId))]
    [InverseProperty(nameof(Entities.Turma.Avaliacoes))]
    public virtual Turma? Turma { get; set; }

    [ForeignKey(nameof(DisciplinaId))]
    [InverseProperty(nameof(Entities.Disciplina.Avaliacoes))]
    public virtual Disciplina? Disciplina { get; set; }

    [ForeignKey(nameof(SubDisciplinaId))]
    [InverseProperty(nameof(Entities.SubDisciplina.Avaliacoes))]
    public virtual SubDisciplina? SubDisciplina { get; set; }

    [InverseProperty(nameof(Nota.Avaliacao))]
    public virtual ICollection<Nota> Notas { get; set; } = new List<Nota>();
}
