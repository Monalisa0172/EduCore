using System.ComponentModel.DataAnnotations;

namespace EduCore.Web.Client.Atribuicoes.Models;

/// <summary>
/// Quem leciona o quê na turma (GET api/atribuicoes). Sem subdisciplina = disciplina inteira.
/// </summary>
public sealed record AtribuicaoItem(
    int Id,
    int ProfessorId,
    string ProfessorNome,
    int TurmaId,
    string TurmaNome,
    int AnoLetivo,
    bool TurmaAtiva,
    int DisciplinaId,
    string DisciplinaNome,
    int? SubDisciplinaId,
    string? SubDisciplinaNome,
    bool Ativo)
{
    public bool DisciplinaInteira => SubDisciplinaId is null;

    /// <summary>"Português" ou "Português · Gramática".</summary>
    public string ItemTexto => DisciplinaInteira ? DisciplinaNome : $"{DisciplinaNome} · {SubDisciplinaNome}";
}

/// <summary>
/// Professor para escolha na atribuição (GET api/professores).
/// </summary>
public sealed record ProfessorOpcao(int Id, string Nome, string Email, string Especialidade, bool Ativo)
{
    public string Texto => string.IsNullOrWhiteSpace(Especialidade) ? Nome : $"{Nome} — {Especialidade}";
}

/// <summary>Corpo de POST api/atribuicoes.</summary>
public sealed record CriarAtribuicaoRequest(int ProfessorId, int TurmaId, int DisciplinaId, int? SubDisciplinaId);

/// <summary>Corpo de PATCH api/atribuicoes/{id}/professor.</summary>
public sealed record TrocarProfessorRequest(int ProfessorId);

/// <summary>Atribuição gravada pelo diálogo; <paramref name="Troca"/> indica troca de professor.</summary>
public sealed record AtribuicaoSalva(AtribuicaoItem Atribuicao, bool Troca);

/// <summary>
/// Estado do formulário de atribuição. As regras espelham o AtribuicaoRequest da API.
/// </summary>
public sealed class AtribuicaoFormModel : IValidatableObject
{
    public static readonly string[] Campos = [nameof(DisciplinaId), nameof(SubDisciplinaId), nameof(ProfessorId)];

    [Required(ErrorMessage = "Selecione a disciplina.")]
    public int? DisciplinaId { get; set; }

    /// <summary>Verdadeiro: a disciplina é dividida e esta atribuição vale para uma subdisciplina.</summary>
    public bool PorSubDisciplina { get; set; }

    public int? SubDisciplinaId { get; set; }

    [Required(ErrorMessage = "Selecione o professor.")]
    public int? ProfessorId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PorSubDisciplina && SubDisciplinaId is null)
        {
            yield return new ValidationResult("Selecione a subdisciplina.", [nameof(SubDisciplinaId)]);
        }
    }

    public CriarAtribuicaoRequest ParaRequest(int turmaId)
        => new(ProfessorId ?? 0, turmaId, DisciplinaId ?? 0, PorSubDisciplina ? SubDisciplinaId : null);
}
