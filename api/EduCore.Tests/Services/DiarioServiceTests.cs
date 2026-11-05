using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Acesso;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class DiarioServiceTests
{
    private const int TurmaId = 1;
    private const int PortuguesId = 10;
    private const int ProfessorId = 5;

    private readonly Mock<IAcessoAcademico> _acesso = new();
    private readonly Mock<ITurmaRepository> _turmaRepository = new();
    private readonly Mock<IDisciplinaRepository> _disciplinaRepository = new();
    private readonly Mock<IAtribuicaoRepository> _atribuicaoRepository = new();
    private readonly Mock<IAvaliacaoRepository> _avaliacaoRepository = new();

    private readonly DiarioService _service;

    public DiarioServiceTests()
    {
        _service = new DiarioService(
            _acesso.Object, _turmaRepository.Object, _disciplinaRepository.Object,
            _atribuicaoRepository.Object, _avaliacaoRepository.Object,
            CalendarioPadrao.Criar(), new RelogioFixo(new DateOnly(2026, 5, 10)));

        _acesso.Setup(x => x.PodeFazerChamadaAsync(TurmaId, PortuguesId)).ReturnsAsync(true);
        _acesso.Setup(x => x.ProfessorIdAsync()).ReturnsAsync(ProfessorId);
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId))
            .ReturnsAsync(new Turma { Id = TurmaId, Nome = "9º Ano A", AnoLetivo = 2026, Ativo = true });
        _turmaRepository.Setup(x => x.GetByIdAsync(TurmaId))
            .ReturnsAsync(new TurmaResponseDTO { Id = TurmaId, QuantidadeAlunos = 8 });
        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(PortuguesId))
            .ReturnsAsync(new Disciplina { Id = PortuguesId, Nome = "Português", Ativo = true });
        _avaliacaoRepository.Setup(x => x.GetPesosPorBimestreAsync(TurmaId, PortuguesId))
            .ReturnsAsync(new Dictionary<int, decimal> { [1] = 100, [2] = 50 });
        _atribuicaoRepository.Setup(x => x.GetAtivasDoProfessorAsync(ProfessorId, TurmaId, PortuguesId))
            .ReturnsAsync([new AtribuicaoResponseDTO { SubDisciplinaId = 100, SubDisciplinaNome = "Gramática", ProfessorNome = "Paulo" }]);
        _atribuicaoRepository.Setup(x => x.GetAtivasDaDisciplinaAsync(TurmaId, PortuguesId))
            .ReturnsAsync(
            [
                new AtribuicaoResponseDTO { SubDisciplinaId = 100, SubDisciplinaNome = "Gramática", ProfessorNome = "Paulo" },
                new AtribuicaoResponseDTO { SubDisciplinaId = 101, SubDisciplinaNome = "Literatura", ProfessorNome = "Lívia" }
            ]);
    }

    [Fact]
    public async Task Traz_Bimestre_Vigente_E_Pesos_De_Toda_A_Disciplina()
    {
        var diario = (await _service.ObterAsync(TurmaId, PortuguesId)).Valor!;

        diario.BimestreVigente.Should().Be(2);
        diario.AlunosAtivos.Should().Be(8);
        diario.Bimestres.Should().HaveCount(4);
        diario.Bimestres[0].Should().BeEquivalentTo(new { Bimestre = 1, PesoTotal = 100m, PesoDisponivel = 0m, Encerrado = true });
        diario.Bimestres[1].Should().BeEquivalentTo(new { Bimestre = 2, PesoTotal = 50m, PesoDisponivel = 50m, Encerrado = false });
        diario.Bimestres[2].PesoDisponivel.Should().Be(100);
    }

    [Fact]
    public async Task Professor_Ve_So_O_Que_Leciona()
    {
        var diario = (await _service.ObterAsync(TurmaId, PortuguesId)).Valor!;

        diario.Atribuicoes.Should().ContainSingle().Which.SubDisciplinaNome.Should().Be("Gramática");
    }

    [Fact]
    public async Task Admin_Ve_Todas_As_Atribuicoes()
    {
        _acesso.SetupGet(x => x.EhAdmin).Returns(true);
        _acesso.Setup(x => x.ProfessorIdAsync()).ReturnsAsync((int?)null);

        var diario = (await _service.ObterAsync(TurmaId, PortuguesId)).Valor!;

        diario.Atribuicoes.Select(a => a.SubDisciplinaNome).Should().Equal("Gramática", "Literatura");
    }

    [Fact]
    public async Task Quem_Nao_Leciona_Recebe_Nao_Encontrado()
    {
        _acesso.Setup(x => x.PodeFazerChamadaAsync(TurmaId, PortuguesId)).ReturnsAsync(false);

        var resultado = await _service.ObterAsync(TurmaId, PortuguesId);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _turmaRepository.Verify(x => x.GetEntityByIdAsync(It.IsAny<int>()), Times.Never);
    }
}
