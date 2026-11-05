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
    private const int TurmaId = 1;
    private const int MatematicaId = 10;
    private const int AlgebraId = 100;
    private const int GeometriaId = 101;

    private readonly Mock<IAvaliacaoRepository> _avaliacaoRepository = new();
    private readonly Mock<IAtribuicaoRepository> _atribuicaoRepository = new();
    private readonly Mock<ITurmaRepository> _turmaRepository = new();
    private readonly Mock<IDisciplinaRepository> _disciplinaRepository = new();
    private readonly Mock<ISubDisciplinaRepository> _subDisciplinaRepository = new();

    private readonly AvaliacaoService _service;

    public AvaliacaoServiceTests()
    {
        _service = new AvaliacaoService(
            _avaliacaoRepository.Object,
            _atribuicaoRepository.Object,
            _turmaRepository.Object,
            _disciplinaRepository.Object,
            _subDisciplinaRepository.Object);

        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId))
            .ReturnsAsync(new Turma { Id = TurmaId, Nome = "1º Ano A", Ativo = true });
        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(MatematicaId))
            .ReturnsAsync(new Disciplina { Id = MatematicaId, Nome = "Matemática", Ativo = true });
        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(AlgebraId))
            .ReturnsAsync(new SubDisciplina { Id = AlgebraId, Nome = "Álgebra", DisciplinaId = MatematicaId, Ativo = true });
        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(GeometriaId))
            .ReturnsAsync(new SubDisciplina { Id = GeometriaId, Nome = "Geometria", DisciplinaId = MatematicaId, Ativo = true });
        _avaliacaoRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new AvaliacaoResponseDTO { Id = id });
    }

    private static CreateAvaliacaoRequest Request(decimal peso = 30, int? subDisciplinaId = null) => new()
    {
        TurmaId = TurmaId,
        DisciplinaId = MatematicaId,
        SubDisciplinaId = subDisciplinaId,
        Nome = " Prova 1 ",
        Peso = peso,
        DataAplicacao = new DateTime(2026, 3, 10),
        Bimestre = BimestreEnum.Primeiro
    };

    private void AtribuidaInteira()
        => _atribuicaoRepository.Setup(x => x.GetAtivasDaDisciplinaAsync(TurmaId, MatematicaId))
            .ReturnsAsync([new AtribuicaoResponseDTO { Id = 1, ProfessorNome = "Ana" }]);

    private void AtribuidaSoAlgebra()
        => _atribuicaoRepository.Setup(x => x.GetAtivasDaDisciplinaAsync(TurmaId, MatematicaId))
            .ReturnsAsync([new AtribuicaoResponseDTO { Id = 1, ProfessorNome = "Ana", SubDisciplinaId = AlgebraId, SubDisciplinaNome = "Álgebra" }]);

    private void PesoJaUsado(decimal total, int? ignorar = null)
        => _avaliacaoRepository.Setup(x => x.GetPesoTotalAsync(TurmaId, MatematicaId, 1, ignorar)).ReturnsAsync(total);

    [Fact]
    public async Task Deve_Criar_Avaliacao_Com_Sucesso()
    {
        AtribuidaInteira();
        PesoJaUsado(40);

        var resultado = await _service.CriarAsync(Request(peso: 30));

        resultado.Sucesso.Should().BeTrue();
        _avaliacaoRepository.Verify(x => x.AddAsync(It.Is<Avaliacao>(a =>
            a.TurmaId == TurmaId && a.DisciplinaId == MatematicaId && a.Nome == "Prova 1" &&
            a.Peso == 30 && a.Bimestre == 1 && a.Ativo)), Times.Once);
    }

    [Fact]
    public async Task Deve_Permitir_Peso_Que_Completa_Exatamente_100()
    {
        AtribuidaInteira();
        PesoJaUsado(70);

        var resultado = await _service.CriarAsync(Request(peso: 30));

        resultado.Sucesso.Should().BeTrue();
    }

    [Fact]
    public async Task Nao_Deve_Ultrapassar_100_No_Bimestre()
    {
        AtribuidaInteira();
        PesoJaUsado(70);

        var resultado = await _service.CriarAsync(Request(peso: 40));

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Campo.Should().Be(nameof(UpdateAvaliacaoRequest.Peso));
        resultado.Mensagem.Should().Contain("no máximo 30");
        _avaliacaoRepository.Verify(x => x.AddAsync(It.IsAny<Avaliacao>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Avisar_Quando_Bimestre_Ja_Esta_Completo()
    {
        AtribuidaInteira();
        PesoJaUsado(100);

        var resultado = await _service.CriarAsync(Request(peso: 10));

        resultado.Mensagem.Should().Contain("já somam 100");
    }

    [Fact]
    public async Task Nao_Deve_Criar_Em_Turma_Inativa()
    {
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId))
            .ReturnsAsync(new Turma { Id = TurmaId, Ativo = false });

        var resultado = await _service.CriarAsync(Request());

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Campo.Should().Be(nameof(CreateAvaliacaoRequest.TurmaId));
    }

    [Fact]
    public async Task Nao_Deve_Criar_Sem_Professor_Atribuido()
    {
        _atribuicaoRepository.Setup(x => x.GetAtivasDaDisciplinaAsync(TurmaId, MatematicaId)).ReturnsAsync([]);

        var resultado = await _service.CriarAsync(Request());

        resultado.Campo.Should().Be(nameof(CreateAvaliacaoRequest.DisciplinaId));
        resultado.Mensagem.Should().Contain("não tem professor atribuído");
    }

    [Fact]
    public async Task Disciplina_Dividida_Exige_Subdisciplina()
    {
        AtribuidaSoAlgebra();

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: null));

        resultado.Campo.Should().Be(nameof(UpdateAvaliacaoRequest.SubDisciplinaId));
        resultado.Mensagem.Should().Contain("Álgebra");
    }

    [Fact]
    public async Task Disciplina_Dividida_Recusa_Subdisciplina_Sem_Professor()
    {
        AtribuidaSoAlgebra();

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: GeometriaId));

        resultado.Campo.Should().Be(nameof(UpdateAvaliacaoRequest.SubDisciplinaId));
        resultado.Mensagem.Should().Contain("Geometria não tem professor");
    }

    [Fact]
    public async Task Disciplina_Dividida_Aceita_Subdisciplina_Atribuida()
    {
        AtribuidaSoAlgebra();
        PesoJaUsado(0);

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: AlgebraId));

        resultado.Sucesso.Should().BeTrue();
        _avaliacaoRepository.Verify(x => x.AddAsync(It.Is<Avaliacao>(a => a.SubDisciplinaId == AlgebraId)), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Aceitar_Subdisciplina_De_Outra_Disciplina()
    {
        AtribuidaInteira();
        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(999))
            .ReturnsAsync(new SubDisciplina { Id = 999, DisciplinaId = 55, Ativo = true });

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: 999));

        resultado.Campo.Should().Be(nameof(UpdateAvaliacaoRequest.SubDisciplinaId));
        resultado.Mensagem.Should().Contain("não pertence");
    }

    [Fact]
    public async Task Atualizar_Desconsidera_O_Proprio_Peso()
    {
        var avaliacao = new Avaliacao { Id = 7, TurmaId = TurmaId, DisciplinaId = MatematicaId, Peso = 50, Bimestre = 1, Ativo = true };
        _avaliacaoRepository.Setup(x => x.GetEntityByIdAsync(7)).ReturnsAsync(avaliacao);
        // Outras avaliacoes somam 50: a propria pode ir de 50 para 50 (ou menos) sem erro.
        PesoJaUsado(50, ignorar: 7);

        var resultado = await _service.AtualizarAsync(7, Request(peso: 50));

        resultado.Sucesso.Should().BeTrue();
        avaliacao.Nome.Should().Be("Prova 1");
        _avaliacaoRepository.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Atualizar_Inexistente_Retorna_Nao_Encontrado()
    {
        var resultado = await _service.AtualizarAsync(404, Request());

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
    }

    [Fact]
    public async Task Deve_Inativar_Avaliacao()
    {
        var avaliacao = new Avaliacao { Id = 7, Ativo = true };
        _avaliacaoRepository.Setup(x => x.GetEntityByIdAsync(7)).ReturnsAsync(avaliacao);

        var resultado = await _service.InativarAsync(7);

        resultado.Sucesso.Should().BeTrue();
        avaliacao.Ativo.Should().BeFalse();
        _avaliacaoRepository.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Reativar_Se_O_Peso_Nao_Couber()
    {
        var avaliacao = new Avaliacao { Id = 7, TurmaId = TurmaId, DisciplinaId = MatematicaId, Peso = 40, Bimestre = 1, Ativo = false };
        _avaliacaoRepository.Setup(x => x.GetEntityByIdAsync(7)).ReturnsAsync(avaliacao);
        PesoJaUsado(80, ignorar: 7);

        var resultado = await _service.ReativarAsync(7);

        resultado.Campo.Should().Be(nameof(UpdateAvaliacaoRequest.Peso));
        avaliacao.Ativo.Should().BeFalse();
        _avaliacaoRepository.Verify(x => x.SalvarAsync(), Times.Never);
    }
}
