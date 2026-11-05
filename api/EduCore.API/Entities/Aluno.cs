using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduCore.API.Entities;

[Table("Aluno")]
public partial class Aluno
{
    [Key]
    public int Id { get; set; }

    public int? UsuarioId { get; set; }

    [InverseProperty(nameof(Matricula.Aluno))]
    public virtual ICollection<Matricula> Matriculas { get; set; } = new List<Matricula>();

    [InverseProperty(nameof(Nota.Aluno))]
    public virtual ICollection<Nota> Notas { get; set; } = new List<Nota>();

    [InverseProperty(nameof(Presenca.Aluno))]
    public virtual ICollection<Presenca> Presencas { get; set; } = new List<Presenca>();

    [ForeignKey("UsuarioId")]
    [InverseProperty("Alunos")]
    public virtual Usuario? Usuario { get; set; }
}
