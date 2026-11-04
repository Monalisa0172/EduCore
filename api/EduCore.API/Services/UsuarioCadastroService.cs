namespace EduCore.API.Services;

using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;

/// <summary>
/// Cadastro e edição de alunos e professores (usuário + perfil + matrículas).
/// </summary>
public class UsuarioCadastroService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITurmaRepository _turmaRepository;

    public UsuarioCadastroService(
        IUsuarioRepository usuarioRepository,
        ITurmaRepository turmaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _turmaRepository = turmaRepository;
    }

    public async Task<UsuarioDetalheDTO?> GetDetalheAsync(int id)
    {
        var usuario = await _usuarioRepository.GetComPerfilAsync(id);

        return usuario is null || !UsuarioService.TipoListavel(usuario.Tipo)
            ? null
            : ParaDetalhe(usuario);
    }

    public async Task<UsuarioCadastroResultado> CreateAsync(CreateUsuarioRequest request)
    {
        var tipo = request.Tipo!.Value;

        if (!UsuarioService.TipoListavel(tipo))
            return UsuarioCadastroResultado.Invalido(nameof(request.Tipo), "Informe Aluno ou Professor.");

        var turmaIds = request.TurmaIds.Distinct().ToList();
        var invalido = await ValidarPerfilAsync(tipo, request, turmaIds);
        if (invalido is not null)
            return invalido;

        var email = NormalizarEmail(request.Email);
        if (await _usuarioRepository.EmailEmUsoAsync(email))
            return UsuarioCadastroResultado.EmailEmUso();

        var usuario = new Usuario
        {
            Nome = request.Nome.Trim(),
            Email = email,
            Senha = BCrypt.Net.BCrypt.HashPassword(request.Senha),
            Tipo = tipo,
            Ativo = true
        };

        if (tipo == TipoUsuarioEnum.Aluno)
        {
            usuario.Alunos.Add(new Aluno
            {
                Matriculas = turmaIds.Select(id => new Matricula { TurmaId = id }).ToList()
            });
        }
        else
        {
            var funcionario = new Funcionario
            {
                Status = StatusFuncionarioEnum.Ativo,
                Ativo = true
            };

            AplicarDadosProfessor(funcionario, request.Professor!);
            usuario.Funcionarios.Add(funcionario);
        }

        // O grafo inteiro (usuario + perfil + matriculas) vai num unico SaveChanges: tudo ou nada.
        await _usuarioRepository.AddAsync(usuario);

        return UsuarioCadastroResultado.Sucesso((await GetDetalheAsync(usuario.Id))!);
    }

    public async Task<UsuarioCadastroResultado> UpdateAsync(int id, UpdateUsuarioRequest request)
    {
        var usuario = await _usuarioRepository.GetComPerfilAsync(id);

        if (usuario is null || !UsuarioService.TipoListavel(usuario.Tipo))
            return UsuarioCadastroResultado.NaoEncontrado();

        var turmaIds = request.TurmaIds.Distinct().ToList();
        var invalido = await ValidarPerfilAsync(usuario.Tipo, request, turmaIds);
        if (invalido is not null)
            return invalido;

        var email = NormalizarEmail(request.Email);
        if (await _usuarioRepository.EmailEmUsoAsync(email, id))
            return UsuarioCadastroResultado.EmailEmUso();

        usuario.Nome = request.Nome.Trim();
        usuario.Email = email;

        if (!string.IsNullOrWhiteSpace(request.NovaSenha))
            usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

        if (usuario.Tipo == TipoUsuarioEnum.Aluno)
            AtualizarMatriculas(usuario, turmaIds);
        else
            AplicarDadosProfessor(ObterOuCriarFuncionario(usuario), request.Professor!);

        await _usuarioRepository.SalvarAlteracoesAsync();

        // Recarrega para trazer as turmas das matriculas novas (navegacao ainda nao carregada).
        return UsuarioCadastroResultado.Sucesso((await GetDetalheAsync(id))!);
    }

    private async Task<UsuarioCadastroResultado?> ValidarPerfilAsync(
        TipoUsuarioEnum tipo,
        SalvarUsuarioRequest request,
        IReadOnlyCollection<int> turmaIds)
    {
        if (tipo == TipoUsuarioEnum.Professor)
        {
            return request.Professor is null
                ? UsuarioCadastroResultado.Invalido(nameof(request.Professor), "Informe os dados do professor.")
                : null;
        }

        if (turmaIds.Count == 0)
            return null;

        var ativas = await _turmaRepository.GetIdsAtivosAsync(turmaIds);

        return ativas.Count == turmaIds.Count
            ? null
            : UsuarioCadastroResultado.Invalido(
                nameof(request.TurmaIds),
                "Uma ou mais turmas não existem ou estão inativas. Atualize a página e selecione novamente.");
    }

    /// <summary>
    /// Sincroniza só as matrículas em turmas ativas; as de turmas inativas ficam como histórico.
    /// </summary>
    private void AtualizarMatriculas(Usuario usuario, IReadOnlyCollection<int> turmaIds)
    {
        var aluno = usuario.Alunos.FirstOrDefault();
        if (aluno is null)
        {
            aluno = new Aluno();
            usuario.Alunos.Add(aluno);
        }

        var removidas = aluno.Matriculas
            .Where(m => m.Turma is { Ativo: true } && !turmaIds.Contains(m.TurmaId!.Value))
            .ToList();

        _usuarioRepository.RemoverMatriculas(removidas);

        var atuais = aluno.Matriculas
            .Except(removidas)
            .Select(m => m.TurmaId)
            .ToHashSet();

        foreach (var turmaId in turmaIds.Where(id => !atuais.Contains(id)))
            aluno.Matriculas.Add(new Matricula { TurmaId = turmaId });
    }

    /// <summary>
    /// Alguns professores antigos não têm registro de funcionário: ele é criado na primeira edição.
    /// </summary>
    private static Funcionario ObterOuCriarFuncionario(Usuario usuario)
    {
        var funcionario = ObterFuncionario(usuario);
        if (funcionario is not null)
            return funcionario;

        funcionario = new Funcionario
        {
            Status = StatusFuncionarioEnum.Ativo,
            Ativo = usuario.Ativo
        };

        usuario.Funcionarios.Add(funcionario);

        return funcionario;
    }

    private static void AplicarDadosProfessor(Funcionario funcionario, DadosProfessorRequest dados)
    {
        funcionario.Cargo = dados.Cargo.Trim();
        funcionario.DataAdmissao = dados.DataAdmissao!.Value;
        funcionario.TipoContrato = (int)dados.TipoContrato!.Value;

        funcionario.Professor ??= new Professor();
        funcionario.Professor.Especialidade = dados.Especialidade.Trim();
    }

    private static Funcionario? ObterFuncionario(Usuario usuario)
        => usuario.Funcionarios.FirstOrDefault(f => f.Professor is not null)
            ?? usuario.Funcionarios.FirstOrDefault();

    private static string NormalizarEmail(string email)
        => email.Trim().ToLowerInvariant();

    private static UsuarioDetalheDTO ParaDetalhe(Usuario usuario)
    {
        var turmaIds = usuario.Alunos
            .SelectMany(a => a.Matriculas)
            .Where(m => m.Turma is { Ativo: true })
            .Select(m => m.TurmaId!.Value)
            .Distinct()
            .Order()
            .ToList();

        var funcionario = usuario.Tipo == TipoUsuarioEnum.Professor
            ? ObterFuncionario(usuario)
            : null;

        var professor = funcionario is null
            ? null
            : new DadosProfessorDTO(
                funcionario.Professor?.Especialidade ?? string.Empty,
                funcionario.Cargo ?? string.Empty,
                funcionario.DataAdmissao,
                funcionario.TipoContrato is int contrato && Enum.IsDefined(typeof(TipoContratoEnum), contrato)
                    ? (TipoContratoEnum)contrato
                    : null);

        return new UsuarioDetalheDTO(
            usuario.Id,
            usuario.Nome ?? string.Empty,
            usuario.Email ?? string.Empty,
            usuario.Tipo.ToString(),
            usuario.Ativo,
            turmaIds,
            professor);
    }
}
