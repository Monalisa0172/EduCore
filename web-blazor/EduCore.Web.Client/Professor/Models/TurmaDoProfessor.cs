using EduCore.Web.Client.Atribuicoes.Models;

namespace EduCore.Web.Client.Professor.Models;

/// <summary>
/// Turma vista pelo professor: as disciplinas que ele leciona nela (subdisciplinas agrupadas).
/// </summary>
public sealed record TurmaDoProfessor(int TurmaId, string Nome, int AnoLetivo, bool Ativa, IReadOnlyList<DisciplinaDoProfessor> Disciplinas)
{
    /// <summary>Agrupa as atribuições por turma (ano mais recente primeiro) e, dentro dela, por disciplina.</summary>
    public static List<TurmaDoProfessor> Agrupar(IEnumerable<AtribuicaoItem> atribuicoes)
        => atribuicoes
            .GroupBy(a => (a.TurmaId, a.TurmaNome, a.AnoLetivo, a.TurmaAtiva))
            .Select(t => new TurmaDoProfessor(
                t.Key.TurmaId,
                t.Key.TurmaNome,
                t.Key.AnoLetivo,
                t.Key.TurmaAtiva,
                t.GroupBy(a => (a.DisciplinaId, a.DisciplinaNome))
                    .Select(d => new DisciplinaDoProfessor(
                        d.Key.DisciplinaId,
                        d.Key.DisciplinaNome,
                        d.Where(a => a.SubDisciplinaNome is not null)
                            .Select(a => a.SubDisciplinaNome!)
                            .OrderBy(n => n, StringComparer.CurrentCulture)
                            .ToList()))
                    .OrderBy(d => d.Nome, StringComparer.CurrentCulture)
                    .ToList()))
            .OrderByDescending(t => t.AnoLetivo)
            .ThenBy(t => t.Nome, StringComparer.CurrentCulture)
            .ToList();
}

/// <summary>Disciplina lecionada; <see cref="SubDisciplinas"/> vazia = disciplina inteira.</summary>
public sealed record DisciplinaDoProfessor(int DisciplinaId, string Nome, IReadOnlyList<string> SubDisciplinas)
{
    public string Detalhe => SubDisciplinas.Count == 0 ? "Disciplina inteira" : string.Join(", ", SubDisciplinas);
}
