using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

/// <summary>
/// Quem leciona o quê em qual turma: professor x turma x disciplina.
/// Sem subdisciplina, o professor responde pela disciplina inteira; com subdisciplina,
/// a disciplina é dividida entre professores (ex.: Português = Gramática + Literatura).
/// </summary>
[Table("Atribuicao")]
public class Atribuicao
{
    [Key]
    public int Id { get; set; }

    public int ProfessorId { get; set; }

    public int TurmaId { get; set; }

    public int DisciplinaId { get; set; }

    public int? SubDisciplinaId { get; set; }

    public bool Ativo { get; set; }

    [ForeignKey(nameof(ProfessorId))]
    [InverseProperty(nameof(Entities.Professor.Atribuicoes))]
    public virtual Professor Professor { get; set; } = null!;

    [ForeignKey(nameof(TurmaId))]
    [InverseProperty(nameof(Entities.Turma.Atribuicoes))]
    public virtual Turma Turma { get; set; } = null!;

    [ForeignKey(nameof(DisciplinaId))]
    [InverseProperty(nameof(Entities.Disciplina.Atribuicoes))]
    public virtual Disciplina Disciplina { get; set; } = null!;

    [ForeignKey(nameof(SubDisciplinaId))]
    public virtual SubDisciplina? SubDisciplina { get; set; }
}
