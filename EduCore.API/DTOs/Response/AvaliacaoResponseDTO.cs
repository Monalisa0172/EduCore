using EduCore.API.Enums;

namespace EduCore.API.DTOs.Response;

public class AvaliacaoResponseDTO
{
    public int Id { get; set; }

    public int DisciplinaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    public decimal Peso { get; set; }

    public DateTime DataAplicacao { get; set; }

    public BimestreEnum Bimestre { get; set; }

    public bool Ativo { get; set; }

    public int? SubDisciplinaId { get; set; }

    public string? SubDisciplina { get; set; }
}