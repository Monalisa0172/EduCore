using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class DisciplinaServiceTests
{
    private readonly Mock<IDisciplinaRepository> _disciplinaRepository = new();

    private readonly DisciplinaService _service;

    public DisciplinaServiceTests()
    {
        _service = new DisciplinaService(_disciplinaRepository.Object);
    }

    private static CreateDisciplinaRequest Request(string codigo = "MAT001") => new()
    {
        Nome = "Matemática",
        Codigo = codigo,
        Descricao = "Matemática Básica",
        CargaHoraria = 80
    };

    [Fact]
    public async Task Deve_Criar_Disciplina_Com_Sucesso()
    {
        _disciplinaRepository
            .Setup(x => x.GetByCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Disciplina?)null);

        var resultado = await _service.CreateAsync(Request());

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Nome.Should().Be("Matemática");
        resultado.Valor.Codigo.Should().Be("MAT001");
        resultado.Valor.Ativo.Should().BeTrue();
        resultado.Valor.QuantidadeSubDisciplinas.Should().Be(0);

        _disciplinaRepository.Verify(x => x.AddAsync(It.IsAny<Disciplina>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Gravar_Codigo_Em_Maiusculas_E_Sem_Espacos()
    {
        _disciplinaRepository
            .Setup(x => x.GetByCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Disciplina?)null);

        var resultado = await _service.CreateAsync(Request(codigo: "  mat-01 "));

        resultado.Valor!.Codigo.Should().Be("MAT-01");
        _disciplinaRepository.Verify(x => x.GetByCodigoAsync("MAT-01"), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Duplicada()
    {
        _disciplinaRepository
            .Setup(x => x.GetByCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync(new Disciplina { Id = 7 });

        var resultado = await _service.CreateAsync(Request());

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        resultado.Campo.Should().Be(nameof(CreateDisciplinaRequest.Codigo));

        _disciplinaRepository.Verify(x => x.AddAsync(It.IsAny<Disciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Criar_Disciplina_Com_Nome_Nulo()
    {
        // A validacao do request barra nome vazio na API; o servico apenas nao pode quebrar.
        _disciplinaRepository
            .Setup(x => x.GetByCodigoAsync(It.IsAny<string>()))
            .ReturnsAsync((Disciplina?)null);

        var request = Request();
        request.Nome = null!;

        var resultado = await _service.CreateAsync(request);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Nome.Should().Be(string.Empty);
    }

    [Fact]
    public async Task Deve_Atualizar_Disciplina_Com_Sucesso()
    {
        var disciplina = new Disciplina { Id = 1, Nome = "Matemática", Codigo = "MAT001", Ativo = true };

        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(disciplina);

        var request = new CreateDisciplinaRequest
        {
            Nome = "Matemática Avançada",
            Codigo = "MAT002",
            Descricao = "Nova descrição",
            CargaHoraria = 120
        };

        var resultado = await _service.UpdateAsync(1, request);

        resultado.Sucesso.Should().BeTrue();
        disciplina.Nome.Should().Be("Matemática Avançada");
        disciplina.Codigo.Should().Be("MAT002");
        disciplina.CargaHoraria.Should().Be(120);

        _disciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<Disciplina>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Atualizar_Mantendo_O_Proprio_Codigo()
    {
        var disciplina = new Disciplina { Id = 1, Nome = "Matemática", Codigo = "MAT001", Ativo = true };

        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(disciplina);
        _disciplinaRepository.Setup(x => x.GetByCodigoAsync("MAT001")).ReturnsAsync(disciplina);

        var resultado = await _service.UpdateAsync(1, Request(codigo: "MAT001"));

        resultado.Sucesso.Should().BeTrue();
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Com_Codigo_De_Outra_Disciplina()
    {
        var disciplina = new Disciplina { Id = 1, Codigo = "MAT001", Ativo = true };

        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(disciplina);
        _disciplinaRepository.Setup(x => x.GetByCodigoAsync("POR001")).ReturnsAsync(new Disciplina { Id = 2, Codigo = "POR001" });

        var resultado = await _service.UpdateAsync(1, Request(codigo: "POR001"));

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        disciplina.Codigo.Should().Be("MAT001");
        _disciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<Disciplina>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Disciplina_Inexistente()
    {
        _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Disciplina?)null);

        var resultado = await _service.UpdateAsync(999, Request());

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _disciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<Disciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Inativar_Disciplina_Com_Sucesso()
    {
        var disciplina = new Disciplina { Id = 1, Nome = "Matemática", Codigo = "MAT001", Ativo = true };

        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(disciplina);

        var resultado = await _service.InativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        disciplina.Ativo.Should().BeFalse();
        _disciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<Disciplina>()), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_Disciplina_Inexistente()
    {
        _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Disciplina?)null);

        var resultado = await _service.InativarAsync(999);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _disciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<Disciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Reativar_Disciplina_Inativa()
    {
        var disciplina = new Disciplina { Id = 1, Codigo = "MAT001", Ativo = false };

        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(1)).ReturnsAsync(disciplina);

        var resultado = await _service.ReativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        disciplina.Ativo.Should().BeTrue();
        _disciplinaRepository.Verify(x => x.UpdateAsync(disciplina), Times.Once);
    }

    [Fact]
    public async Task Reativar_Disciplina_Ja_Ativa_Nao_Grava_Nada()
    {
        _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(new Disciplina { Id = 1, Codigo = "MAT001", Ativo = true });

        var resultado = await _service.ReativarAsync(1);

        resultado.Sucesso.Should().BeTrue();
        _disciplinaRepository.Verify(x => x.UpdateAsync(It.IsAny<Disciplina>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Buscar_Disciplina_Por_Id()
    {
        _disciplinaRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(new DisciplinaResponseDTO { Id = 1, Nome = "Matemática", Codigo = "MAT001", Ativo = true });

        var resultado = await _service.GetByIdAsync(1);

        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Matemática");
    }

    [Fact]
    public async Task Deve_Retornar_Todas_As_Disciplinas()
    {
        _disciplinaRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(
            [
                new DisciplinaResponseDTO { Id = 1, Nome = "Matemática", Codigo = "MAT001" },
                new DisciplinaResponseDTO { Id = 2, Nome = "Português", Codigo = "POR001" }
            ]);

        var resultado = await _service.GetAllAsync();

        resultado.Should().HaveCount(2);
    }
}
