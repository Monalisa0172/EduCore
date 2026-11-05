using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class AtribuicaoServiceTests
{
    private const int TurmaId = 1;
    private const int PortuguesId = 10;
    private const int GramaticaId = 100;
    private const int LiteraturaId = 101;
    private const int AnaId = 5;
    private const int BrunoId = 6;

    private readonly Mock<IAtribuicaoRepository> _atribuicaoRepository = new();
    private readonly Mock<IProfessorRepository> _professorRepository = new();
    private readonly Mock<ITurmaRepository> _turmaRepository = new();
    private readonly Mock<IDisciplinaRepository> _disciplinaRepository = new();
    private readonly Mock<ISubDisciplinaRepository> _subDisciplinaRepository = new();

    private readonly AtribuicaoService _service;

    public AtribuicaoServiceTests()
    {
        _service = new AtribuicaoService(
            _atribuicaoRepository.Object,
            _professorRepository.Object,
            _turmaRepository.Object,
            _disciplinaRepository.Object,
            _subDisciplinaRepository.Object);

        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId))
            .ReturnsAsync(new Turma { Id = TurmaId, Nome = "1º Ano A", Ativo = true });
        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(PortuguesId))
            .ReturnsAsync(new Disciplina { Id = PortuguesId, Nome = "Português", Ativo = true });
        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(GramaticaId))
            .ReturnsAsync(new SubDisciplina { Id = GramaticaId, Nome = "Gramática", DisciplinaId = PortuguesId, Ativo = true });
        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(LiteraturaId))
            .ReturnsAsync(new SubDisciplina { Id = LiteraturaId, Nome = "Literatura", DisciplinaId = PortuguesId, Ativo = true });
        _professorRepository.Setup(x => x.GetEntityByIdAsync(AnaId)).ReturnsAsync(Professor(AnaId, "Ana"));
        _professorRepository.Setup(x => x.GetEntityByIdAsync(BrunoId)).ReturnsAsync(Professor(BrunoId, "Bruno"));
        _atribuicaoRepository.Setup(x => x.GetAtivasDaDisciplinaAsync(TurmaId, PortuguesId)).ReturnsAsync([]);
        _atribuicaoRepository.Setup(x => x.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => new AtribuicaoResponseDTO { Id = id });
    }

    private static Professor Professor(int id, string nome, bool ativo = true) => new()
    {
        Id = id,
        Funcionario = new Funcionario { Ativo = ativo, Usuario = new Usuario { Nome = nome, Ativo = ativo } }
    };

    private static AtribuicaoRequest Request(int professorId = AnaId, int? subDisciplinaId = null) => new()
    {
        ProfessorId = professorId,
        TurmaId = TurmaId,
        DisciplinaId = PortuguesId,
        SubDisciplinaId = subDisciplinaId
    };

    private void JaAtribuidas(params AtribuicaoResponseDTO[] ativas)
        => _atribuicaoRepository.Setup(x => x.GetAtivasDaDisciplinaAsync(TurmaId, PortuguesId)).ReturnsAsync(ativas.ToList());

    private static AtribuicaoResponseDTO Ativa(int id, string professor, int? subId = null, string? subNome = null)
        => new() { Id = id, ProfessorNome = professor, SubDisciplinaId = subId, SubDisciplinaNome = subNome, Ativo = true };

    [Fact]
    public async Task Deve_Atribuir_Disciplina_Inteira()
    {
        var resultado = await _service.CriarAsync(Request());

        resultado.Sucesso.Should().BeTrue();
        _atribuicaoRepository.Verify(x => x.AddAsync(It.Is<Atribuicao>(a =>
            a.ProfessorId == AnaId && a.TurmaId == TurmaId && a.DisciplinaId == PortuguesId &&
            a.SubDisciplinaId == null && a.Ativo)), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Ter_Dois_Professores_No_Mesmo_Item()
    {
        JaAtribuidas(Ativa(1, "Bruno"));

        var resultado = await _service.CriarAsync(Request());

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        resultado.Campo.Should().Be(nameof(AtribuicaoRequest.DisciplinaId));
        resultado.Mensagem.Should().StartWith("Bruno já leciona Português");
        _atribuicaoRepository.Verify(x => x.AddAsync(It.IsAny<Atribuicao>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Atribuir_Inteira_Quando_Ja_Dividida()
    {
        JaAtribuidas(Ativa(1, "Bruno", GramaticaId, "Gramática"));

        var resultado = await _service.CriarAsync(Request());

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        resultado.Campo.Should().Be(nameof(AtribuicaoRequest.DisciplinaId));
        resultado.Mensagem.Should().Contain("dividida por subdisciplinas").And.Contain("Gramática");
    }

    [Fact]
    public async Task Nao_Deve_Dividir_Quando_Ja_Atribuida_Inteira()
    {
        JaAtribuidas(Ativa(1, "Bruno"));

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: LiteraturaId));

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        resultado.Campo.Should().Be(nameof(AtribuicaoRequest.SubDisciplinaId));
        resultado.Mensagem.Should().Contain("Português inteira");
    }

    [Fact]
    public async Task Deve_Atribuir_Outra_Subdisciplina_A_Outro_Professor()
    {
        JaAtribuidas(Ativa(1, "Bruno", GramaticaId, "Gramática"));

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: LiteraturaId));

        resultado.Sucesso.Should().BeTrue();
        _atribuicaoRepository.Verify(x => x.AddAsync(It.Is<Atribuicao>(a => a.SubDisciplinaId == LiteraturaId)), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Atribuir_Professor_Inativo()
    {
        _professorRepository.Setup(x => x.GetEntityByIdAsync(AnaId)).ReturnsAsync(Professor(AnaId, "Ana", ativo: false));

        var resultado = await _service.CriarAsync(Request());

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Campo.Should().Be(nameof(AtribuicaoRequest.ProfessorId));
    }

    [Fact]
    public async Task Nao_Deve_Atribuir_Em_Turma_Inativa()
    {
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId)).ReturnsAsync(new Turma { Id = TurmaId, Ativo = false });

        var resultado = await _service.CriarAsync(Request());

        resultado.Campo.Should().Be(nameof(AtribuicaoRequest.TurmaId));
    }

    [Fact]
    public async Task Nao_Deve_Aceitar_Subdisciplina_De_Outra_Disciplina()
    {
        _subDisciplinaRepository.Setup(x => x.GetEntityByIdAsync(999))
            .ReturnsAsync(new SubDisciplina { Id = 999, DisciplinaId = 55, Ativo = true });

        var resultado = await _service.CriarAsync(Request(subDisciplinaId: 999));

        resultado.Campo.Should().Be(nameof(AtribuicaoRequest.SubDisciplinaId));
    }

    [Fact]
    public async Task Deve_Trocar_O_Professor()
    {
        var atribuicao = new Atribuicao { Id = 3, ProfessorId = AnaId, TurmaId = TurmaId, DisciplinaId = PortuguesId, Ativo = true };
        _atribuicaoRepository.Setup(x => x.GetEntityByIdAsync(3)).ReturnsAsync(atribuicao);

        var resultado = await _service.TrocarProfessorAsync(3, new TrocarProfessorRequest { ProfessorId = BrunoId });

        resultado.Sucesso.Should().BeTrue();
        atribuicao.ProfessorId.Should().Be(BrunoId);
        _atribuicaoRepository.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Trocar_Professor_De_Atribuicao_Encerrada()
    {
        _atribuicaoRepository.Setup(x => x.GetEntityByIdAsync(3))
            .ReturnsAsync(new Atribuicao { Id = 3, ProfessorId = AnaId, Ativo = false });

        var resultado = await _service.TrocarProfessorAsync(3, new TrocarProfessorRequest { ProfessorId = BrunoId });

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        _atribuicaoRepository.Verify(x => x.SalvarAsync(), Times.Never);
    }

    [Fact]
    public async Task Nao_Deve_Reativar_Com_Conflito()
    {
        var atribuicao = new Atribuicao { Id = 3, ProfessorId = AnaId, TurmaId = TurmaId, DisciplinaId = PortuguesId, Ativo = false };
        _atribuicaoRepository.Setup(x => x.GetEntityByIdAsync(3)).ReturnsAsync(atribuicao);
        JaAtribuidas(Ativa(8, "Bruno"));

        var resultado = await _service.ReativarAsync(3);

        resultado.Falha.Should().Be(FalhaOperacao.Conflito);
        atribuicao.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Deve_Encerrar_Atribuicao()
    {
        var atribuicao = new Atribuicao { Id = 3, Ativo = true };
        _atribuicaoRepository.Setup(x => x.GetEntityByIdAsync(3)).ReturnsAsync(atribuicao);

        var resultado = await _service.InativarAsync(3);

        resultado.Sucesso.Should().BeTrue();
        atribuicao.Ativo.Should().BeFalse();
    }

    [Fact]
    public async Task Encerrar_Inexistente_Retorna_Nao_Encontrado()
    {
        var resultado = await _service.InativarAsync(404);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
    }
}
