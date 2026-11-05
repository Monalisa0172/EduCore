namespace EduCore.Web.Client.Disciplinas.Models;

/// <summary>
/// Disciplina retornada pela API (GET api/disciplinas).
/// </summary>
public sealed record DisciplinaItem(
    int Id,
    string Nome,
    string Codigo,
    string? Descricao,
    int? CargaHoraria,
    bool Ativo,
    int QuantidadeSubDisciplinas)
{
    // Disciplinas antigas foram cadastradas sem codigo e sem carga horaria.
    public bool TemCodigo => !string.IsNullOrWhiteSpace(Codigo);

    public string CodigoTexto => TemCodigo ? Codigo : "Sem código";

    public string CargaHorariaTexto => CargaHoraria is > 0 ? $"{CargaHoraria} h" : "Não informada";

    public string SubDisciplinasTexto => QuantidadeSubDisciplinas switch
    {
        0 => "Nenhuma",
        1 => "1 subdisciplina",
        var n => $"{n} subdisciplinas"
    };
}

/// <summary>
/// Subdisciplina retornada pela API (GET api/subdisciplinas/disciplina/{id}).
/// </summary>
public sealed record SubDisciplinaItem(
    int Id,
    string Nome,
    string? Descricao,
    int DisciplinaId,
    string Disciplina,
    bool Ativo);

/// <summary>Corpo de POST/PUT api/disciplinas.</summary>
public sealed record SalvarDisciplinaRequest(string Nome, string Codigo, string? Descricao, int CargaHoraria);

/// <summary>Corpo de POST/PUT api/subdisciplinas.</summary>
public sealed record SalvarSubDisciplinaRequest(string Nome, string? Descricao, int DisciplinaId);
