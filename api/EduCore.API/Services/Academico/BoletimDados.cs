namespace EduCore.API.Services.Academico;

/// <summary>Matrícula do aluno: turma e ano letivo.</summary>
public record TurmaDoAluno(int TurmaId, string Nome, int AnoLetivo, bool Ativa);

public record DisciplinaDoBoletim(int Id, string Nome);

/// <summary>Avaliação ativa da turma com a nota do aluno (nula se não lançada).</summary>
public record AvaliacaoDoAluno(
    int Id,
    int DisciplinaId,
    string Nome,
    string? SubDisciplinaNome,
    decimal Peso,
    DateTime DataAplicacao,
    int Bimestre,
    decimal? Nota);

public record PresencaDoAluno(int DisciplinaId, DateOnly Data, bool Presente);

/// <summary>Tudo o que o boletim de uma turma precisa, lido do banco de uma vez.</summary>
public record DadosDoBoletim(
    IReadOnlyList<DisciplinaDoBoletim> Disciplinas,
    IReadOnlyList<AvaliacaoDoAluno> Avaliacoes,
    IReadOnlyList<PresencaDoAluno> Presencas);
