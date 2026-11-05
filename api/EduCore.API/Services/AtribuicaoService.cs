using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

/// <summary>
/// Atribuição de professores: quem leciona cada disciplina (ou subdisciplina) em cada turma.
/// Regras: uma atribuição ativa por turma + disciplina + subdisciplina, e a disciplina é
/// atribuída inteira OU dividida por subdisciplinas, nunca os dois ao mesmo tempo.
/// </summary>
public class AtribuicaoService
{
    private const string NaoEncontrada = "Atribuição não encontrada.";

    private readonly IAtribuicaoRepository _atribuicaoRepository;
    private readonly IProfessorRepository _professorRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IDisciplinaRepository _disciplinaRepository;
    private readonly ISubDisciplinaRepository _subDisciplinaRepository;

    public AtribuicaoService(
        IAtribuicaoRepository atribuicaoRepository,
        IProfessorRepository professorRepository,
        ITurmaRepository turmaRepository,
        IDisciplinaRepository disciplinaRepository,
        ISubDisciplinaRepository subDisciplinaRepository)
    {
        _atribuicaoRepository = atribuicaoRepository;
        _professorRepository = professorRepository;
        _turmaRepository = turmaRepository;
        _disciplinaRepository = disciplinaRepository;
        _subDisciplinaRepository = subDisciplinaRepository;
    }

    public Task<List<AtribuicaoResponseDTO>> ListarAsync(int? turmaId, int? professorId, int? disciplinaId, bool? ativo)
        => _atribuicaoRepository.ListarAsync(turmaId, professorId, disciplinaId, ativo);

    public Task<AtribuicaoResponseDTO?> GetByIdAsync(int id)
        => _atribuicaoRepository.GetByIdAsync(id);

    public async Task<ResultadoOperacao<AtribuicaoResponseDTO>> CriarAsync(AtribuicaoRequest request)
    {
        var turma = await _turmaRepository.GetEntityByIdAsync(request.TurmaId);
        if (turma is null)
            return Invalido(nameof(AtribuicaoRequest.TurmaId), "Turma não encontrada.");
        if (!turma.Ativo)
            return Invalido(nameof(AtribuicaoRequest.TurmaId), "A turma está inativa. Reative a turma para atribuir professores.");

        var validacao = await ValidarDisciplinaEProfessorAsync(request.DisciplinaId, request.SubDisciplinaId, request.ProfessorId);
        if (validacao.Erro is not null)
            return validacao.Erro;

        var conflito = await VerificarConflitoAsync(request.TurmaId, validacao.Disciplina!, request.SubDisciplinaId, validacao.SubDisciplina);
        if (conflito is not null)
            return conflito;

        var atribuicao = new Atribuicao
        {
            ProfessorId = request.ProfessorId,
            TurmaId = request.TurmaId,
            DisciplinaId = request.DisciplinaId,
            SubDisciplinaId = request.SubDisciplinaId,
            Ativo = true
        };

        await _atribuicaoRepository.AddAsync(atribuicao);

        return await RecarregarAsync(atribuicao.Id);
    }

    /// <summary>
    /// Troca o professor mantendo turma e disciplina (as avaliações e notas ficam onde estão).
    /// </summary>
    public async Task<ResultadoOperacao<AtribuicaoResponseDTO>> TrocarProfessorAsync(int id, TrocarProfessorRequest request)
    {
        var atribuicao = await _atribuicaoRepository.GetEntityByIdAsync(id);
        if (atribuicao is null)
            return ResultadoOperacao<AtribuicaoResponseDTO>.NaoEncontrado(NaoEncontrada);

        if (!atribuicao.Ativo)
            return Invalido(nameof(TrocarProfessorRequest.ProfessorId), "A atribuição está encerrada. Reative-a antes de trocar o professor.");

        if (atribuicao.ProfessorId != request.ProfessorId)
        {
            var erroProfessor = await ValidarProfessorAsync(request.ProfessorId);
            if (erroProfessor is not null)
                return erroProfessor;

            atribuicao.ProfessorId = request.ProfessorId;
            await _atribuicaoRepository.SalvarAsync();
        }

        return await RecarregarAsync(id);
    }

    /// <summary>
    /// Encerra a atribuição (exclusão lógica): o histórico de quem lecionou é mantido.
    /// </summary>
    public async Task<ResultadoOperacao<AtribuicaoResponseDTO>> InativarAsync(int id)
    {
        var atribuicao = await _atribuicaoRepository.GetEntityByIdAsync(id);
        if (atribuicao is null)
            return ResultadoOperacao<AtribuicaoResponseDTO>.NaoEncontrado(NaoEncontrada);

        if (atribuicao.Ativo)
        {
            atribuicao.Ativo = false;
            await _atribuicaoRepository.SalvarAsync();
        }

        return await RecarregarAsync(id);
    }

    /// <summary>
    /// Reativa a atribuição, desde que turma, disciplina e professor continuem ativos e não haja conflito.
    /// </summary>
    public async Task<ResultadoOperacao<AtribuicaoResponseDTO>> ReativarAsync(int id)
    {
        var atribuicao = await _atribuicaoRepository.GetEntityByIdAsync(id);
        if (atribuicao is null)
            return ResultadoOperacao<AtribuicaoResponseDTO>.NaoEncontrado(NaoEncontrada);

        if (atribuicao.Ativo)
            return await RecarregarAsync(id);

        var turma = await _turmaRepository.GetEntityByIdAsync(atribuicao.TurmaId);
        if (turma is null || !turma.Ativo)
            return Invalido(nameof(AtribuicaoRequest.TurmaId), "A turma está inativa. Reative a turma primeiro.");

        var validacao = await ValidarDisciplinaEProfessorAsync(atribuicao.DisciplinaId, atribuicao.SubDisciplinaId, atribuicao.ProfessorId);
        if (validacao.Erro is not null)
            return validacao.Erro;

        var conflito = await VerificarConflitoAsync(atribuicao.TurmaId, validacao.Disciplina!, atribuicao.SubDisciplinaId, validacao.SubDisciplina);
        if (conflito is not null)
            return conflito;

        atribuicao.Ativo = true;
        await _atribuicaoRepository.SalvarAsync();

        return await RecarregarAsync(id);
    }

