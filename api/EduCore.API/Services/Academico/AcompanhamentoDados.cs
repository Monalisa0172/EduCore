namespace EduCore.API.Services.Academico;

/// <summary>Avaliação ativa da turma com as notas de todos os alunos.</summary>
public record AvaliacaoDaTurma(
    int Id,
    string Nome,
    int? SubDisciplinaId,
    string? SubDisciplinaNome,
    decimal Peso,
    DateTime DataAplicacao,
    int Bimestre,
    IReadOnlyList<NotaDoAluno> Notas);

public record NotaDoAluno(int AlunoId, decimal Valor);

/// <summary>Registro de chamada de um aluno na disciplina da turma.</summary>
public record PresencaDaTurma(int AlunoId, DateOnly Data, bool Presente);
