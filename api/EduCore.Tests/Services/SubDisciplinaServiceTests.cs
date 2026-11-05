using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class SubDisciplinaServiceTests
{
    private readonly Mock<ISubDisciplinaRepository> _subDisciplinaRepository = new();

    private readonly Mock<IDisciplinaRepository> _disciplinaRepository = new();

    private readonly SubDisciplinaService _service;

    public SubDisciplinaServiceTests()
    {
        _service = new SubDisciplinaService(
            _subDisciplinaRepository.Object,
            _disciplinaRepository.Object);
    }

    private void DisciplinaExistente(int id, bool ativa = true, string nome = "Matemática")
        => _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(id))
            .ReturnsAsync(new Disciplina { Id = id, Nome = nome, Ativo = ativa });

    [Fact]
    public async Task Deve_Criar_SubDisciplina_Com_Sucesso()
    {
        DisciplinaExistente(1);

        var request = new CreateSubDisciplinaRequest
        {
            Nome = "  Álgebra ",
            Descricao = "Fundamentos",
            DisciplinaId = 1
        };

        var resultado = await _service.CreateAsync(request);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Nome.Should().Be("Álgebra");
        resultado.Valor.Disciplina.Should().Be("Matemática");
        resultado.Valor.Ativo.Should().BeTrue();

        _subDisciplinaRepository.Verify(x => x.AddAsync(It.IsAny<SubDisciplina>()), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_SubDisciplina_Sem_Disciplina()
    {
        _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Disciplina?)null);

        var resultado = await _service.CreateAsync(new CreateSubDisciplinaRequest { Nome = "Álgebra", DisciplinaId = 1 });

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Campo.Should().Be(nameof(CreateSubDisciplinaRequest.DisciplinaId));
        _subDisciplinaRepository.Verify(x => x.AddAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Criar_SubDisciplina_Em_Disciplina_Inativa()
    {
        DisciplinaExistente(1, ativa: false);

        var resultado = await _service.CreateAsync(new CreateSubDisciplinaRequest { Nome = "Álgebra", DisciplinaId = 1 });

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        _subDisciplinaRepository.Verify(x => x.AddAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Criar_SubDisciplina_Duplicada()
    {
        DisciplinaExistente(1);

        _subDisciplinaRepository
            .Setup(x => x.GetByNomeAndDisciplinaAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new SubDisciplina { Id = 5 });

        var resultado = await _service.CreateAsync(new CreateSubDisciplinaRequest { Nome = "Álgebra", DisciplinaId = 1 });

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        resultado.Campo.Should().Be(nameof(CreateSubDisciplinaRequest.Nome));
        _subDisciplinaRepository.Verify(x => x.AddAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Atualizar_SubDisciplina_Com_Sucesso()
    {
        var subDisciplina = new SubDisciplina { Id = 1, Nome = "Álgebra", DisciplinaId = 1, Ativo = true };

        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(subDisciplina);
        DisciplinaExistente(2);

        var request = new CreateSubDisciplinaRequest
        {
            Nome = "Geometria",
            Descricao = "Nova descrição",
            DisciplinaId = 2
        };

        var resultado = await _service.UpdateAsync(1, request);

        resultado.Sucesso.Should().BeTrue();
        subDisciplina.Nome.Should().Be("Geometria");
        subDisciplina.Descricao.Should().Be("Nova descrição");
        subDisciplina.DisciplinaId.Should().Be(2);

        _subDisciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<SubDisciplina>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Atualizar_Mantendo_O_Proprio_Nome()
    {
        var subDisciplina = new SubDisciplina { Id = 1, Nome = "Álgebra", DisciplinaId = 1, Ativo = true };

        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(subDisciplina);
        _subDisciplinaRepository.Setup(x => x.GetByNomeAndDisciplinaAsync("Álgebra", 1)).ReturnsAsync(subDisciplina);

        var resultado = await _service.UpdateAsync(1, new CreateSubDisciplinaRequest
        {
            Nome = "Álgebra",
            Descricao = "Só a descrição mudou",
            DisciplinaId = 1
        });

        resultado.Sucesso.Should().BeTrue();
        subDisciplina.Descricao.Should().Be("Só a descrição mudou");
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Com_Nome_De_Outra_SubDisciplina()
    {
        var subDisciplina = new SubDisciplina { Id = 1, Nome = "Álgebra", DisciplinaId = 1, Ativo = true };

        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(subDisciplina);
        _subDisciplinaRepository
            .Setup(x => x.GetByNomeAndDisciplinaAsync("Geometria", 1))
            .ReturnsAsync(new SubDisciplina { Id = 2, Nome = "Geometria", DisciplinaId = 1 });

        var resultado = await _service.UpdateAsync(1, new CreateSubDisciplinaRequest { Nome = "Geometria", DisciplinaId = 1 });

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        subDisciplina.Nome.Should().Be("Álgebra");
        _subDisciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_SubDisciplina_Inexistente()
    {
        _subDisciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SubDisciplina?)null);

        var resultado = await _service.UpdateAsync(999, new CreateSubDisciplinaRequest { Nome = "Geometria" });

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _subDisciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Inativar_SubDisciplina_Com_Sucesso()
    {
        var subDisciplina = new SubDisciplina { Id = 1, Nome = "Álgebra", Ativo = true };

        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(subDisciplina);

        var resultado = await _service.InativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        subDisciplina.Ativo.Should().BeFalse();
        _subDisciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<SubDisciplina>()), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_SubDisciplina_Inexistente()
    {
        _subDisciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((SubDisciplina?)null);

        var resultado = await _service.InativarAsync(999);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _subDisciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Reativar_SubDisciplina_De_Disciplina_Ativa()
    {
        var subDisciplina = new SubDisciplina { Id = 1, Nome = "Álgebra", DisciplinaId = 1, Ativo = false };

        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(subDisciplina);
        DisciplinaExistente(1);

        var resultado = await _service.ReativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        subDisciplina.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Nao_Deve_Reativar_SubDisciplina_De_Disciplina_Inativa()
    {
        var subDisciplina = new SubDisciplina { Id = 1, Nome = "Álgebra", DisciplinaId = 1, Ativo = false };

        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(subDisciplina);
        DisciplinaExistente(1, ativa: false);

        var resultado = await _service.ReativarAsync(1);

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        subDisciplina.Ativo.Should().BeFalse();
        _subDisciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<SubDisciplina>()), Times.Never);
    }
}
