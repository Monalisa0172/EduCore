using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace EduCore.Tests.Services;

public class ProfessorTurmaServiceTests
{
    private readonly Mock<IProfessorTurmaRepository>
        _professorTurmaRepository;

    private readonly Mock<IProfessorRepository>
        _professorRepository;

    private readonly Mock<ITurmaRepository>
        _turmaRepository;

    private readonly ProfessorTurmaService
        _service;

    public ProfessorTurmaServiceTests()
    {
        _professorTurmaRepository =
            new Mock<IProfessorTurmaRepository>();

        _professorRepository =
            new Mock<IProfessorRepository>();

        _turmaRepository =
            new Mock<ITurmaRepository>();

        _service =
            new ProfessorTurmaService(
                _professorTurmaRepository.Object,
                _professorRepository.Object,
                _turmaRepository.Object);
    }

    [Fact]
    public void Deve_Instanciar_Service()
    {
        _service.Should().NotBeNull();
    }

    [Fact]
    public async Task Deve_Criar_Vinculo_Com_Sucesso()
    {
        // Arrange

        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(new Professor
            {
                Id = 1
            });

        _turmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(new Turma
            {
                Id = 1
            });

        _professorTurmaRepository
            .Setup(x =>
                x.GetByProfessorAndTurmaAsync(
                    1,
                    1))
            .ReturnsAsync((ProfessorTurma?)null);

        _professorTurmaRepository
            .Setup(x =>
                x.AddAsync(It.IsAny<ProfessorTurma>()))
            .Returns(Task.CompletedTask);

        _professorTurmaRepository
            .Setup(x =>
                x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new ProfessorTurmaResponseDTO
            {
                Id = 1,
                ProfessorId = 1,
                TurmaId = 1,
                Ativo = true
            });

        var request =
            new CreateProfessorTurmaRequest
            {
                ProfessorId = 1,
                TurmaId = 1
            };

        // Act

        var resultado =
            await _service.CreateAsync(request);

        // Assert

        resultado.Should().NotBeNull();

        resultado!.ProfessorId.Should()
            .Be(1);

        resultado.TurmaId.Should()
            .Be(1);

        resultado.Ativo.Should()
            .BeTrue();

        _professorTurmaRepository.Verify(
            x => x.AddAsync(It.IsAny<ProfessorTurma>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Vinculo_Quando_Professor_Nao_Existir()
    {
        // Arrange

        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Professor?)null);

        var request =
            new CreateProfessorTurmaRequest
            {
                ProfessorId = 999,
                TurmaId = 1
            };

        // Act

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Professor não encontrado");
    }

    [Fact]
    public async Task Nao_Deve_Criar_Vinculo_Quando_Turma_Nao_Existir()
    {
        // Arrange

        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Professor());

        _turmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Turma?)null);

        var request =
            new CreateProfessorTurmaRequest
            {
                ProfessorId = 1,
                TurmaId = 999
            };

        // Act

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Turma não encontrada");
    }

    [Fact]
    public async Task Nao_Deve_Criar_Vinculo_Duplicado()
    {
        // Arrange

        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Professor());

        _turmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Turma());

        _professorTurmaRepository
            .Setup(x =>
                x.GetByProfessorAndTurmaAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
            .ReturnsAsync(new ProfessorTurma());

        var request =
            new CreateProfessorTurmaRequest
            {
                ProfessorId = 1,
                TurmaId = 1
            };

        // Act

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        // Assert

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Vínculo já existe");
    }

    [Fact]
    public async Task Deve_Inativar_Vinculo_Com_Sucesso()
    {
        // Arrange

        var vinculo =
            new ProfessorTurma
            {
                Id = 1,
                ProfessorId = 1,
                TurmaId = 1,
                Ativo = true
            };

        _professorTurmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(vinculo);

        _professorTurmaRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<ProfessorTurma>()))
            .Returns(Task.CompletedTask);

        // Act

        var resultado =
            await _service.InativarAsync(1);

        // Assert

        resultado.Should().BeTrue();

        vinculo.Ativo.Should().BeFalse();

        _professorTurmaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<ProfessorTurma>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_Vinculo_Inexistente()
    {
        // Arrange

        _professorTurmaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ProfessorTurma?)null);

        // Act

        var resultado =
            await _service.InativarAsync(999);

        // Assert

        resultado.Should().BeFalse();

        _professorTurmaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<ProfessorTurma>()),
            Times.Never);
    }
}