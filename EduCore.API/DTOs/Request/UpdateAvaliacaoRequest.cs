using EduCore.API.Enums;

namespace EduCore.API.DTOs.Request;

public class UpdateAvaliacaoRequest
{
    public string Nome { get; set; } = string.Empty;

    public decimal Peso { get; set; }

    public DateTime DataAplicacao { get; set; }

    public BimestreEnum Bimestre { get; set; }

    public int? SubDisciplinaId { get; set; }
}