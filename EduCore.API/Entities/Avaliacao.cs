using EduCore.API.Entities;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Avaliacao")]
public class Avaliacao
{
    public int Id { get; set; }

    public int DisciplinaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Peso { get; set; }

    public DateTime DataAplicacao { get; set; }

    public int Bimestre { get; set; }

    public bool Ativo { get; set; }

    public virtual Disciplina? Disciplina { get; set; }

    public int? SubDisciplinaId { get; set; }

    public virtual SubDisciplina? SubDisciplina { get; set; }
}