using System.ComponentModel.DataAnnotations;

namespace EduCore.Web.Client.Turmas.Models;

/// <summary>
/// Turma retornada pela API para as telas de cadastro (GET api/turmas).
/// </summary>
public sealed record TurmaItem(int Id, string Nome, int AnoLetivo, bool Ativo, int QuantidadeAlunos)
{
    public bool SemAlunos => QuantidadeAlunos == 0;

    public string AlunosTexto => QuantidadeAlunos switch
    {
        0 => "Nenhum aluno",
        1 => "1 aluno",
        var n => $"{n} alunos"
    };
}

/// <summary>
/// Aluno matriculado na turma (GET api/turmas/{id}/alunos).
/// </summary>
public sealed record TurmaAluno(int AlunoId, int UsuarioId, string Nome, string Email, bool Ativo);

/// <summary>Resultado da matrícula em lote: lista atualizada da turma e quantos foram selecionados.</summary>
public sealed record MatriculaConcluida(IReadOnlyList<TurmaAluno> Alunos, int Quantidade);

/// <summary>Corpo de POST/PUT api/turmas.</summary>
public sealed record SalvarTurmaRequest(string Nome, int AnoLetivo);

/// <summary>Turma gravada pelo diálogo; <paramref name="Nova"/> indica cadastro (e não edição).</summary>
public sealed record TurmaSalva(TurmaItem Turma, bool Nova);

/// <summary>
/// Estado do formulário de turma. As regras espelham o CreateTurmaRequest da API.
/// </summary>
public sealed class TurmaFormModel
{
    public static readonly string[] Campos = [nameof(Nome), nameof(AnoLetivo)];

    public int? Id { get; private init; }

    public bool Edicao => Id is not null;

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o ano letivo.")]
    [Range(2000, 2100, ErrorMessage = "Informe um ano letivo entre 2000 e 2100.")]
    public int? AnoLetivo { get; set; }

    /// <summary>Nova turma já sugere o ano corrente.</summary>
    public static TurmaFormModel Nova(TimeProvider relogio) => new() { AnoLetivo = relogio.GetLocalNow().Year };

    public static TurmaFormModel De(TurmaItem turma) => new()
    {
        Id = turma.Id,
        Nome = turma.Nome,
        AnoLetivo = turma.AnoLetivo
    };

    public SalvarTurmaRequest ParaRequest() => new(Nome.Trim(), AnoLetivo ?? 0);
}
