using System.ComponentModel.DataAnnotations;

namespace EduCore.Web.Client.Disciplinas.Models;

/// <summary>
/// Estado do formulário de disciplina. As regras espelham o CreateDisciplinaRequest da API.
/// </summary>
public sealed class DisciplinaFormModel
{
    /// <summary>Nomes dos campos, usados para ligar os erros devolvidos pela API ao campo certo.</summary>
    public static readonly string[] Campos = [nameof(Nome), nameof(Codigo), nameof(Descricao), nameof(CargaHoraria)];

    public int? Id { get; private init; }

    public bool Edicao => Id is not null;

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o código.")]
    [StringLength(20, ErrorMessage = "O código deve ter no máximo 20 caracteres.")]
    [RegularExpression(@"^\s*[A-Za-z0-9_-]+\s*$", ErrorMessage = "Use apenas letras, números, hífen ou sublinhado, sem espaços.")]
    public string Codigo { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres.")]
    public string? Descricao { get; set; }

    [Required(ErrorMessage = "Informe a carga horária.")]
    [Range(1, 2000, ErrorMessage = "A carga horária deve estar entre 1 e 2000 horas.")]
    public int? CargaHoraria { get; set; }

    public static DisciplinaFormModel Nova() => new();

    public static DisciplinaFormModel De(DisciplinaItem disciplina) => new()
    {
        Id = disciplina.Id,
        Nome = disciplina.Nome,
        Codigo = disciplina.Codigo,
        Descricao = disciplina.Descricao,
        CargaHoraria = disciplina.CargaHoraria
    };

    public SalvarDisciplinaRequest ParaRequest() => new(
        Nome.Trim(),
        Codigo.Trim().ToUpperInvariant(),
        string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
        CargaHoraria ?? 0);
}

/// <summary>
/// Estado do formulário de subdisciplina. As regras espelham o CreateSubDisciplinaRequest da API.
/// </summary>
public sealed class SubDisciplinaFormModel
{
    public static readonly string[] Campos = [nameof(Nome), nameof(Descricao)];

    public int? Id { get; private init; }

    public int DisciplinaId { get; private init; }

    public bool Edicao => Id is not null;

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(255, ErrorMessage = "A descrição deve ter no máximo 255 caracteres.")]
    public string? Descricao { get; set; }

    public static SubDisciplinaFormModel Nova(int disciplinaId) => new() { DisciplinaId = disciplinaId };

    public static SubDisciplinaFormModel De(SubDisciplinaItem subDisciplina) => new()
    {
        Id = subDisciplina.Id,
        DisciplinaId = subDisciplina.DisciplinaId,
        Nome = subDisciplina.Nome,
        Descricao = subDisciplina.Descricao
    };

    public SalvarSubDisciplinaRequest ParaRequest() => new(
        Nome.Trim(),
        string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim(),
        DisciplinaId);
}
