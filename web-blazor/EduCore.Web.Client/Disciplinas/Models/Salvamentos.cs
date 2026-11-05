namespace EduCore.Web.Client.Disciplinas.Models;

/// <summary>Disciplina gravada pelo diálogo; <paramref name="Nova"/> indica cadastro (e não edição).</summary>
public sealed record DisciplinaSalva(DisciplinaItem Disciplina, bool Nova);

/// <summary>Subdisciplina gravada pelo diálogo; <paramref name="Nova"/> indica cadastro (e não edição).</summary>
public sealed record SubDisciplinaSalva(SubDisciplinaItem SubDisciplina, bool Nova);
