using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Chamada de uma aula (turma + disciplina + dia), enviada de uma vez.
/// Refazer a chamada do mesmo dia atualiza os registros, sem duplicar.
/// </summary>
public class RegistrarChamadaRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecione a turma.")]
    public int TurmaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione a disciplina.")]
    public int DisciplinaId { get; set; }

    [Required(ErrorMessage = "Informe a data da aula.")]
    public DateOnly? Data { get; set; }

    [Required(ErrorMessage = "Envie a presença dos alunos.")]
    [MinLength(1, ErrorMessage = "Envie a presença dos alunos.")]
    [MaxLength(200, ErrorMessage = "Envie no máximo 200 alunos por vez.")]
    public List<PresencaRequest> Presencas { get; set; } = [];
}

public class PresencaRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Aluno inválido.")]
    public int AlunoId { get; set; }

    public bool Presente { get; set; }
}
