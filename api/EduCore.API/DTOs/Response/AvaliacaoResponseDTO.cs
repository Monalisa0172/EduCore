using EduCore.API.Enums;

namespace EduCore.API.DTOs.Response;

public class AvaliacaoResponseDTO
{
    public int Id { get; set; }

    public int TurmaId { get; set; }

    public string TurmaNome { get; set; } = string.Empty;

    public int DisciplinaId { get; set; }

    public string DisciplinaNome { get; set; } = string.Empty;

    public int? SubDisciplinaId { get; set; }

    public string? SubDisciplinaNome { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Peso { get; set; }

    public DateTime DataAplicacao { get; set; }

    public BimestreEnum Bimestre { get; set; }

    public bool Ativo { get; set; }

    /// <summary>Quantas notas já foram lançadas nesta avaliação.</summary>
    public int QuantidadeNotas { get; set; }
}
