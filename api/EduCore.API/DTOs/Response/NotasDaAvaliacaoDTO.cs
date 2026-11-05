namespace EduCore.API.DTOs.Response;

/// <summary>
/// Grade de lançamento: a avaliação e todos os alunos da turma, com a nota de cada um (ou vazia).
/// </summary>
public class NotasDaAvaliacaoDTO
{
    public AvaliacaoResponseDTO Avaliacao { get; set; } = new();

    /// <summary>Falso quando a avaliação ou a turma está inativa (só consulta).</summary>
    public bool PodeLancar { get; set; }

    public int Lancadas { get; set; }

    public List<NotaAlunoDTO> Alunos { get; set; } = [];
}

public class NotaAlunoDTO
{
    public int AlunoId { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Aluno inativo continua na lista (histórico), mas não recebe nota nova.</summary>
    public bool Ativo { get; set; }

    public decimal? Nota { get; set; }
}
