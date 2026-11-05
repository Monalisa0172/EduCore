using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

/// <summary>
/// Avaliações por turma + disciplina + bimestre. Regras:
/// - a disciplina precisa ter professor atribuído na turma;
/// - se a disciplina está dividida por subdisciplinas, a avaliação indica qual delas;
/// - os pesos das avaliações ativas de turma + disciplina + bimestre somam no máximo 100
///   (a média do bimestre fecha quando a soma chega a 100).
/// </summary>
public class AvaliacaoService
{
    public const decimal PesoMaximo = 100m;

    private const string NaoEncontrada = "Avaliação não encontrada.";

    private readonly IAvaliacaoRepository _avaliacaoRepository;
    private readonly IAtribuicaoRepository _atribuicaoRepository;
    private readonly ITurmaRepository _turmaRepository;
    private readonly IDisciplinaRepository _disciplinaRepository;
    private readonly ISubDisciplinaRepository _subDisciplinaRepository;

    public AvaliacaoService(
        IAvaliacaoRepository avaliacaoRepository,
        IAtribuicaoRepository atribuicaoRepository,
        ITurmaRepository turmaRepository,
        IDisciplinaRepository disciplinaRepository,
        ISubDisciplinaRepository subDisciplinaRepository)
    {
        _avaliacaoRepository = avaliacaoRepository;
        _atribuicaoRepository = atribuicaoRepository;
        _turmaRepository = turmaRepository;
        _disciplinaRepository = disciplinaRepository;
        _subDisciplinaRepository = subDisciplinaRepository;
    }

    public Task<List<AvaliacaoResponseDTO>> ListarAsync(int? turmaId, int? disciplinaId, int? bimestre, bool? ativo)
        => _avaliacaoRepository.ListarAsync(turmaId, disciplinaId, bimestre, ativo);

    public Task<AvaliacaoResponseDTO?> GetByIdAsync(int id)
        => _avaliacaoRepository.GetByIdAsync(id);

    public async Task<ResultadoOperacao<AvaliacaoResponseDTO>> CriarAsync(CreateAvaliacaoRequest request)
    {
        var turma = await _turmaRepository.GetEntityByIdAsync(request.TurmaId);
        if (turma is null)
            return Invalido(nameof(CreateAvaliacaoRequest.TurmaId), "Turma não encontrada.");
        if (!turma.Ativo)
            return Invalido(nameof(CreateAvaliacaoRequest.TurmaId), "A turma está inativa e não recebe novas avaliações.");

        var disciplina = await _disciplinaRepository.GetEntityByIdAsync(request.DisciplinaId);
        if (disciplina is null)
            return Invalido(nameof(CreateAvaliacaoRequest.DisciplinaId), "Disciplina não encontrada.");
        if (!disciplina.Ativo)
            return Invalido(nameof(CreateAvaliacaoRequest.DisciplinaId), $"A disciplina {disciplina.Nome} está inativa.");

        var erroSub = await ValidarSubDisciplinaAsync(turma.Id, disciplina, request.SubDisciplinaId);
        if (erroSub is not null)
            return erroSub;

        var erroPeso = await ValidarPesoAsync(turma.Id, disciplina, (int)request.Bimestre, request.Peso, null);
        if (erroPeso is not null)
            return erroPeso;

        var avaliacao = new Avaliacao
        {
            TurmaId = turma.Id,
            DisciplinaId = disciplina.Id,
            SubDisciplinaId = request.SubDisciplinaId,
            Nome = request.Nome.Trim(),
            Peso = request.Peso,
            DataAplicacao = request.DataAplicacao!.Value,
            Bimestre = (int)request.Bimestre,
            Ativo = true
        };

        await _avaliacaoRepository.AddAsync(avaliacao);

        return await RecarregarAsync(avaliacao.Id);
    }

    public async Task<ResultadoOperacao<AvaliacaoResponseDTO>> AtualizarAsync(int id, UpdateAvaliacaoRequest request)
    {
        var avaliacao = await _avaliacaoRepository.GetEntityByIdAsync(id);
        if (avaliacao is null)
            return ResultadoOperacao<AvaliacaoResponseDTO>.NaoEncontrado(NaoEncontrada);

        if (!avaliacao.Ativo)
            return Invalido(nameof(UpdateAvaliacaoRequest.Nome), "A avaliação está inativa. Reative-a para editar.");

        var disciplina = await _disciplinaRepository.GetEntityByIdAsync(avaliacao.DisciplinaId);
        if (disciplina is null)
            return Invalido(nameof(CreateAvaliacaoRequest.DisciplinaId), "Disciplina não encontrada.");

        if (request.SubDisciplinaId != avaliacao.SubDisciplinaId)
        {
            var erroSub = await ValidarSubDisciplinaAsync(avaliacao.TurmaId, disciplina, request.SubDisciplinaId);
            if (erroSub is not null)
                return erroSub;
        }

        var erroPeso = await ValidarPesoAsync(avaliacao.TurmaId, disciplina, (int)request.Bimestre, request.Peso, avaliacao.Id);
        if (erroPeso is not null)
            return erroPeso;

        avaliacao.Nome = request.Nome.Trim();
        avaliacao.SubDisciplinaId = request.SubDisciplinaId;
        avaliacao.Peso = request.Peso;
        avaliacao.DataAplicacao = request.DataAplicacao!.Value;
        avaliacao.Bimestre = (int)request.Bimestre;

        await _avaliacaoRepository.SalvarAsync();

        return await RecarregarAsync(id);
    }

