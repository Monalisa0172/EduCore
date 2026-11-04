using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class SubDisciplinaServiceTests
{
    private readonly Mock<ISubDisciplinaRepository>
        _subDisciplinaRepository;

    private readonly Mock<IDisciplinaRepository>
        _disciplinaRepository;

    private readonly SubDisciplinaService
        _service;

    public SubDisciplinaServiceTests()
    {
        _subDisciplinaRepository =
            new Mock<ISubDisciplinaRepository>();

        _disciplinaRepository =
            new Mock<IDisciplinaRepository>();

        _service =
            new SubDisciplinaService(
                _subDisciplinaRepository.Object,
                _disciplinaRepository.Object);
    }

    [Fact]
    public async Task Deve_Criar_SubDisciplina_Com_Sucesso()
    {

        var disciplina = new Disciplina
        {
            Id = 1,
            Nome = "Matemática"
        };

        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(disciplina);

        _subDisciplinaRepository
            .Setup(x =>
                x.GetByNomeAndDisciplinaAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
            .ReturnsAsync((SubDisciplina?)null);

        _subDisciplinaRepository
            .Setup(x =>
                x.AddAsync(It.IsAny<SubDisciplina>()))
            .Returns(Task.CompletedTask);

        var request =
            new CreateSubDisciplinaRequest
            {
                Nome = "Álgebra",
                Descricao = "Fundamentos",
                DisciplinaId = 1
            };

        // Act

        var resultado =
            await _service.CreateAsync(request);

        // Assert

        resultado.Should().NotBeNull();

        resultado!.Nome.Should()
            .Be("Álgebra");

        resultado.Disciplina.Should()
            .Be("Matemática");

        resultado.Ativo.Should()
            .BeTrue();

        _subDisciplinaRepository.Verify(
            x => x.AddAsync(It.IsAny<SubDisciplina>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_SubDisciplina_Sem_Disciplina()
    {

        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Disciplina?)null);

        var request =
            new CreateSubDisciplinaRequest
            {
                Nome = "Álgebra",
                DisciplinaId = 1
            };

        // Act

        var resultado =
            await _service.CreateAsync(request);

        // Assert

        resultado.Should().BeNull();

        _subDisciplinaRepository.Verify(
            x => x.AddAsync(It.IsAny<SubDisciplina>()),
            Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Criar_SubDisciplina_Duplicada()
    {
        // Arrange

        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Disciplina());

        _subDisciplinaRepository
            .Setup(x =>
                x.GetByNomeAndDisciplinaAsync(
                    It.IsAny<string>(),
                    It.IsAny<int>()))
            .ReturnsAsync(new SubDisciplina());

        var request =
            new CreateSubDisciplinaRequest
            {
                Nome = "Álgebra",
                DisciplinaId = 1
            };

        // Act

        var resultado =
            await _service.CreateAsync(request);

        // Assert

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Deve_Atualizar_SubDisciplina_Com_Sucesso()
    {
        var subDisciplina =
            new SubDisciplina
            {
                Id = 1,
                Nome = "Álgebra",
                DisciplinaId = 1,
                Ativo = true
            };

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(subDisciplina);

        _subDisciplinaRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<SubDisciplina>()))
            .Returns(Task.CompletedTask);

        var request =
            new CreateSubDisciplinaRequest
            {
                Nome = "Geometria",
                Descricao = "Nova descrição",
                DisciplinaId = 2
            };

        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeTrue();

        subDisciplina.Nome.Should()
            .Be("Geometria");

        subDisciplina.Descricao.Should()
            .Be("Nova descrição");

        subDisciplina.DisciplinaId.Should()
            .Be(2);

        _subDisciplinaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<SubDisciplina>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_SubDisciplina_Inexistente()
    {
        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SubDisciplina?)null);

        var request =
            new CreateSubDisciplinaRequest
            {
                Nome = "Geometria"
            };

        var resultado =
            await _service.UpdateAsync(
                999,
                request);

        resultado.Should().BeFalse();

        _subDisciplinaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<SubDisciplina>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Inativar_SubDisciplina_Com_Sucesso()
    {

        var subDisciplina =
            new SubDisciplina
            {
                Id = 1,
                Nome = "Álgebra",
                Ativo = true
            };

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(subDisciplina);

        _subDisciplinaRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<SubDisciplina>()))
            .Returns(Task.CompletedTask);

        var resultado =
            await _service.DeleteAsync(1);

        resultado.Should().BeTrue();

        subDisciplina.Ativo.Should()
            .BeFalse();

        _subDisciplinaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<SubDisciplina>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_SubDisciplina_Inexistente()
    {

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SubDisciplina?)null);


        var resultado =
            await _service.DeleteAsync(999);

        resultado.Should().BeFalse();

        _subDisciplinaRepository.Verify(
            x => x.UpdateAsync(It.IsAny<SubDisciplina>()),
            Times.Never);
    }
}