    private sealed record Validacao(
        ResultadoOperacao<AtribuicaoResponseDTO>? Erro,
        Disciplina? Disciplina = null,
        SubDisciplina? SubDisciplina = null);

    private async Task<Validacao> ValidarDisciplinaEProfessorAsync(int disciplinaId, int? subDisciplinaId, int professorId)
    {
        var disciplina = await _disciplinaRepository.GetEntityByIdAsync(disciplinaId);
        if (disciplina is null)
            return new(Invalido(nameof(AtribuicaoRequest.DisciplinaId), "Disciplina não encontrada."));
        if (!disciplina.Ativo)
            return new(Invalido(nameof(AtribuicaoRequest.DisciplinaId), $"A disciplina {disciplina.Nome} está inativa."));

        SubDisciplina? subDisciplina = null;
        if (subDisciplinaId.HasValue)
        {
            subDisciplina = await _subDisciplinaRepository.GetEntityByIdAsync(subDisciplinaId.Value);
            if (subDisciplina is null || subDisciplina.DisciplinaId != disciplinaId)
                return new(Invalido(nameof(AtribuicaoRequest.SubDisciplinaId), $"A subdisciplina não pertence a {disciplina.Nome}."));
            if (!subDisciplina.Ativo)
                return new(Invalido(nameof(AtribuicaoRequest.SubDisciplinaId), $"A subdisciplina {subDisciplina.Nome} está inativa."));
        }

        var erroProfessor = await ValidarProfessorAsync(professorId);
        if (erroProfessor is not null)
            return new(erroProfessor);

        return new(null, disciplina, subDisciplina);
    }

    private async Task<ResultadoOperacao<AtribuicaoResponseDTO>?> ValidarProfessorAsync(int professorId)
    {
        var professor = await _professorRepository.GetEntityByIdAsync(professorId);
        if (professor is null)
            return Invalido(nameof(AtribuicaoRequest.ProfessorId), "Professor não encontrado.");

        if (!professor.Funcionario.Ativo || !professor.Funcionario.Usuario.Ativo)
            return Invalido(nameof(AtribuicaoRequest.ProfessorId), $"{professor.Funcionario.Usuario.Nome} está inativo e não pode receber atribuições.");

        return null;
    }

    /// <summary>
    /// Disciplina inteira OU dividida por subdisciplinas, e um professor por item.
    /// </summary>
    private async Task<ResultadoOperacao<AtribuicaoResponseDTO>?> VerificarConflitoAsync(
        int turmaId, Disciplina disciplina, int? subDisciplinaId, SubDisciplina? subDisciplina)
    {
        var ativas = await _atribuicaoRepository.GetAtivasDaDisciplinaAsync(turmaId, disciplina.Id);

        var mesmoItem = ativas.FirstOrDefault(a => a.SubDisciplinaId == subDisciplinaId);
        if (mesmoItem is not null)
        {
            var item = subDisciplina?.Nome ?? disciplina.Nome;
            return ResultadoOperacao<AtribuicaoResponseDTO>.Conflito(
                subDisciplinaId.HasValue ? nameof(AtribuicaoRequest.SubDisciplinaId) : nameof(AtribuicaoRequest.DisciplinaId),
                $"{mesmoItem.ProfessorNome} já leciona {item} nesta turma. Para substituir, troque o professor dessa atribuição.");
        }

        if (subDisciplinaId is null)
        {
            var partes = ativas.Where(a => a.SubDisciplinaId.HasValue).Select(a => a.SubDisciplinaNome).ToList();
            if (partes.Count > 0)
            {
                return ResultadoOperacao<AtribuicaoResponseDTO>.Conflito(
                    nameof(AtribuicaoRequest.DisciplinaId),
                    $"{disciplina.Nome} já está dividida por subdisciplinas nesta turma ({string.Join(", ", partes)}). Atribua por subdisciplina ou encerre essas atribuições primeiro.");
            }
        }
        else
        {
            var inteira = ativas.FirstOrDefault(a => a.SubDisciplinaId is null);
            if (inteira is not null)
            {
                return ResultadoOperacao<AtribuicaoResponseDTO>.Conflito(
                    nameof(AtribuicaoRequest.SubDisciplinaId),
                    $"{inteira.ProfessorNome} já leciona {disciplina.Nome} inteira nesta turma. Encerre essa atribuição para dividir a disciplina por subdisciplinas.");
            }
        }

        return null;
    }

    private async Task<ResultadoOperacao<AtribuicaoResponseDTO>> RecarregarAsync(int id)
    {
        var dto = await _atribuicaoRepository.GetByIdAsync(id);

        return dto is null
            ? ResultadoOperacao<AtribuicaoResponseDTO>.NaoEncontrado(NaoEncontrada)
            : ResultadoOperacao<AtribuicaoResponseDTO>.Ok(dto);
    }

    private static ResultadoOperacao<AtribuicaoResponseDTO> Invalido(string campo, string mensagem)
        => ResultadoOperacao<AtribuicaoResponseDTO>.Invalido(campo, mensagem);
}
