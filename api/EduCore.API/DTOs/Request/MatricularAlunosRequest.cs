using System.ComponentModel.DataAnnotations;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Alunos a matricular numa turma (POST api/turmas/{id}/alunos).
/// </summary>
public class MatricularAlunosRequest
{
    /// <summary>Ids de usuário dos alunos. Quem já está na turma é ignorado.</summary>
    [Required(ErrorMessage = "Selecione ao menos um aluno.")]
    [MinLength(1, ErrorMessage = "Selecione ao menos um aluno.")]
    [MaxLength(200, ErrorMessage = "Matricule no máximo 200 alunos por vez.")]
    public List<int> UsuarioIds { get; set; } = new();
}
