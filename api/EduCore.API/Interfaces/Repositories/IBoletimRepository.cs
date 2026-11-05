using EduCore.API.Services.Academico;

namespace EduCore.API.Interfaces.Repositories;

public interface IBoletimRepository
{
    Task<string?> GetNomeDoAlunoAsync(int alunoId);

    /// <summary>Turmas em que o aluno tem matrícula, do ano mais recente para o mais antigo.</summary>
    Task<List<TurmaDoAluno>> GetTurmasDoAlunoAsync(int alunoId);

    /// <summary>
    /// Disciplinas da turma (atribuídas, avaliadas ou com chamada), avaliações ativas com a nota
    /// do aluno e as presenças dele na turma.
    /// </summary>
    Task<DadosDoBoletim> CarregarAsync(int alunoId, int turmaId);
}
