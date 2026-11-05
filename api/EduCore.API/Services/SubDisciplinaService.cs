using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class SubDisciplinaService
{
    private const string NaoEncontrada = "Subdisciplina não encontrada.";

    private readonly ISubDisciplinaRepository
        _subDisciplinaRepository;

    private readonly IDisciplinaRepository
        _disciplinaRepository;

    public SubDisciplinaService(
        ISubDisciplinaRepository subDisciplinaRepository,
        IDisciplinaRepository disciplinaRepository)
    {
        _subDisciplinaRepository = subDisciplinaRepository;
        _disciplinaRepository = disciplinaRepository;
    }

    public async Task<List<SubDisciplinaResponseDTO>>
        GetAllAsync()
    {
        return await _subDisciplinaRepository
            .GetAllAsync();
    }

    public async Task<SubDisciplinaResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _subDisciplinaRepository
            .GetByIdAsync(id);
    }

    public async Task<List<SubDisciplinaResponseDTO>>
        GetByDisciplinaAsync(int disciplinaId)
    {
        return await _subDisciplinaRepository
            .GetByDisciplinaAsync(disciplinaId);
    }

    public async Task<ResultadoOperacao<SubDisciplinaResponseDTO>>
        CreateAsync(CreateSubDisciplinaRequest request)
    {
        var disciplina = await _disciplinaRepository
            .GetEntityByIdAsync(request.DisciplinaId);

        if (ValidarDisciplina(disciplina) is { } disciplinaInvalida)
        {
            return disciplinaInvalida;
        }

        var nome = (request.Nome ?? string.Empty).Trim();

        if (await NomeEmUsoAsync(nome, request.DisciplinaId, ignorarId: null))
        {
            return NomeDuplicado(nome);
        }

        var subDisciplina = new SubDisciplina
        {
            Nome = nome,
            Descricao = TextoOpcional(request.Descricao),
            DisciplinaId = request.DisciplinaId,
            Ativo = true
        };

        await _subDisciplinaRepository
            .AddAsync(subDisciplina);

        return ResultadoOperacao<SubDisciplinaResponseDTO>.Ok(ParaDto(subDisciplina, disciplina!.Nome));
    }

    public async Task<ResultadoOperacao<SubDisciplinaResponseDTO>> UpdateAsync(
        int id,
        CreateSubDisciplinaRequest request)
    {
        var subDisciplina =
            await _subDisciplinaRepository
                .GetEntityByIdAsync(id);

        if (subDisciplina == null)
        {
            return ResultadoOperacao<SubDisciplinaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        // Mudar de disciplina exige que a nova exista e esteja ativa.
        if (request.DisciplinaId != subDisciplina.DisciplinaId)
        {
            var novaDisciplina = await _disciplinaRepository
                .GetEntityByIdAsync(request.DisciplinaId);

            if (ValidarDisciplina(novaDisciplina) is { } disciplinaInvalida)
            {
                return disciplinaInvalida;
            }
        }

        var nome = (request.Nome ?? string.Empty).Trim();

        if (await NomeEmUsoAsync(nome, request.DisciplinaId, ignorarId: id))
        {
            return NomeDuplicado(nome);
        }

        subDisciplina.Nome = nome;
        subDisciplina.Descricao = TextoOpcional(request.Descricao);
        subDisciplina.DisciplinaId = request.DisciplinaId;

        await _subDisciplinaRepository
            .UpdateAsync(subDisciplina);

        return await RecarregarAsync(subDisciplina);
    }

    /// <summary>
    /// Inativa a subdisciplina (exclusão lógica): avaliações e notas já lançadas são mantidas.
    /// </summary>
    public async Task<ResultadoOperacao<SubDisciplinaResponseDTO>> InativarAsync(int id)
    {
        var subDisciplina = await _subDisciplinaRepository
            .GetEntityByIdAsync(id);

        if (subDisciplina == null)
        {
            return ResultadoOperacao<SubDisciplinaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        if (subDisciplina.Ativo)
        {
            subDisciplina.Ativo = false;
            await _subDisciplinaRepository.UpdateAsync(subDisciplina);
        }

        return await RecarregarAsync(subDisciplina);
    }

    public async Task<ResultadoOperacao<SubDisciplinaResponseDTO>> ReativarAsync(int id)
    {
        var subDisciplina = await _subDisciplinaRepository
            .GetEntityByIdAsync(id);

        if (subDisciplina == null)
        {
            return ResultadoOperacao<SubDisciplinaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        var disciplina = await _disciplinaRepository
            .GetEntityByIdAsync(subDisciplina.DisciplinaId);

        if (disciplina is not { Ativo: true })
        {
            return ResultadoOperacao<SubDisciplinaResponseDTO>.Invalido(
                nameof(CreateSubDisciplinaRequest.DisciplinaId),
                "Reative a disciplina antes de reativar a subdisciplina.");
        }

        if (!subDisciplina.Ativo)
        {
            subDisciplina.Ativo = true;
            await _subDisciplinaRepository.UpdateAsync(subDisciplina);
        }

        return await RecarregarAsync(subDisciplina);
    }

    private static ResultadoOperacao<SubDisciplinaResponseDTO>? ValidarDisciplina(Disciplina? disciplina)
    {
        if (disciplina is null)
        {
            return ResultadoOperacao<SubDisciplinaResponseDTO>.Invalido(
                nameof(CreateSubDisciplinaRequest.DisciplinaId),
                "Disciplina não encontrada.");
        }

        if (!disciplina.Ativo)
        {
            return ResultadoOperacao<SubDisciplinaResponseDTO>.Invalido(
                nameof(CreateSubDisciplinaRequest.DisciplinaId),
                "A disciplina está inativa. Reative a disciplina para gerenciar as subdisciplinas.");
        }

        return null;
    }

    private async Task<bool> NomeEmUsoAsync(string nome, int disciplinaId, int? ignorarId)
    {
        var existente = await _subDisciplinaRepository
            .GetByNomeAndDisciplinaAsync(nome, disciplinaId);

        return existente is not null && existente.Id != ignorarId;
    }

    private async Task<ResultadoOperacao<SubDisciplinaResponseDTO>> RecarregarAsync(SubDisciplina subDisciplina)
    {
        var dto = await _subDisciplinaRepository.GetByIdAsync(subDisciplina.Id)
            ?? ParaDto(subDisciplina, nomeDisciplina: null);

        return ResultadoOperacao<SubDisciplinaResponseDTO>.Ok(dto);
    }

    private static ResultadoOperacao<SubDisciplinaResponseDTO> NomeDuplicado(string nome)
        => ResultadoOperacao<SubDisciplinaResponseDTO>.Conflito(
            nameof(CreateSubDisciplinaRequest.Nome),
            $"Já existe a subdisciplina \"{nome}\" nesta disciplina.");

    private static string? TextoOpcional(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static SubDisciplinaResponseDTO ParaDto(SubDisciplina subDisciplina, string? nomeDisciplina) => new()
    {
        Id = subDisciplina.Id,
        Nome = subDisciplina.Nome ?? string.Empty,
        Descricao = subDisciplina.Descricao,
        DisciplinaId = subDisciplina.DisciplinaId,
        Disciplina = nomeDisciplina ?? string.Empty,
        Ativo = subDisciplina.Ativo
    };
}
