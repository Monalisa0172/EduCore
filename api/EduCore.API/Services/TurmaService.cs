using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class TurmaService
{
    private const string NaoEncontrada = "Turma não encontrada.";

    private readonly ITurmaRepository _turmaRepository;

    public TurmaService(ITurmaRepository turmaRepository)
    {
        _turmaRepository = turmaRepository;
    }

    public async Task<List<TurmaResponseDTO>>
        GetAllAsync()
    {
        return await _turmaRepository
            .GetAllAsync();
    }

    public async Task<TurmaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _turmaRepository
            .GetByIdAsync(id);
    }

    /// <summary>
    /// Alunos matriculados na turma; nulo quando a turma não existe.
    /// </summary>
    public async Task<List<TurmaAlunoDTO>?> GetAlunosAsync(int id)
    {
        if (await _turmaRepository.GetEntityByIdAsync(id) is null)
        {
            return null;
        }

        return await _turmaRepository.GetAlunosAsync(id);
    }

    public async Task<ResultadoOperacao<TurmaResponseDTO>>
        CreateAsync(
            CreateTurmaRequest request)
    {
        var nome = (request.Nome ?? string.Empty).Trim();

        if (await _turmaRepository.GetByNomeEAnoAsync(nome, request.AnoLetivo) is not null)
        {
            return NomeEmUso(nome, request.AnoLetivo);
        }

        var turma = new Turma
        {
            Nome = nome,
            AnoLetivo = request.AnoLetivo,
            Ativo = true
        };

        await _turmaRepository
            .AddAsync(turma);

        return await RecarregarAsync(turma);
    }

    public async Task<ResultadoOperacao<TurmaResponseDTO>> UpdateAsync(
        int id,
        CreateTurmaRequest request)
    {
        var turma = await _turmaRepository
            .GetEntityByIdAsync(id);

        if (turma == null)
        {
            return ResultadoOperacao<TurmaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        var nome = (request.Nome ?? string.Empty).Trim();
        var mesmoNome = await _turmaRepository.GetByNomeEAnoAsync(nome, request.AnoLetivo);

        if (mesmoNome is not null && mesmoNome.Id != id)
        {
            return NomeEmUso(nome, request.AnoLetivo);
        }

        turma.Nome = nome;
        turma.AnoLetivo = request.AnoLetivo;

        await _turmaRepository
            .UpdateAsync(turma);

        return await RecarregarAsync(turma);
    }

    /// <summary>
    /// Inativa a turma (exclusão lógica): as matrículas ficam no histórico,
    /// mas a turma deixa de aceitar novos alunos.
    /// </summary>
    public Task<ResultadoOperacao<TurmaResponseDTO>> InativarAsync(int id)
        => AlterarStatusAsync(id, ativo: false);

    public Task<ResultadoOperacao<TurmaResponseDTO>> ReativarAsync(int id)
        => AlterarStatusAsync(id, ativo: true);

    private async Task<ResultadoOperacao<TurmaResponseDTO>> AlterarStatusAsync(int id, bool ativo)
    {
        var turma =
            await _turmaRepository
                .GetEntityByIdAsync(id);

        if (turma == null)
        {
            return ResultadoOperacao<TurmaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        if (turma.Ativo != ativo)
        {
            turma.Ativo = ativo;
            await _turmaRepository.UpdateAsync(turma);
        }

        return await RecarregarAsync(turma);
    }

    /// <summary>
    /// Relê pelo repositório para devolver a contagem de alunos atualizada.
    /// </summary>
    private async Task<ResultadoOperacao<TurmaResponseDTO>> RecarregarAsync(Turma turma)
    {
        var dto = await _turmaRepository.GetByIdAsync(turma.Id) ?? new TurmaResponseDTO
        {
            Id = turma.Id,
            Nome = turma.Nome ?? string.Empty,
            AnoLetivo = turma.AnoLetivo,
            Ativo = turma.Ativo
        };

        return ResultadoOperacao<TurmaResponseDTO>.Ok(dto);
    }

    private static ResultadoOperacao<TurmaResponseDTO> NomeEmUso(string nome, int anoLetivo)
        => ResultadoOperacao<TurmaResponseDTO>.Conflito(
            nameof(CreateTurmaRequest.Nome),
            $"Já existe a turma \"{nome}\" no ano letivo {anoLetivo}.");
}
