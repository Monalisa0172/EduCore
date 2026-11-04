using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

[Table("ProfessorSubDisciplina")]
public partial class ProfessorSubDisciplina
{
    [Key]
    public int Id { get; set; }
    public int ProfessorId { get; set; }
    public int SubDisciplinaId { get; set; }
    public bool Ativo { get; set; }
    [ForeignKey("ProfessorId")]
    public virtual Professor Professor { get; set; } = null!;
    [ForeignKey("SubDisciplinaId")]
    public virtual SubDisciplina SubDisciplina { get; set; } = null!;
}