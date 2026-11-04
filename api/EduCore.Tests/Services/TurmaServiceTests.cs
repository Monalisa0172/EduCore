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
    private readonly Mock<ITurmaRepository>
        _turmaRepository;

    private readonly TurmaService
        _service;

    public TurmaServiceTests()
    {
        _turmaRepository =
            new Mock<ITurmaRepository>();

        _service =
            new TurmaService(
                _turmaRepository.Object);
    }

    [Fact]
    public async Task Deve_Criar_Turma_Com_Sucesso()
    {
        // Arrange

        _turmaRepository
            .Setup(x =>
                x.GetByNomeAsync(It.IsAny<string>()))
            .ReturnsAsync((Turma?)null);

        _turmaRepository
            .Setup(x =>
                x.AddAsync(It.IsAny<Turma>()))
            .Returns(Task.CompletedTask);

        _turmaRepository
            .Setup(x =>
                x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new TurmaResponseDTO
            {
                Id = 1,
                Nome = "1º Ano A",
                AnoLetivo = 2026,
                Ativo = true
            });

        var request =
            new CreateTurmaRequest
            {
                Nome = "1º Ano A",
                AnoLetivo = 2026
            };

        // Act

        var resultado =
            await _service.CreateAsync(request);

        // Assert

        resultado.Should().NotBeNull();

        resultado!.Nome.Should()
            .Be("1º Ano A");

        resultado.AnoLetivo.Should()
            .Be(2026);

        resultado.Ativo.Should()
            .BeTrue();

        _turmaRepository.Verify(
            x => x.AddAsync(It.IsAny<Turma>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Turma_Duplicada()
    {
        // Arrange

        _turmaRepository
            .Setup(x =>
                x.GetByNomeAsync(It.IsAny<string>()))
            .ReturnsAsync(new Turma());

        var request =
            new CreateTurmaRequest
            {
                Nome = "1º Ano A",
                AnoLetivo = 2026
            };

        // Act

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Turma já cadastrada");
    }

    [Fact]
    public async Task Deve_Inativar_Turma_Com_Sucesso()
    {
        // Arrange

        var turma = new Turma
        {
            Id = 1,
            Nome = "1º Ano A",
            AnoLetivo = 2026,
            Ativo = true
        };

        _turmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(turma);

        _turmaRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<Turma>()))
            .Returns(Task.CompletedTask);

        // Act

        var resultado =
            await _service.InativarAsync(1);

        // Assert

        resultado.Should().BeTrue();

        turma.Ativo.Should().BeFalse();

        _turmaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Turma>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_Turma_Inexistente()
    {
        // Arrange

        _turmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Turma?)null);

        // Act

        var resultado =
            await _service.InativarAsync(999);

        // Assert

        resultado.Should().BeFalse();

        _turmaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Turma>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Buscar_Turma_Por_Id()
    {
        // Arrange

        _turmaRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new TurmaResponseDTO
            {
                Id = 1,
                Nome = "1º Ano A",
                AnoLetivo = 2026,
                Ativo = true
            });

        // Act

        var resultado =
            await _service.GetByIdAsync(1);

        // Assert

        resultado.Should().NotBeNull();

        resultado!.Id.Should().Be(1);

        resultado.Nome.Should().Be("1º Ano A");
    }

    [Fact]
    public async Task Deve_Retornar_Null_Quando_Turma_Nao_Existir()
    {
        // Arrange

        _turmaRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((TurmaResponseDTO?)null);

        // Act

        var resultado =
            await _service.GetByIdAsync(999);

        // Assert

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Deve_Retornar_Todas_As_Turmas()
    {
        var turmas =
            new List<TurmaResponseDTO>
            {
            new()
            {
                Id = 1,
                Nome = "1º Ano A"
            }
            };

        _turmaRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(turmas);

        var resultado =
            await _service.GetAllAsync();

        resultado.Should().HaveCount(1);

        resultado[0].Nome.Should()
            .Be("1º Ano A");
    }
}