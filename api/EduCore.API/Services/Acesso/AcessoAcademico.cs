using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services.Acesso;

/// <summary>
/// Regras de "quem pode mexer em quê" no acadêmico. Admin pode tudo; o professor só o que leciona
/// (atribuição ativa). Disciplina atribuída inteira dá acesso a todas as avaliações dela;
/// atribuída por subdisciplina, só às avaliações daquela subdisciplina.
/// </summary>
public interface IAcessoAcademico
{
    bool EhAdmin { get; }

    /// <summary>Professor logado (nulo para admin, aluno ou professor inativo).</summary>
    Task<int?> ProfessorIdAsync();

    /// <summary>Aluno logado (nulo para quem não é aluno ou está inativo).</summary>
    Task<int?> AlunoIdAsync();

    Task<bool> PodeGerenciarAvaliacaoAsync(int turmaId, int disciplinaId, int? subDisciplinaId);

    Task<bool> PodeFazerChamadaAsync(int turmaId, int disciplinaId);
}

public class AcessoAcademico : IAcessoAcademico
{
    private readonly IUsuarioAtual _usuarioAtual;
    private readonly IProfessorRepository _professorRepository;
    private readonly IAlunoRepository _alunoRepository;
    private readonly IAtribuicaoRepository _atribuicaoRepository;

    // Escopo da requisição: resolve o professor uma vez só.
    private Task<int?>? _professorId;

    public AcessoAcademico(
        IUsuarioAtual usuarioAtual,
        IProfessorRepository professorRepository,
        IAlunoRepository alunoRepository,
        IAtribuicaoRepository atribuicaoRepository)
    {
        _usuarioAtual = usuarioAtual;
        _professorRepository = professorRepository;
        _alunoRepository = alunoRepository;
        _atribuicaoRepository = atribuicaoRepository;
    }

    public bool EhAdmin => _usuarioAtual.EhAdmin;

    public Task<int?> ProfessorIdAsync()
        => _usuarioAtual.EhProfessor
            ? _professorId ??= _professorRepository.GetIdAtivoPorUsuarioAsync(_usuarioAtual.UsuarioId)
            : Task.FromResult<int?>(null);

    public Task<int?> AlunoIdAsync()
        => _usuarioAtual.EhAluno
            ? _alunoRepository.GetIdAtivoPorUsuarioAsync(_usuarioAtual.UsuarioId)
            : Task.FromResult<int?>(null);

    public async Task<bool> PodeGerenciarAvaliacaoAsync(int turmaId, int disciplinaId, int? subDisciplinaId)
    {
        if (EhAdmin)
            return true;

        var atribuicoes = await AtribuicoesDoProfessorAsync(turmaId, disciplinaId);

        return atribuicoes.Any(a => a.SubDisciplinaId is null || a.SubDisciplinaId == subDisciplinaId);
    }

    public async Task<bool> PodeFazerChamadaAsync(int turmaId, int disciplinaId)
    {
        if (EhAdmin)
            return true;

        return (await AtribuicoesDoProfessorAsync(turmaId, disciplinaId)).Count > 0;
    }

    private async Task<List<DTOs.Response.AtribuicaoResponseDTO>> AtribuicoesDoProfessorAsync(int turmaId, int disciplinaId)
    {
        var professorId = await ProfessorIdAsync();

        return professorId is null
            ? []
            : await _atribuicaoRepository.GetAtivasDoProfessorAsync(professorId.Value, turmaId, disciplinaId);
    }
}
