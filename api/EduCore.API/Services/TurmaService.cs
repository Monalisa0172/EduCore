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

    /// <summary>
    /// Matricula vários alunos de uma vez. Quem já está na turma é ignorado (operação idempotente).
    /// </summary>
    public async Task<ResultadoOperacao<List<TurmaAlunoDTO>>> MatricularAsync(int turmaId, MatricularAlunosRequest request)
    {
        var turma = await _turmaRepository.GetEntityByIdAsync(turmaId);
        if (turma is null)
        {
            return ResultadoOperacao<List<TurmaAlunoDTO>>.NaoEncontrado(NaoEncontrada);
        }

        if (!turma.Ativo)
        {
            return ResultadoOperacao<List<TurmaAlunoDTO>>.Invalido(
                nameof(MatricularAlunosRequest.UsuarioIds),
                "A turma está inativa e não aceita novas matrículas. Reative a turma primeiro.");
        }

        var usuarioIds = request.UsuarioIds.Distinct().ToList();
        var usuarios = await _turmaRepository.GetAlunosAtivosParaMatriculaAsync(usuarioIds);

        if (usuarios.Count != usuarioIds.Count)
        {
            return ResultadoOperacao<List<TurmaAlunoDTO>>.Invalido(
                nameof(MatricularAlunosRequest.UsuarioIds),
                "Um ou mais alunos não existem ou estão inativos. Atualize a lista e selecione novamente.");
        }

        foreach (var usuario in usuarios)
        {
            // Alunos antigos podem nao ter o registro de perfil: e criado aqui, como no cadastro de usuario.
            var aluno = usuario.Alunos.FirstOrDefault();
            if (aluno is null)
            {
                aluno = new Aluno();
                usuario.Alunos.Add(aluno);
            }

            if (!aluno.Matriculas.Any(m => m.TurmaId == turmaId))
            {
                aluno.Matriculas.Add(new Matricula { TurmaId = turmaId });
            }
        }

        await _turmaRepository.SalvarAsync();

        return ResultadoOperacao<List<TurmaAlunoDTO>>.Ok(await _turmaRepository.GetAlunosAsync(turmaId));
    }

    /// <summary>
    /// Remove o aluno da turma. Turmas inativas não mudam: as matrículas delas são histórico.
    /// </summary>
    public async Task<ResultadoOperacao<List<TurmaAlunoDTO>>> RemoverMatriculaAsync(int turmaId, int usuarioId)
    {
        var turma = await _turmaRepository.GetEntityByIdAsync(turmaId);
        if (turma is null)
        {
            return ResultadoOperacao<List<TurmaAlunoDTO>>.NaoEncontrado(NaoEncontrada);
        }

        if (!turma.Ativo)
        {
            return ResultadoOperacao<List<TurmaAlunoDTO>>.Invalido(
                "TurmaId",
                "A turma está inativa: as matrículas dela ficam preservadas no histórico.");
        }

        var matricula = await _turmaRepository.GetMatriculaAsync(turmaId, usuarioId);
        if (matricula is null)
        {
            return ResultadoOperacao<List<TurmaAlunoDTO>>.NaoEncontrado("O aluno não está matriculado nesta turma.");
        }

        _turmaRepository.RemoverMatricula(matricula);
        await _turmaRepository.SalvarAsync();

        return ResultadoOperacao<List<TurmaAlunoDTO>>.Ok(await _turmaRepository.GetAlunosAsync(turmaId));
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
