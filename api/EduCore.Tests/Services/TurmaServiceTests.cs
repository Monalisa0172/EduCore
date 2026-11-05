using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class TurmaServiceTests
{
    private readonly Mock<ITurmaRepository> _turmaRepository = new();

    private readonly TurmaService _service;

    public TurmaServiceTests()
    {
        _service = new TurmaService(_turmaRepository.Object);
    }

    private static CreateTurmaRequest Request(string nome = "1º Ano A", int ano = 2026) => new()
    {
        Nome = nome,
        AnoLetivo = ano
    };

    [Fact]
    public async Task Deve_Criar_Turma_Com_Sucesso()
    {
        _turmaRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new TurmaResponseDTO { Id = 1, Nome = "1º Ano A", AnoLetivo = 2026, Ativo = true });

        var resultado = await _service.CreateAsync(Request(nome: "  1º Ano A "));

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Nome.Should().Be("1º Ano A");
        resultado.Valor.AnoLetivo.Should().Be(2026);
        resultado.Valor.Ativo.Should().BeTrue();

        _turmaRepository.Verify(x => x.AddAsync(It.Is<Turma>(t => t.Nome == "1º Ano A" && t.Ativo)), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Turma_Duplicada_No_Mesmo_Ano()
    {
        _turmaRepository
            .Setup(x => x.GetByNomeEAnoAsync("1º Ano A", 2026))
            .ReturnsAsync(new Turma { Id = 9 });

        var resultado = await _service.CreateAsync(Request());

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        resultado.Campo.Should().Be(nameof(CreateTurmaRequest.Nome));
        _turmaRepository.Verify(x => x.AddAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Criar_Turma_Com_Mesmo_Nome_Em_Outro_Ano()
    {
        // So existe "1º Ano A" em 2025: criar em 2026 e permitido.
        _turmaRepository
            .Setup(x => x.GetByNomeEAnoAsync("1º Ano A", 2025))
            .ReturnsAsync(new Turma { Id = 9 });

        var resultado = await _service.CreateAsync(Request(ano: 2026));

        resultado.Sucesso.Should().BeTrue();
        _turmaRepository.Verify(x => x.AddAsync(It.IsAny<Turma>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Atualizar_Turma_Com_Sucesso()
    {
        var turma = new Turma { Id = 1, Nome = "1º Ano A", AnoLetivo = 2025, Ativo = true };
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(turma);

        var resultado = await _service.UpdateAsync(1, Request(nome: "1º Ano B", ano: 2026));

        resultado.Sucesso.Should().BeTrue();
        turma.Nome.Should().Be("1º Ano B");
        turma.AnoLetivo.Should().Be(2026);
        _turmaRepository.Verify(x => x.UpdateAsync(turma), Times.Once);
    }

    [Fact]
    public async Task Deve_Atualizar_Mantendo_O_Proprio_Nome()
    {
        var turma = new Turma { Id = 1, Nome = "1º Ano A", AnoLetivo = 2026, Ativo = true };
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(turma);
        _turmaRepository.Setup(x => x.GetByNomeEAnoAsync("1º Ano A", 2026)).ReturnsAsync(turma);

        var resultado = await _service.UpdateAsync(1, Request());

        resultado.Sucesso.Should().BeTrue();
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Com_Nome_De_Outra_Turma_Do_Mesmo_Ano()
    {
        var turma = new Turma { Id = 1, Nome = "1º Ano A", AnoLetivo = 2026, Ativo = true };
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(turma);
        _turmaRepository.Setup(x => x.GetByNomeEAnoAsync("1º Ano B", 2026)).ReturnsAsync(new Turma { Id = 2 });

        var resultado = await _service.UpdateAsync(1, Request(nome: "1º Ano B"));

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        turma.Nome.Should().Be("1º Ano A");
        _turmaRepository.Verify(x => x.UpdateAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Turma_Inexistente()
    {
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(It.IsAny<int>())).ReturnsAsync((Turma?)null);

        var resultado = await _service.UpdateAsync(999, Request());

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
    }

    [Fact]
    public async Task Deve_Inativar_Turma_Com_Sucesso()
    {
        var turma = new Turma { Id = 1, Nome = "1º Ano A", AnoLetivo = 2026, Ativo = true };
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(turma);

        var resultado = await _service.InativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        turma.Ativo.Should().BeFalse();
        _turmaRepository.Verify(x => x.UpdateAsync(It.IsAny<Turma>()), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_Turma_Inexistente()
    {
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(It.IsAny<int>())).ReturnsAsync((Turma?)null);

        var resultado = await _service.InativarAsync(999);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _turmaRepository.Verify(x => x.UpdateAsync(It.IsAny<Turma>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Reativar_Turma_Inativa()
    {
        var turma = new Turma { Id = 1, Nome = "1º Ano A", AnoLetivo = 2025, Ativo = false };
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(turma);

        var resultado = await _service.ReativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        turma.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Deve_Listar_Alunos_Da_Turma()
    {
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(new Turma { Id = 1 });
        _turmaRepository
            .Setup(x => x.GetAlunosAsync(1))
            .ReturnsAsync([new TurmaAlunoDTO { AlunoId = 3, UsuarioId = 10, Nome = "Ana", Ativo = true }]);

        var alunos = await _service.GetAlunosAsync(1);

        alunos.Should().ContainSingle(a => a.Nome == "Ana");
    }

    [Fact]
    public async Task Alunos_De_Turma_Inexistente_Retorna_Null()
    {
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(It.IsAny<int>())).ReturnsAsync((Turma?)null);

        var alunos = await _service.GetAlunosAsync(999);

        alunos.Should().BeNull();
        _turmaRepository.Verify(x => x.GetAlunosAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Buscar_Turma_Por_Id()
    {
        _turmaRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new TurmaResponseDTO { Id = 1, Nome = "1º Ano A", AnoLetivo = 2026, Ativo = true });

        var resultado = await _service.GetByIdAsync(1);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("1º Ano A");
    }

    [Fact]
    public async Task Deve_Retornar_Null_Quando_Turma_Nao_Existir()
    {
        _turmaRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((TurmaResponseDTO?)null);

        var resultado = await _service.GetByIdAsync(999);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Deve_Retornar_Todas_As_Turmas()
    {
        _turmaRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync([new TurmaResponseDTO { Id = 1, Nome = "1º Ano A" }]);

        var resultado = await _service.GetAllAsync();

        resultado.Should().HaveCount(1);
        resultado[0].Nome.Should().Be("1º Ano A");
    }
}
