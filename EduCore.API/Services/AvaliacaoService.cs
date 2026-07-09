using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class AvaliacaoService
{
    private readonly IAvaliacaoRepository
        _avaliacaoRepository;

    private readonly IDisciplinaRepository
        _disciplinaRepository;

    private readonly ISubDisciplinaRepository
        _subDisciplinaRepository;

    public AvaliacaoService(
        IAvaliacaoRepository avaliacaoRepository,
        ISubDisciplinaRepository subDisciplinaRepository,
        IDisciplinaRepository disciplinaRepository)
    {
        _avaliacaoRepository =
            avaliacaoRepository;

        _disciplinaRepository =
            disciplinaRepository;

        _subDisciplinaRepository =
            subDisciplinaRepository;
    }

    public async Task<List<AvaliacaoResponseDTO>>
        GetAllAsync()
    {
        return await _avaliacaoRepository
            .GetAllAsync();
    }

    public async Task<AvaliacaoResponseDTO?>
        GetByIdAsync(int id)
    {
        return await _avaliacaoRepository
            .GetByIdAsync(id);
    }

    public async Task<List<AvaliacaoResponseDTO>>
        GetByDisciplinaAsync(int disciplinaId)
    {
        return await _avaliacaoRepository
            .GetByDisciplinaAsync(disciplinaId);
    }

    public async Task<AvaliacaoResponseDTO?> CreateAsync(CreateAvaliacaoRequest request)
    {
        var disciplina =
            await _disciplinaRepository
                .GetEntityByIdAsync(request.DisciplinaId);

        if (disciplina == null)
            return null;

        if (request.SubDisciplinaId.HasValue)
        {
            var subDisciplina =
                await _subDisciplinaRepository
                    .GetEntityByIdAsync(request.SubDisciplinaId.Value);

            if (subDisciplina == null)
                return null;

            if (subDisciplina.DisciplinaId != request.DisciplinaId)
                return null;
        }

        if (!Enum.IsDefined(
                typeof(BimestreEnum),
                request.Bimestre))
            return null;

        if (request.Peso <= 0 ||
            request.Peso > 100)
            return null;

        var pesoAtual =
            await _avaliacaoRepository
                .GetPesoTotalByDisciplinaEBimestreAsync(
                    request.DisciplinaId,
                    request.Bimestre);

        if (pesoAtual + request.Peso > 100)
            return null;

        var avaliacao =
            new Avaliacao
            {
                DisciplinaId = request.DisciplinaId,
                SubDisciplinaId = request.SubDisciplinaId,
                Nome = request.Nome,
                Peso = request.Peso,
                DataAplicacao = request.DataAplicacao,
                Bimestre = (int)request.Bimestre,
                Ativo = true
            };

        await _avaliacaoRepository
            .AddAsync(avaliacao);

        return new AvaliacaoResponseDTO
        {
            Id = avaliacao.Id,
            DisciplinaId = avaliacao.DisciplinaId,
            SubDisciplinaId = avaliacao.SubDisciplinaId,
            Nome = avaliacao.Nome,
            Peso = avaliacao.Peso,
            DataAplicacao = avaliacao.DataAplicacao,
            Bimestre = (BimestreEnum)avaliacao.Bimestre,
            Ativo = avaliacao.Ativo
        };
    }
    public async Task<bool> UpdateAsync(
    int id,
    UpdateAvaliacaoRequest request)
    {
        var avaliacao =
            await _avaliacaoRepository
                .GetEntityByIdAsync(id);

        if (avaliacao == null)
            return false;

        if (!Enum.IsDefined(
                typeof(BimestreEnum),
                request.Bimestre))
            return false;

        if (request.Peso <= 0 ||
            request.Peso > 100)
            return false;

        var pesoAtual =
            await _avaliacaoRepository
                .GetPesoTotalByDisciplinaEBimestreAsync(
                    avaliacao.DisciplinaId,
                    request.Bimestre,
                    avaliacao.Id);

        if (pesoAtual + request.Peso > 100)
            return false;
        if (request.SubDisciplinaId.HasValue)
        {
            var subDisciplina =
                await _subDisciplinaRepository
                    .GetEntityByIdAsync(request.SubDisciplinaId.Value);

            if (subDisciplina == null)
                return false;

            if (subDisciplina.DisciplinaId != avaliacao.DisciplinaId)
                return false;
        }

        avaliacao.Nome =
            request.Nome;

        avaliacao.SubDisciplinaId =
            request.SubDisciplinaId;

        avaliacao.Peso =
            request.Peso;

        avaliacao.DataAplicacao =
            request.DataAplicacao;

        avaliacao.Bimestre =
            (int)request.Bimestre;

        await _avaliacaoRepository
            .UpdateAsync(avaliacao);

        return true;
    }
    public async Task<bool>
        DeleteAsync(int id)
    {
        var avaliacao =
            await _avaliacaoRepository
                .GetEntityByIdAsync(id);

        if (avaliacao == null)
            return false;

        avaliacao.Ativo = false;

        await _avaliacaoRepository
            .UpdateAsync(avaliacao);

        return true;
    }
}