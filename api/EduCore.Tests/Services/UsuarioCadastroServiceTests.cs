using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class UsuarioCadastroServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<ITurmaRepository> _turmaRepository = new();
    private readonly UsuarioCadastroService _service;

    public UsuarioCadastroServiceTests()
    {
        _service = new UsuarioCadastroService(
            _usuarioRepository.Object,
            _turmaRepository.Object);

        _turmaRepository
            .Setup(x => x.GetIdsAtivosAsync(It.IsAny<IReadOnlyCollection<int>>()))
            .ReturnsAsync((IReadOnlyCollection<int> ids) =>
                ids.Where(id => id != TurmaInativaId).ToList());
    }

    private const int TurmaInativaId = 5;

    [Fact]
    public async Task Deve_Criar_Aluno_Com_Matriculas_Em_Um_Unico_Add()
    {
        // Arrange

        Usuario? salvo = null;

        _usuarioRepository
            .Setup(x => x.AddAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u =>
            {
                u.Id = 30;
                salvo = u;
            })
            .Returns(Task.CompletedTask);

        _usuarioRepository
            .Setup(x => x.GetComPerfilAsync(30))
            .ReturnsAsync(() => ComTurmasCarregadas(salvo!));

        var request = new CreateUsuarioRequest
        {
            Tipo = TipoUsuarioEnum.Aluno,
            Nome = "  Maria Clara ",
            Email = " Maria@Escola.com ",
            Senha = "senhaForte1",
            TurmaIds = new List<int> { 1, 2, 2 }
        };

        // Act

        var resultado = await _service.CreateAsync(request);

        // Assert

        resultado.Falha.Should().BeNull();
        resultado.Usuario!.TurmaIds.Should().Equal(1, 2);

        salvo!.Nome.Should().Be("Maria Clara");
        salvo.Email.Should().Be("maria@escola.com");
        BCrypt.Net.BCrypt.Verify("senhaForte1", salvo.Senha).Should().BeTrue();
        salvo.Alunos.Single().Matriculas.Select(m => m.TurmaId).Should().Equal(1, 2);

        _usuarioRepository.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Criar_Professor_Com_Funcionario_E_Professor()
    {
        // Arrange

        Usuario? salvo = null;

        _usuarioRepository
            .Setup(x => x.AddAsync(It.IsAny<Usuario>()))
            .Callback<Usuario>(u => salvo = u)
            .Returns(Task.CompletedTask);

        _usuarioRepository
            .Setup(x => x.GetComPerfilAsync(It.IsAny<int>()))
            .ReturnsAsync(() => salvo);

        var request = new CreateUsuarioRequest
        {
            Tipo = TipoUsuarioEnum.Professor,
            Nome = "João",
            Email = "joao@escola.com",
            Senha = "senhaForte1",
            Professor = DadosProfessor()
        };

        // Act

        var resultado = await _service.CreateAsync(request);

        // Assert

        resultado.Falha.Should().BeNull();

        var funcionario = salvo!.Funcionarios.Single();
        funcionario.Cargo.Should().Be("Professor");
        funcionario.TipoContrato.Should().Be((int)TipoContratoEnum.Pj);
        funcionario.Status.Should().Be(StatusFuncionarioEnum.Ativo);
        funcionario.Ativo.Should().BeTrue();
        funcionario.Professor!.Especialidade.Should().Be("Matemática");

        resultado.Usuario!.Professor!.TipoContrato.Should().Be(TipoContratoEnum.Pj);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Com_Email_Em_Uso()
    {
        // Arrange

        _usuarioRepository
            .Setup(x => x.EmailEmUsoAsync("ana@escola.com", null))
            .ReturnsAsync(true);

        var request = new CreateUsuarioRequest
        {
            Tipo = TipoUsuarioEnum.Aluno,
            Nome = "Ana",
            Email = "ANA@escola.com",
            Senha = "senhaForte1"
        };

        // Act

        var resultado = await _service.CreateAsync(request);

        // Assert

        resultado.Falha.Should().Be(FalhaCadastro.EmailEmUso);

        _usuarioRepository.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Matricular_Em_Turma_Inativa()
    {
        // Arrange

        var request = new CreateUsuarioRequest
        {
            Tipo = TipoUsuarioEnum.Aluno,
            Nome = "Ana",
            Email = "ana@escola.com",
            Senha = "senhaForte1",
            TurmaIds = new List<int> { 1, TurmaInativaId }
        };

        // Act

        var resultado = await _service.CreateAsync(request);

        // Assert

        resultado.Falha.Should().Be(FalhaCadastro.Invalido);
        resultado.Campo.Should().Be(nameof(CreateUsuarioRequest.TurmaIds));

        _usuarioRepository.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Theory]
    [InlineData(TipoUsuarioEnum.Professor)]
    [InlineData(TipoUsuarioEnum.Admin)]
    public async Task Deve_Rejeitar_Professor_Sem_Dados_E_Tipo_Nao_Suportado(TipoUsuarioEnum tipo)
    {
        // Arrange

        var request = new CreateUsuarioRequest
        {
            Tipo = tipo,
            Nome = "Teste",
            Email = "teste@escola.com",
            Senha = "senhaForte1"
        };

        // Act

        var resultado = await _service.CreateAsync(request);

        // Assert

        resultado.Falha.Should().Be(FalhaCadastro.Invalido);

        _usuarioRepository.Verify(x => x.AddAsync(It.IsAny<Usuario>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Sincronizar_So_Matriculas_De_Turmas_Ativas()
    {
        // Arrange

        var turma1 = new Turma { Id = 1, Ativo = true };
        var turma2 = new Turma { Id = 2, Ativo = true };
        var turmaInativa = new Turma { Id = TurmaInativaId, Ativo = false };

        var naTurma1 = new Matricula { Id = 10, TurmaId = 1, Turma = turma1 };
        var naTurma2 = new Matricula { Id = 11, TurmaId = 2, Turma = turma2 };
        var historico = new Matricula { Id = 12, TurmaId = TurmaInativaId, Turma = turmaInativa };

        var usuario = Aluno(7, naTurma1, naTurma2, historico);
        var senhaOriginal = usuario.Senha;

        IEnumerable<Matricula>? removidas = null;

        _usuarioRepository.Setup(x => x.GetComPerfilAsync(7)).ReturnsAsync(usuario);
        _usuarioRepository
            .Setup(x => x.RemoverMatriculas(It.IsAny<IEnumerable<Matricula>>()))
            .Callback<IEnumerable<Matricula>>(m => removidas = m.ToList());

        var request = new UpdateUsuarioRequest
        {
            Nome = "Aluno Editado",
            Email = "aluno@escola.com",
            NovaSenha = "   ",
            TurmaIds = new List<int> { 2, 3 }
        };

        // Act

        var resultado = await _service.UpdateAsync(7, request);

        // Assert

        resultado.Falha.Should().BeNull();

        removidas.Should().ContainSingle().Which.Should().BeSameAs(naTurma1);

        usuario.Alunos.Single().Matriculas
            .Should().Contain(historico)
            .And.Contain(m => m.TurmaId == 3 && m.Id == 0);

        usuario.Senha.Should().Be(senhaOriginal);

        _usuarioRepository.Verify(x => x.SalvarAlteracoesAsync(), Times.Once);
    }

    [Fact]
    public async Task Deve_Criar_Funcionario_Ao_Editar_Professor_Legado()
    {
        // Arrange

        var usuario = new Usuario
        {
            Id = 19,
            Nome = "André Luiz",
            Email = "andre@escola.com",
            Senha = "hash",
            Tipo = TipoUsuarioEnum.Professor,
            Ativo = true
        };

        _usuarioRepository.Setup(x => x.GetComPerfilAsync(19)).ReturnsAsync(usuario);

        var request = new UpdateUsuarioRequest
        {
            Nome = "André Luiz",
            Email = "andre@escola.com",
            NovaSenha = "novaSenha123",
            Professor = DadosProfessor()
        };

        // Act

        var resultado = await _service.UpdateAsync(19, request);

        // Assert

        resultado.Falha.Should().BeNull();

        usuario.Funcionarios.Single().Professor!.Especialidade.Should().Be("Matemática");
        BCrypt.Net.BCrypt.Verify("novaSenha123", usuario.Senha).Should().BeTrue();
    }

    [Fact]
    public async Task Deve_Retornar_Nao_Encontrado_Ao_Editar_Usuario_Inexistente()
    {
        // Arrange

        _usuarioRepository
            .Setup(x => x.GetComPerfilAsync(It.IsAny<int>()))
            .ReturnsAsync((Usuario?)null);

        // Act

        var resultado = await _service.UpdateAsync(999, new UpdateUsuarioRequest());

        // Assert

        resultado.Falha.Should().Be(FalhaCadastro.NaoEncontrado);

        _usuarioRepository.Verify(x => x.SalvarAlteracoesAsync(), Times.Never);
    }

    private static DadosProfessorRequest DadosProfessor() => new()
    {
        Especialidade = " Matemática ",
        Cargo = "Professor",
        DataAdmissao = new DateOnly(2026, 2, 1),
        TipoContrato = TipoContratoEnum.Pj
    };

    private static Usuario Aluno(int id, params Matricula[] matriculas)
    {
        var usuario = new Usuario
        {
            Id = id,
            Nome = "Aluno",
            Email = "aluno@escola.com",
            Senha = "hash-original",
            Tipo = TipoUsuarioEnum.Aluno,
            Ativo = true
        };

        usuario.Alunos.Add(new Aluno { Matriculas = matriculas.ToList() });

        return usuario;
    }

    /// <summary>
    /// Simula o Include do repositório: matrículas recém-criadas passam a ter a turma carregada.
    /// </summary>
    private static Usuario ComTurmasCarregadas(Usuario usuario)
    {
        foreach (var matricula in usuario.Alunos.SelectMany(a => a.Matriculas))
            matricula.Turma ??= new Turma { Id = matricula.TurmaId!.Value, Ativo = true };

        return usuario;
    }
}
