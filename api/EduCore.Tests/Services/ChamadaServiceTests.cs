using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Acesso;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class ChamadaServiceTests
{
    private const int TurmaId = 1;
    private const int DisciplinaId = 10;
    private const int Ana = 11;
    private const int Bia = 12;
    private static readonly DateOnly Hoje = new(2026, 5, 10);

    private readonly Mock<IPresencaRepository> _presencaRepository = new();
    private readonly Mock<ITurmaRepository> _turmaRepository = new();
    private readonly Mock<IDisciplinaRepository> _disciplinaRepository = new();
    private readonly Mock<IAcessoAcademico> _acesso = new();

    private readonly Turma _turma = new() { Id = TurmaId, Nome = "1º Ano A", AnoLetivo = 2026, Ativo = true };
    private readonly List<Presenca> _registros = [];

    private readonly ChamadaService _service;

    public ChamadaServiceTests()
    {
        _service = new ChamadaService(
            _presencaRepository.Object, _turmaRepository.Object, _disciplinaRepository.Object,
            _acesso.Object, CalendarioPadrao.Criar(), new RelogioFixo(Hoje));

        _acesso.Setup(x => x.PodeFazerChamadaAsync(TurmaId, DisciplinaId)).ReturnsAsync(true);
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId)).ReturnsAsync(_turma);
        _disciplinaRepository.Setup(x => x.GetEntityByIdAsync(DisciplinaId))
            .ReturnsAsync(new Disciplina { Id = DisciplinaId, Nome = "Matemática", Ativo = true });
        _turmaRepository.Setup(x => x.GetAlunosAsync(TurmaId)).ReturnsAsync(
        [
            new TurmaAlunoDTO { AlunoId = Ana, Nome = "Ana", Ativo = true },
            new TurmaAlunoDTO { AlunoId = Bia, Nome = "Bia", Ativo = true }
        ]);
        _presencaRepository.Setup(x => x.GetDaAulaAsync(TurmaId, DisciplinaId, It.IsAny<DateOnly>())).ReturnsAsync(() => _registros.ToList());
    }

    private static RegistrarChamadaRequest Chamada(DateOnly data, params (int AlunoId, bool Presente)[] presencas) => new()
    {
        TurmaId = TurmaId,
        DisciplinaId = DisciplinaId,
        Data = data,
        Presencas = presencas.Select(p => new PresencaRequest { AlunoId = p.AlunoId, Presente = p.Presente }).ToList()
    };

    [Fact]
    public async Task Registra_A_Chamada_Do_Dia()
    {
        var resultado = await _service.RegistrarAsync(Chamada(Hoje, (Ana, true), (Bia, false)));

        resultado.Sucesso.Should().BeTrue();
        _presencaRepository.Verify(x => x.Adicionar(It.Is<Presenca>(p => p.AlunoId == Ana && p.Presente && p.Data == Hoje && p.TurmaId == TurmaId)), Times.Once);
        _presencaRepository.Verify(x => x.Adicionar(It.Is<Presenca>(p => p.AlunoId == Bia && !p.Presente)), Times.Once);
        _presencaRepository.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Refazer_A_Chamada_Atualiza_Sem_Duplicar()
    {
        var registroBia = new Presenca { AlunoId = Bia, Presente = false, Data = Hoje };
        _registros.Add(registroBia);

        await _service.RegistrarAsync(Chamada(Hoje, (Bia, true)));

        registroBia.Presente.Should().BeTrue();
        _presencaRepository.Verify(x => x.Adicionar(It.IsAny<Presenca>()), Times.Never);
    }

    [Fact]
    public async Task Nao_Aceita_Data_Futura_Nem_De_Outro_Ano()
    {
        var futura = await _service.RegistrarAsync(Chamada(Hoje.AddDays(1), (Ana, true)));
        futura.Campo.Should().Be(nameof(RegistrarChamadaRequest.Data));
        futura.Mensagem.Should().Contain("futura");

        var outroAno = await _service.RegistrarAsync(Chamada(new DateOnly(2025, 11, 3), (Ana, true)));
        outroAno.Mensagem.Should().Contain("2026");

        _presencaRepository.Verify(x => x.SalvarAsync(), Times.Never);
    }

    [Fact]
    public async Task Nao_Aceita_Aluno_Fora_Da_Turma()
    {
        var resultado = await _service.RegistrarAsync(Chamada(Hoje, (Ana, true), (999, true)));

        resultado.Mensagem.Should().Contain("não estão matriculados");
    }

    [Fact]
    public async Task Professor_Que_Nao_Leciona_Recebe_Nao_Encontrado()
    {
        _acesso.Setup(x => x.PodeFazerChamadaAsync(TurmaId, DisciplinaId)).ReturnsAsync(false);

        (await _service.RegistrarAsync(Chamada(Hoje, (Ana, true)))).Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        (await _service.ObterAsync(TurmaId, DisciplinaId, Hoje)).Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _turmaRepository.Verify(x => x.GetEntityByIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task Obter_Mostra_O_Bimestre_E_Se_Ja_Foi_Registrada()
    {
        _registros.Add(new Presenca { AlunoId = Ana, Presente = true, Data = Hoje });

        var chamada = (await _service.ObterAsync(TurmaId, DisciplinaId, Hoje)).Valor!;

        chamada.Bimestre.Should().Be(2);
        chamada.Registrada.Should().BeTrue();
        chamada.PodeRegistrar.Should().BeTrue();
        chamada.Presentes.Should().Be(1);
        chamada.Alunos.Single(a => a.AlunoId == Bia).Presente.Should().BeNull();
    }

    [Fact]
    public async Task Turma_Inativa_Fica_So_Para_Consulta()
    {
        _turma.Ativo = false;

        (await _service.ObterAsync(TurmaId, DisciplinaId, Hoje)).Valor!.PodeRegistrar.Should().BeFalse();
        (await _service.RegistrarAsync(Chamada(Hoje, (Ana, true)))).Campo.Should().Be(nameof(RegistrarChamadaRequest.TurmaId));
    }
}