    /// <summary>
    /// Inativa a avaliação: as notas lançadas ficam guardadas, mas o peso sai da média do bimestre.
    /// </summary>
    public async Task<ResultadoOperacao<AvaliacaoResponseDTO>> InativarAsync(int id)
    {
        var avaliacao = await _avaliacaoRepository.GetEntityByIdAsync(id);
        if (avaliacao is null)
            return ResultadoOperacao<AvaliacaoResponseDTO>.NaoEncontrado(NaoEncontrada);

        if (avaliacao.Ativo)
        {
            avaliacao.Ativo = false;
            await _avaliacaoRepository.SalvarAsync();
        }

        return await RecarregarAsync(id);
    }

    /// <summary>
    /// Reativa a avaliação se o peso dela ainda couber no bimestre.
    /// </summary>
    public async Task<ResultadoOperacao<AvaliacaoResponseDTO>> ReativarAsync(int id)
    {
        var avaliacao = await _avaliacaoRepository.GetEntityByIdAsync(id);
        if (avaliacao is null)
            return ResultadoOperacao<AvaliacaoResponseDTO>.NaoEncontrado(NaoEncontrada);

        if (!avaliacao.Ativo)
        {
            var disciplina = await _disciplinaRepository.GetEntityByIdAsync(avaliacao.DisciplinaId);
            var erroPeso = await ValidarPesoAsync(avaliacao.TurmaId, disciplina!, avaliacao.Bimestre, avaliacao.Peso, avaliacao.Id);
            if (erroPeso is not null)
                return erroPeso;

            avaliacao.Ativo = true;
            await _avaliacaoRepository.SalvarAsync();
        }

        return await RecarregarAsync(id);
    }

    /// <summary>
    /// A disciplina precisa ser lecionada na turma. Dividida por subdisciplinas: a avaliação indica
    /// uma subdisciplina atribuída. Inteira: a avaliação pode (opcionalmente) indicar uma subdisciplina dela.
    /// </summary>
    private async Task<ResultadoOperacao<AvaliacaoResponseDTO>?> ValidarSubDisciplinaAsync(int turmaId, Disciplina disciplina, int? subDisciplinaId)
    {
        var atribuicoes = await _atribuicaoRepository.GetAtivasDaDisciplinaAsync(turmaId, disciplina.Id);
        if (atribuicoes.Count == 0)
        {
            return Invalido(nameof(CreateAvaliacaoRequest.DisciplinaId),
                $"{disciplina.Nome} ainda não tem professor atribuído nesta turma. Faça a atribuição antes de criar avaliações.");
        }

        var dividida = atribuicoes.All(a => a.SubDisciplinaId.HasValue);

        if (subDisciplinaId is null)
        {
            if (dividida)
            {
                var partes = string.Join(", ", atribuicoes.Select(a => a.SubDisciplinaNome));
                return Invalido(nameof(UpdateAvaliacaoRequest.SubDisciplinaId),
                    $"{disciplina.Nome} é dividida por subdisciplinas nesta turma ({partes}). Informe a subdisciplina da avaliação.");
            }

            return null;
        }

        var subDisciplina = await _subDisciplinaRepository.GetEntityByIdAsync(subDisciplinaId.Value);
        if (subDisciplina is null || subDisciplina.DisciplinaId != disciplina.Id)
            return Invalido(nameof(UpdateAvaliacaoRequest.SubDisciplinaId), $"A subdisciplina não pertence a {disciplina.Nome}.");
        if (!subDisciplina.Ativo)
            return Invalido(nameof(UpdateAvaliacaoRequest.SubDisciplinaId), $"A subdisciplina {subDisciplina.Nome} está inativa.");

        if (dividida && !atribuicoes.Any(a => a.SubDisciplinaId == subDisciplinaId))
        {
            return Invalido(nameof(UpdateAvaliacaoRequest.SubDisciplinaId),
                $"{subDisciplina.Nome} não tem professor atribuído nesta turma.");
        }

        return null;
    }

    private async Task<ResultadoOperacao<AvaliacaoResponseDTO>?> ValidarPesoAsync(
        int turmaId, Disciplina disciplina, int bimestre, decimal peso, int? avaliacaoIgnoradaId)
    {
        var pesoAtual = await _avaliacaoRepository.GetPesoTotalAsync(turmaId, disciplina.Id, bimestre, avaliacaoIgnoradaId);

        if (pesoAtual + peso <= PesoMaximo)
            return null;

        var disponivel = PesoMaximo - pesoAtual;
        var texto = disponivel <= 0
            ? $"Os pesos de {disciplina.Nome} no {bimestre}º bimestre já somam 100. Ajuste ou inative outra avaliação primeiro."
            : $"Os pesos de {disciplina.Nome} no {bimestre}º bimestre já somam {pesoAtual:0.##}. O peso desta avaliação pode ser no máximo {disponivel:0.##}.";

        return Invalido(nameof(UpdateAvaliacaoRequest.Peso), texto);
    }

    private async Task<ResultadoOperacao<AvaliacaoResponseDTO>> RecarregarAsync(int id)
    {
        var dto = await _avaliacaoRepository.GetByIdAsync(id);

        return dto is null
            ? ResultadoOperacao<AvaliacaoResponseDTO>.NaoEncontrado(NaoEncontrada)
            : ResultadoOperacao<AvaliacaoResponseDTO>.Ok(dto);
    }

    private static ResultadoOperacao<AvaliacaoResponseDTO> Invalido(string campo, string mensagem)
        => ResultadoOperacao<AvaliacaoResponseDTO>.Invalido(campo, mensagem);
}
