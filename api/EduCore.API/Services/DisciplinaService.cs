using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class DisciplinaService
{
    private const string NaoEncontrada = "Disciplina não encontrada.";

    private readonly IDisciplinaRepository _disciplinaRepository;

    public DisciplinaService(
        IDisciplinaRepository disciplinaRepository)
    {
        _disciplinaRepository = disciplinaRepository;
    }

    public async Task<List<DisciplinaResponseDTO>> GetAllAsync()
    {
        return await _disciplinaRepository.GetAllAsync();
    }

    public async Task<DisciplinaResponseDTO?> GetByIdAsync(int id)
    {
        return await _disciplinaRepository.GetByIdAsync(id);
    }

    public async Task<ResultadoOperacao<DisciplinaResponseDTO>> CreateAsync(
        CreateDisciplinaRequest request)
    {
        var codigo = NormalizarCodigo(request.Codigo);

        if (await _disciplinaRepository.GetByCodigoAsync(codigo) is not null)
        {
            return CodigoEmUso(codigo);
        }

        var disciplina = new Disciplina
        {
            Nome = request.Nome?.Trim(),
            Codigo = codigo,
            Descricao = TextoOpcional(request.Descricao),
            CargaHoraria = request.CargaHoraria,
            Ativo = true
        };

        await _disciplinaRepository.AddAsync(disciplina);

        return ResultadoOperacao<DisciplinaResponseDTO>.Ok(ParaDto(disciplina, quantidadeSubDisciplinas: 0));
    }

    public async Task<ResultadoOperacao<DisciplinaResponseDTO>> UpdateAsync(
        int id,
        CreateDisciplinaRequest request)
    {
        var disciplina = await _disciplinaRepository
            .GetEntityByIdAsync(id);

        if (disciplina == null)
        {
            return ResultadoOperacao<DisciplinaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        var codigo = NormalizarCodigo(request.Codigo);
        var mesmoCodigo = await _disciplinaRepository.GetByCodigoAsync(codigo);

        if (mesmoCodigo is not null && mesmoCodigo.Id != id)
        {
            return CodigoEmUso(codigo);
        }

        disciplina.Nome = request.Nome?.Trim();
        disciplina.Codigo = codigo;
        disciplina.Descricao = TextoOpcional(request.Descricao);
        disciplina.CargaHoraria = request.CargaHoraria;

        await _disciplinaRepository.UpdateAsync(disciplina);

        return await RecarregarAsync(disciplina);
    }

    /// <summary>
    /// Inativa a disciplina (exclusão lógica): o histórico de notas e presenças é mantido.
    /// </summary>
    public Task<ResultadoOperacao<DisciplinaResponseDTO>> InativarAsync(int id)
        => AlterarStatusAsync(id, ativo: false);

    public Task<ResultadoOperacao<DisciplinaResponseDTO>> ReativarAsync(int id)
        => AlterarStatusAsync(id, ativo: true);

    private async Task<ResultadoOperacao<DisciplinaResponseDTO>> AlterarStatusAsync(int id, bool ativo)
    {
        var disciplina = await _disciplinaRepository
            .GetEntityByIdAsync(id);

        if (disciplina == null)
        {
            return ResultadoOperacao<DisciplinaResponseDTO>.NaoEncontrado(NaoEncontrada);
        }

        if (disciplina.Ativo != ativo)
        {
            disciplina.Ativo = ativo;
            await _disciplinaRepository.UpdateAsync(disciplina);
        }

        return await RecarregarAsync(disciplina);
    }

    /// <summary>
    /// Relê pelo repositório para devolver a contagem de subdisciplinas atualizada.
    /// </summary>
    private async Task<ResultadoOperacao<DisciplinaResponseDTO>> RecarregarAsync(Disciplina disciplina)
    {
        var dto = await _disciplinaRepository.GetByIdAsync(disciplina.Id)
            ?? ParaDto(disciplina, quantidadeSubDisciplinas: 0);

        return ResultadoOperacao<DisciplinaResponseDTO>.Ok(dto);
    }

    private static ResultadoOperacao<DisciplinaResponseDTO> CodigoEmUso(string codigo)
        => ResultadoOperacao<DisciplinaResponseDTO>.Conflito(
            nameof(CreateDisciplinaRequest.Codigo),
            $"Já existe uma disciplina com o código {codigo}.");

    private static string NormalizarCodigo(string? codigo)
        => (codigo ?? string.Empty).Trim().ToUpperInvariant();

    private static string? TextoOpcional(string? texto)
        => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    private static DisciplinaResponseDTO ParaDto(Disciplina disciplina, int quantidadeSubDisciplinas) => new()
    {
        Id = disciplina.Id,
        Nome = disciplina.Nome ?? string.Empty,
        Codigo = disciplina.Codigo,
        Descricao = disciplina.Descricao,
        CargaHoraria = disciplina.CargaHoraria,
        Ativo = disciplina.Ativo,
        QuantidadeSubDisciplinas = quantidadeSubDisciplinas
    };
}
