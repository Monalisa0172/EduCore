using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class ProfessorSubDisciplinaServiceTests
{
    private readonly Mock<IProfessorSubDisciplinaRepository>
        _professorSubDisciplinaRepository;

    private readonly Mock<IProfessorRepository>
        _professorRepository;

    private readonly Mock<ISubDisciplinaRepository>
        _subDisciplinaRepository;

    private readonly ProfessorSubDisciplinaService
        _service;

    public ProfessorSubDisciplinaServiceTests()
    {
        _professorSubDisciplinaRepository =
            new Mock<IProfessorSubDisciplinaRepository>();

        _professorRepository =
            new Mock<IProfessorRepository>();

        _subDisciplinaRepository =
            new Mock<ISubDisciplinaRepository>();

        _service =
            new ProfessorSubDisciplinaService(
                _professorSubDisciplinaRepository.Object,
                _professorRepository.Object,
                _subDisciplinaRepository.Object);
    }

    [Fact]
    public async Task Deve_Criar_Vinculo_Com_Sucesso()
    {
        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Professor());

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubDisciplina());

        _professorSubDisciplinaRepository
            .Setup(x =>
                x.GetByProfessorAndSubDisciplinaAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
            .ReturnsAsync((ProfessorSubDisciplina?)null);

        _professorSubDisciplinaRepository
            .Setup(x =>
                x.AddAsync(It.IsAny<ProfessorSubDisciplina>()))
            .Returns(Task.CompletedTask);

        _professorSubDisciplinaRepository
            .Setup(x =>
                x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(
                new ProfessorSubDisciplinaResponseDTO());

        var request =
            new CreateProfessorSubDisciplinaRequest
            {
                ProfessorId = 1,
                SubDisciplinaId = 1
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().NotBeNull();

        _professorSubDisciplinaRepository.Verify(
            x => x.AddAsync(
                It.IsAny<ProfessorSubDisciplina>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Vinculo_Com_Professor_Inexistente()
    {
        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Professor?)null);

        var request =
            new CreateProfessorSubDisciplinaRequest
            {
                ProfessorId = 999,
                SubDisciplinaId = 1
            };

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage(
                "Professor não encontrado");
    }

    [Fact]
    public async Task Nao_Deve_Criar_Vinculo_Com_SubDisciplina_Inexistente()
    {
        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Professor());

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SubDisciplina?)null);

        var request =
            new CreateProfessorSubDisciplinaRequest
            {
                ProfessorId = 1,
                SubDisciplinaId = 999
            };

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage(
                "Subdisciplina não encontrada");
    }

    [Fact]
    public async Task Nao_Deve_Criar_Vinculo_Duplicado()
    {
        _professorRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Professor());

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new SubDisciplina());

        _professorSubDisciplinaRepository
            .Setup(x =>
                x.GetByProfessorAndSubDisciplinaAsync(
                    It.IsAny<int>(),
                    It.IsAny<int>()))
            .ReturnsAsync(
                new ProfessorSubDisciplina());

        var request =
            new CreateProfessorSubDisciplinaRequest
            {
                ProfessorId = 1,
                SubDisciplinaId = 1
            };

        Func<Task> act =
            async () =>
                await _service.CreateAsync(request);

        await act.Should()
            .ThrowAsync<Exception>()
            .WithMessage("Vínculo já existe");
    }

    [Fact]
    public async Task Deve_Inativar_Vinculo_Com_Sucesso()
    {
        var vinculo =
            new ProfessorSubDisciplina
            {
                Id = 1,
                Ativo = true
            };

        _professorSubDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(vinculo);

        _professorSubDisciplinaRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<ProfessorSubDisciplina>()))
            .Returns(Task.CompletedTask);

        var resultado =
            await _service.InativarAsync(1);

        resultado.Should().BeTrue();

        vinculo.Ativo.Should().BeFalse();

        _professorSubDisciplinaRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<ProfessorSubDisciplina>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_Vinculo_Inexistente()
    {
        _professorSubDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(
                (ProfessorSubDisciplina?)null);

        var resultado =
            await _service.InativarAsync(999);

        resultado.Should().BeFalse();

        _professorSubDisciplinaRepository.Verify(
            x => x.UpdateAsync(
                It.IsAny<ProfessorSubDisciplina>()),
            Times.Never);
    }
}