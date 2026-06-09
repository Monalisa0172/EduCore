using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services
{
    public class DisciplinaServiceTests
    {
        private readonly Mock<IDisciplinaRepository>
            _disciplinaRepository;

        private readonly DisciplinaService
            _service;

    public DisciplinaServiceTests()
    {
            _disciplinaRepository =
            new Mock<IDisciplinaRepository>();

        _service =
            new DisciplinaService(
                _disciplinaRepository.Object);
    }

    [Fact]
    public async Task Deve_Criar_Disciplina_Com_Sucesso()
        {
            _disciplinaRepository
                .Setup(x =>
                    x.GetByCodigoAsync(It.IsAny<string>()))
                .ReturnsAsync((Disciplina?)null);

            _disciplinaRepository
                .Setup(x =>
                    x.AddAsync(It.IsAny<Disciplina>()))
                .Returns(Task.CompletedTask);

            var request =
                new CreateDisciplinaRequest
                {
                    Nome = "Matemática",
                    Codigo = "MAT001",
                    Descricao = "Matemática Básica",
                    CargaHoraria = 80
                };

            var resultado =
                await _service.CreateAsync(request);

            resultado.Should().NotBeNull();

            resultado!.Nome.Should()
                .Be("Matemática");

            resultado.Codigo.Should()
                .Be("MAT001");

            resultado.Ativo.Should()
                .BeTrue();

            _disciplinaRepository.Verify(
                x => x.AddAsync(It.IsAny<Disciplina>()),
                Times.Once);
        }

    [Fact]
    public async Task Nao_Deve_Criar_Disciplina_Duplicada()
        {
            _disciplinaRepository
                .Setup(x =>
                    x.GetByCodigoAsync(It.IsAny<string>()))
                .ReturnsAsync(new Disciplina());

            var request =
                new CreateDisciplinaRequest
                {
                    Nome = "Matemática",
                    Codigo = "MAT001",
                    Descricao = "Matemática Básica",
                    CargaHoraria = 80
                };

            var resultado =
                await _service.CreateAsync(request);

            resultado.Should().BeNull();

            _disciplinaRepository.Verify(
                x => x.AddAsync(It.IsAny<Disciplina>()),
                Times.Never);
        }

    [Fact]
    public async Task Deve_Inativar_Disciplina_Com_Sucesso()
        {
            var disciplina =
                new Disciplina
                {
                    Id = 1,
                    Nome = "Matemática",
                    Codigo = "MAT001",
                    Ativo = true
                };

            _disciplinaRepository
                .Setup(x =>
                    x.GetEntityByIdAsync(1))
                .ReturnsAsync(disciplina);

            var resultado =
                await _service.DeleteAsync(1);

            resultado.Should().BeTrue();

            disciplina.Ativo.Should()
                .BeFalse();

            _disciplinaRepository.Verify(
                x => x.UpdateAsync(It.IsAny<Disciplina>()),
                Times.Once);
        }

    [Fact]
    public async Task Nao_Deve_Inativar_Disciplina_Inexistente()
        {
            _disciplinaRepository
                .Setup(x =>
                    x.GetEntityByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Disciplina?)null);

            var resultado =
                await _service.DeleteAsync(999);

            resultado.Should().BeFalse();

            _disciplinaRepository.Verify(
                x => x.UpdateAsync(It.IsAny<Disciplina>()),
                Times.Never);
        }

        [Fact]
        public async Task Deve_Buscar_Disciplina_Por_Id()
        {
            var disciplina =
                new API.DTOs.Response.DisciplinaResponseDTO
                {
                    Id = 1,
                    Nome = "Matemática",
                    Codigo = "MAT001",
                    Ativo = true
                };

            _disciplinaRepository
                .Setup(x =>
                    x.GetByIdAsync(1))
                .ReturnsAsync(disciplina);

            var resultado =
                await _service.GetByIdAsync(1);

            resultado.Should().NotBeNull();

            resultado!.Id.Should().Be(1);

            resultado.Nome.Should()
                .Be("Matemática");
        }

        [Fact]
        public async Task Deve_Retornar_Todas_As_Disciplinas()
        {
            var disciplinas =
                new List<API.DTOs.Response.DisciplinaResponseDTO>
                {
            new()
            {
                Id = 1,
                Nome = "Matemática",
                Codigo = "MAT001"
            },
            new()
            {
                Id = 2,
                Nome = "Português",
                Codigo = "POR001"
            }
                };

            _disciplinaRepository
                .Setup(x =>
                    x.GetAllAsync())
                .ReturnsAsync(disciplinas);

            var resultado =
                await _service.GetAllAsync();

            resultado.Should()
                .HaveCount(2);
        }

        [Fact]
        public async Task Deve_Atualizar_Disciplina_Com_Sucesso()
        {
            var disciplina =
                new Disciplina
                {
                    Id = 1,
                    Nome = "Matemática",
                    Codigo = "MAT001",
                    Ativo = true
                };

            _disciplinaRepository
                .Setup(x =>
                    x.GetEntityByIdAsync(1))
                .ReturnsAsync(disciplina);

            var request =
                new CreateDisciplinaRequest
                {
                    Nome = "Matemática Avançada",
                    Codigo = "MAT002",
                    Descricao = "Nova descrição",
                    CargaHoraria = 120
                };

            var resultado =
                await _service.UpdateAsync(
                    1,
                    request);

            resultado.Should().BeTrue();

            disciplina.Nome.Should()
                .Be("Matemática Avançada");

            disciplina.Codigo.Should()
                .Be("MAT002");

            _disciplinaRepository.Verify(
                x => x.UpdateAsync(It.IsAny<Disciplina>()),
                Times.Once);
        }

        [Fact]
        public async Task Nao_Deve_Atualizar_Disciplina_Inexistente()
        {
            _disciplinaRepository
                .Setup(x =>
                    x.GetEntityByIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Disciplina?)null);

            var request =
                new CreateDisciplinaRequest
                {
                    Nome = "Teste",
                    Codigo = "TES001",
                    CargaHoraria = 80
                };

            var resultado =
                await _service.UpdateAsync(
                    999,
                    request);

            resultado.Should().BeFalse();

            _disciplinaRepository.Verify(
                x => x.UpdateAsync(It.IsAny<Disciplina>()),
                Times.Never);
        }

        [Fact]
        public async Task Deve_Criar_Disciplina_Com_Nome_Nulo()
        {
            _disciplinaRepository
                .Setup(x =>
                    x.GetByCodigoAsync(It.IsAny<string>()))
                .ReturnsAsync((Disciplina?)null);

            _disciplinaRepository
                .Setup(x =>
                    x.AddAsync(It.IsAny<Disciplina>()))
                .Returns(Task.CompletedTask);

            var request =
                new CreateDisciplinaRequest
                {
                    Nome = null!,
                    Codigo = "MAT001",
                    Descricao = "Teste",
                    CargaHoraria = 80
                };

            var resultado =
                await _service.CreateAsync(request);

            resultado.Should().NotBeNull();

            resultado!.Nome.Should()
                .Be(string.Empty);
        }
    }
}
