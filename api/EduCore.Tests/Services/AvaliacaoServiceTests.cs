using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class AvaliacaoServiceTests
{
    private readonly Mock<IAvaliacaoRepository>
    _avaliacaoRepository;

private readonly Mock<IDisciplinaRepository>
    _disciplinaRepository;

private readonly Mock<ISubDisciplinaRepository>
    _subDisciplinaRepository;

    private readonly AvaliacaoService
        _service;

    public AvaliacaoServiceTests()
    {
        _avaliacaoRepository =
            new Mock<IAvaliacaoRepository>();

        _disciplinaRepository =
            new Mock<IDisciplinaRepository>();

        _subDisciplinaRepository =             
            new Mock<ISubDisciplinaRepository>();

        _service =
            new AvaliacaoService(
                _avaliacaoRepository.Object,
                _subDisciplinaRepository.Object,
                _disciplinaRepository.Object);
    }

    [Fact]
    public async Task Deve_Criar_Avaliacao_Com_Sucesso()
    {
        var disciplina =
            new Disciplina
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
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(
                new SubDisciplina
                {
                    Id = 1,
                    DisciplinaId = 1,
                    Nome = "Capoeira",
                    Ativo = true
                }
            );

        _avaliacaoRepository
            .Setup(x =>
                x.GetPesoTotalByDisciplinaEBimestreAsync(
                    1,
                    BimestreEnum.Primeiro,
                    null))
            .ReturnsAsync(40);

        _avaliacaoRepository
            .Setup(x =>
                x.AddAsync(It.IsAny<Avaliacao>()))
            .Returns(Task.CompletedTask);

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Nome = "Prova Mensal",
                Peso = 30,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().NotBeNull();

        resultado!.Nome.Should()
            .Be("Prova Mensal");

        resultado.Peso.Should()
            .Be(30);

        _avaliacaoRepository.Verify(
            x => x.AddAsync(It.IsAny<Avaliacao>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Avaliacao_Sem_Disciplina()
    {
        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Disciplina?)null);

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                Nome = "Prova",
                Peso = 20,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().BeNull();

        _avaliacaoRepository.Verify(
            x => x.AddAsync(It.IsAny<Avaliacao>()),
            Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Avaliacao_Com_Peso_Maior_Que_100()
    {
        var disciplina =
            new Disciplina
            {
                Id = 1
            };

        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(disciplina);

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                Nome = "Prova",
                Peso = 120,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Nao_Deve_Criar_Avaliacao_Quando_Soma_Do_Bimestre_Ultrapassar_100()
    {
        var disciplina =
            new Disciplina
            {
                Id = 1
            };

        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(disciplina);

        _avaliacaoRepository
            .Setup(x =>
                x.GetPesoTotalByDisciplinaEBimestreAsync(
                    1,
                    BimestreEnum.Primeiro,
                    null))
            .ReturnsAsync(80);

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                Nome = "Prova Final",
                Peso = 30,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().BeNull();

        _avaliacaoRepository.Verify(
            x => x.AddAsync(It.IsAny<Avaliacao>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Atualizar_Avaliacao_Com_Sucesso()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)BimestreEnum.Primeiro
            };
        _avaliacaoRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        _avaliacaoRepository
            .Setup(x =>
                x.GetPesoTotalByDisciplinaEBimestreAsync(
                    1,
                    BimestreEnum.Primeiro,
                    1))
            .ReturnsAsync(40);

        _avaliacaoRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<Avaliacao>()))
            .Returns(Task.CompletedTask);

        var request =
            new UpdateAvaliacaoRequest
            {
                Nome = "Prova Atualizada",
                Peso = 30,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeTrue();

        _avaliacaoRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Avaliacao>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Avaliacao_Inexistente()
    {
        _avaliacaoRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Avaliacao?)null);

        var request =
            new UpdateAvaliacaoRequest
            {
                Nome = "Teste",
                Peso = 20,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.UpdateAsync(
                999,
                request);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Avaliacao_Quando_Soma_Do_Bimestre_Ultrapassar_100()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)BimestreEnum.Primeiro
            };

        _avaliacaoRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        _avaliacaoRepository
            .Setup(x =>
                x.GetPesoTotalByDisciplinaEBimestreAsync(
                    1,
                    BimestreEnum.Primeiro,
                    1))
            .ReturnsAsync(90);

        var request =
            new UpdateAvaliacaoRequest
            {
                Nome = "Prova Atualizada",
                Peso = 20,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeFalse();

        _avaliacaoRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Avaliacao>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Inativar_Avaliacao_Com_Sucesso()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                Ativo = true
            };

        _avaliacaoRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        _avaliacaoRepository
            .Setup(x =>
                x.UpdateAsync(It.IsAny<Avaliacao>()))
            .Returns(Task.CompletedTask);

        var resultado =
            await _service.DeleteAsync(1);

        resultado.Should().BeTrue();

        avaliacao.Ativo.Should()
            .BeFalse();

        _avaliacaoRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Avaliacao>()),
            Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Inativar_Avaliacao_Inexistente()
    {
        _avaliacaoRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Avaliacao?)null);

        var resultado =
            await _service.DeleteAsync(999);

        resultado.Should().BeFalse();

        _avaliacaoRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Avaliacao>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Retornar_Todas_As_Avaliacoes()
    {
        var lista = new List<AvaliacaoResponseDTO>
    {
        new()
        {
            Id = 1,
            Nome = "Prova"
        }
    };

        _avaliacaoRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(lista);

        var resultado =
            await _service.GetAllAsync();

        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task Deve_Retornar_Avaliacao_Por_Id()
    {
        var avaliacao =
            new AvaliacaoResponseDTO
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Nome = "Prova"
            };

        _avaliacaoRepository
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(avaliacao);

        var resultado =
            await _service.GetByIdAsync(1);

        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
    }

    [Fact]
    public async Task Deve_Retornar_Avaliacoes_Por_Disciplina()
    {
        var lista =
            new List<AvaliacaoResponseDTO>
            {
            new()
            {
                Id = 1,
                DisciplinaId = 1
            }
            };

        _avaliacaoRepository
            .Setup(x =>
                x.GetByDisciplinaAsync(1))
            .ReturnsAsync(lista);

        var resultado =
            await _service.GetByDisciplinaAsync(1);

        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task Nao_Deve_Criar_Avaliacao_Com_Bimestre_Invalido()
    {
        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Disciplina());

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                Nome = "Prova Mensal",
                Peso = 30,
                DataAplicacao = DateTime.Now,
                Bimestre = (BimestreEnum)99
            };


        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Nao_Deve_Criar_Avaliacao_Com_Peso_Zero()
    {
        _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(new Disciplina());

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                Nome = "Prova Mensal",
                Peso = 0,
                DataAplicacao = DateTime.Now,
                Bimestre = (BimestreEnum)99
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().BeNull();
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Com_Bimestre_Invalido()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)(BimestreEnum)99
            };
        _avaliacaoRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        var request =
            new UpdateAvaliacaoRequest
            {
                Nome = "Teste",
                Peso = 20,
                DataAplicacao = DateTime.Now,
                Bimestre = (BimestreEnum)(int)(BimestreEnum)99
            };
        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Com_Peso_Zero()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)BimestreEnum.Primeiro
            };

        _avaliacaoRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        var request =
            new UpdateAvaliacaoRequest
            {
                Nome = "Teste",
                Peso = 0,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeFalse();
    }


    [Fact]
    public async Task Nao_Deve_Atualizar_Com_Peso_Maior_Que_100()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)BimestreEnum.Primeiro
            };

        _avaliacaoRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        var request =
            new UpdateAvaliacaoRequest
            {
                Nome = "Teste",
                Peso = 200,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Deve_Criar_Avaliacao_Com_SubDisciplina_Valida()
    {
        var disciplina =
            new Disciplina
            {
                Id = 1,
                SubDisciplinas =
                [
                    new SubDisciplina
            {
                Id = 1,
                DisciplinaId = 1
            }
                ]
            };

        _disciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(disciplina);

        _subDisciplinaRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(
                new SubDisciplina
                {
                    Id = 1,
                    DisciplinaId = 1,
                    Nome = "Capoeira",
                    Ativo = true
                });

        _avaliacaoRepository
            .Setup(x => x.GetPesoTotalByDisciplinaEBimestreAsync(
                1,
                BimestreEnum.Primeiro,
                null))
            .ReturnsAsync(20);

        _avaliacaoRepository
            .Setup(x => x.AddAsync(It.IsAny<Avaliacao>()))
            .Returns(Task.CompletedTask);

        var request = new CreateAvaliacaoRequest
        {
            DisciplinaId = 1,
            SubDisciplinaId = 1,
            Nome = "Prova",
            Peso = 20,
            Bimestre = BimestreEnum.Primeiro,
            DataAplicacao = DateTime.Now
        };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().NotBeNull();
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Com_SubDisciplina_Invalida()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)BimestreEnum.Primeiro
            };

        _avaliacaoRepository
            .Setup(x => x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        var request = new UpdateAvaliacaoRequest
        {
            Nome = "Nova",
            Peso = 20,
            Bimestre = BimestreEnum.Primeiro,
            SubDisciplinaId = 999
        };

        var resultado =
            await _service.UpdateAsync(1, request);

        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task Nao_Deve_Criar_Avaliacao_Quando_SubDisciplina_Nao_Existir()
    {
        var disciplina =
            new Disciplina
            {
                Id = 1
            };

        _disciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(disciplina);

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync((SubDisciplina?)null);

        var request =
            new CreateAvaliacaoRequest
            {
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Nome = "Prova",
                Peso = 20,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.CreateAsync(request);

        resultado.Should().BeNull();

        _avaliacaoRepository.Verify(
            x => x.AddAsync(It.IsAny<Avaliacao>()),
            Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Quando_SubDisciplina_Nao_Existir()
    {
        var avaliacao =
            new Avaliacao
            {
                Id = 1,
                DisciplinaId = 1,
                SubDisciplinaId = 1,
                Peso = 20,
                Bimestre = (int)BimestreEnum.Primeiro,
                Ativo = true
            };

        _avaliacaoRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync(avaliacao);

        _subDisciplinaRepository
            .Setup(x =>
                x.GetEntityByIdAsync(1))
            .ReturnsAsync((SubDisciplina?)null);

        var request =
            new UpdateAvaliacaoRequest
            {
                SubDisciplinaId = 1,
                Nome = "Prova Atualizada",
                Peso = 20,
                DataAplicacao = DateTime.Now,
                Bimestre = BimestreEnum.Primeiro
            };

        var resultado =
            await _service.UpdateAsync(
                1,
                request);

        resultado.Should().BeFalse();

        _avaliacaoRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Avaliacao>()),
            Times.Never);
    }

}
