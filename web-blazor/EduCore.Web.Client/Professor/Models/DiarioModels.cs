using System.ComponentModel.DataAnnotations;

namespace EduCore.Web.Client.Professor.Models;

/// <summary>
/// Cabeçalho do diário de classe (GET api/diario): turma, disciplina, bimestre vigente, pesos por
/// bimestre (de toda a disciplina) e o que o usuário leciona nela.
/// </summary>
public sealed record DiarioInfo(
    int TurmaId,
    string TurmaNome,
    int AnoLetivo,
    bool TurmaAtiva,
    int AlunosAtivos,
    int DisciplinaId,
    string DisciplinaNome,
    int BimestreVigente,
    IReadOnlyList<DiarioBimestre> Bimestres,
    IReadOnlyList<DiarioAtribuicao> Atribuicoes)
{
    /// <summary>Leciona a disciplina inteira (pode criar avaliação sem subdisciplina).</summary>
    public bool DisciplinaInteira => Atribuicoes.Any(a => a.SubDisciplinaId is null);

    /// <summary>Subdisciplinas que o usuário leciona (vazio quando é a disciplina inteira).</summary>
    public IReadOnlyList<DiarioAtribuicao> SubDisciplinas => Atribuicoes.Where(a => a.SubDisciplinaId is not null).ToList();

    /// <summary>"Matemática" ou "Português · Gramática".</summary>
    public string Titulo => DisciplinaInteira || SubDisciplinas.Count == 0
        ? DisciplinaNome
        : $"{DisciplinaNome} · {string.Join(", ", SubDisciplinas.Select(s => s.SubDisciplinaNome))}";

    public DiarioBimestre Bimestre(int numero) => Bimestres.First(b => b.Bimestre == numero);
}

public sealed record DiarioBimestre(int Bimestre, decimal PesoTotal, decimal PesoDisponivel, bool Encerrado)
{
    public bool Completo => PesoDisponivel <= 0;
}

public sealed record DiarioAtribuicao(int? SubDisciplinaId, string? SubDisciplinaNome, string ProfessorNome);

/// <summary>
/// Avaliação (GET api/avaliacoes).
/// </summary>
public sealed record AvaliacaoItem(
    int Id,
    int TurmaId,
    string TurmaNome,
    int DisciplinaId,
    string DisciplinaNome,
    int? SubDisciplinaId,
    string? SubDisciplinaNome,
    string Nome,
    decimal Peso,
    DateTime DataAplicacao,
    int Bimestre,
    bool Ativo,
    int QuantidadeNotas)
{
    public DateOnly Data => DateOnly.FromDateTime(DataAplicacao);
}

/// <summary>Avaliação gravada pelo diálogo; <paramref name="Nova"/> indica criação.</summary>
public sealed record AvaliacaoSalva(AvaliacaoItem Avaliacao, bool Nova);

/// <summary>Corpo de POST api/avaliacoes (turma e disciplina) e PUT api/avaliacoes/{id} (sem elas).</summary>
public sealed record SalvarAvaliacaoRequest(
    int? TurmaId,
    int? DisciplinaId,
    int? SubDisciplinaId,
    string Nome,
    decimal Peso,
    DateTime DataAplicacao,
    int Bimestre);

/// <summary>
/// Formulário de avaliação. As regras espelham o UpdateAvaliacaoRequest da API.
/// </summary>
public sealed class AvaliacaoFormModel : IValidatableObject
{
    public static readonly string[] Campos =
        [nameof(Nome), nameof(Peso), nameof(DataAplicacao), nameof(Bimestre), nameof(SubDisciplinaId)];

    public int? Id { get; init; }

    public bool Edicao => Id is not null;

    [Required(ErrorMessage = "Informe o nome da avaliação.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o peso.")]
    [Range(typeof(decimal), "0.01", "100", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true,
        ErrorMessage = "O peso deve ser maior que 0 e no máximo 100.")]
    public decimal? Peso { get; set; }

    [Required(ErrorMessage = "Informe a data de aplicação.")]
    public DateOnly? DataAplicacao { get; set; }

    [Range(1, 4, ErrorMessage = "Escolha o bimestre.")]
    public int Bimestre { get; set; }

    public int? SubDisciplinaId { get; set; }

    /// <summary>Quando o professor leciona só subdisciplinas, a avaliação precisa indicar uma delas.</summary>
    public bool SubDisciplinaObrigatoria { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (SubDisciplinaObrigatoria && SubDisciplinaId is null)
        {
            yield return new ValidationResult("Escolha a subdisciplina.", [nameof(SubDisciplinaId)]);
        }
    }

    public SalvarAvaliacaoRequest ParaRequest(int turmaId, int disciplinaId) => new(
        Edicao ? null : turmaId,
        Edicao ? null : disciplinaId,
        SubDisciplinaId,
        Nome.Trim(),
        Peso ?? 0,
        DataAplicacao!.Value.ToDateTime(TimeOnly.MinValue),
        Bimestre);
}

/// <summary>
/// Grade de notas de uma avaliação (GET api/avaliacoes/{id}/notas).
/// </summary>
public sealed record NotasDaAvaliacao(AvaliacaoItem Avaliacao, bool PodeLancar, int Lancadas, IReadOnlyList<NotaAluno> Alunos);

public sealed record NotaAluno(int AlunoId, string Nome, bool Ativo, decimal? Nota);

/// <summary>Item do PUT api/avaliacoes/{id}/notas (valor nulo remove a nota).</summary>
public sealed record NotaLancamento(int AlunoId, decimal? Valor);

/// <summary>
/// Chamada de uma aula (GET api/chamadas).
/// </summary>
public sealed record ChamadaInfo(
    int TurmaId,
    string TurmaNome,
    int DisciplinaId,
    string DisciplinaNome,
    DateOnly Data,
    int Bimestre,
    bool Registrada,
    bool PodeRegistrar,
    int Presentes,
    IReadOnlyList<ChamadaAluno> Alunos);

public sealed record ChamadaAluno(int AlunoId, string Nome, bool Ativo, bool? Presente);

/// <summary>Aula com chamada registrada (GET api/chamadas/aulas).</summary>
public sealed record AulaRegistrada(DateOnly Data, int Bimestre, int Presentes, int Total);

/// <summary>Item do PUT api/chamadas.</summary>
public sealed record PresencaLancamento(int AlunoId, bool Presente);
