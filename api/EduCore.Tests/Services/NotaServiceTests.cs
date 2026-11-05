using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Acesso;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class NotaServiceTests
{
    private const int AvaliacaoId = 7;
    private const int TurmaId = 1;
    private const int Ana = 11;
    private const int Bia = 12;
    private const int Caio = 13;

    private readonly Mock<IAvaliacaoRepository> _avaliacaoRepository = new();
    private readonly Mock<INotaRepository> _notaRepository = new();
    private readonly Mock<ITurmaRepository> _turmaRepository = new();
    private readonly Mock<IAcessoAcademico> _acesso = new();

    private readonly Avaliacao _avaliacao = new() { Id = AvaliacaoId, TurmaId = TurmaId, DisciplinaId = 10, Ativo = true };
    private readonly Turma _turma = new() { Id = TurmaId, Ativo = true };
    private readonly List<Nota> _notas = [];

    private readonly NotaService _service;

    public NotaServiceTests()
    {
        _service = new NotaService(_avaliacaoRepository.Object, _notaRepository.Object, _turmaRepository.Object, _acesso.Object);

        _acesso.Setup(x => x.PodeGerenciarAvaliacaoAsync(TurmaId, 10, null)).ReturnsAsync(true);
        _avaliacaoRepository.Setup(x => x.GetEntityByIdAsync(AvaliacaoId)).ReturnsAsync(_avaliacao);
        _avaliacaoRepository.Setup(x => x.GetByIdAsync(AvaliacaoId))
            .ReturnsAsync(new AvaliacaoResponseDTO { Id = AvaliacaoId, TurmaId = TurmaId, DisciplinaId = 10, Ativo = true });
        _turmaRepository.Setup(x => x.GetEntityByIdAsync(TurmaId)).ReturnsAsync(_turma);
        _turmaRepository.Setup(x => x.GetAlunosAsync(TurmaId)).ReturnsAsync(
        [
            new TurmaAlunoDTO { AlunoId = Ana, Nome = "Ana", Ativo = true },
            new TurmaAlunoDTO { AlunoId = Bia, Nome = "Bia", Ativo = true },
            new TurmaAlunoDTO { AlunoId = Caio, Nome = "Caio", Ativo = false }
        ]);
        _notaRepository.Setup(x => x.GetDaAvaliacaoAsync(AvaliacaoId)).ReturnsAsync(() => _notas.ToList());
    }

    private static LancarNotasRequest Lote(params (int AlunoId, decimal? Valor)[] notas)
        => new() { Notas = notas.Select(n => new NotaLancamentoRequest { AlunoId = n.AlunoId, Valor = n.Valor }).ToList() };

    [Fact]
    public async Task Obter_Traz_Todos_Os_Alunos_Com_A_Nota_De_Cada_Um()
    {
        _notas.Add(new Nota { AlunoId = Ana, Valor = 8.5m });

        var resultado = await _service.ObterAsync(AvaliacaoId);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Alunos.Should().HaveCount(3);
        resultado.Valor.Alunos.Single(a => a.AlunoId == Ana).Nota.Should().Be(8.5m);
        resultado.Valor.Alunos.Single(a => a.AlunoId == Bia).Nota.Should().BeNull();
        resultado.Valor.Lancadas.Should().Be(1);
        resultado.Valor.PodeLancar.Should().BeTrue();
    }

    [Fact]
    public async Task Lote_Adiciona_Atualiza_E_Remove_Num_Unico_Salvamento()
    {
        var notaAna = new Nota { AlunoId = Ana, Valor = 5m };
        var notaBia = new Nota { AlunoId = Bia, Valor = 7m };
        _notas.AddRange([notaAna, notaBia]);
        // Caio inativo sem nota: enviar vazio nao muda nada e e aceito.
        var resultado = await _service.LancarAsync(AvaliacaoId, Lote((Ana, 9.25m), (Bia, null), (Caio, null)));

        resultado.Sucesso.Should().BeTrue();
        notaAna.Valor.Should().Be(9.25m);
        _notaRepository.Verify(x => x.Remover(notaBia), Times.Once);
        _notaRepository.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Lote_Cria_Nota_Nova()
    {
        await _service.LancarAsync(AvaliacaoId, Lote((Bia, 10m)));

        _notaRepository.Verify(x => x.Adicionar(It.Is<Nota>(n => n.AlunoId == Bia && n.Valor == 10m && n.AvaliacaoId == AvaliacaoId)), Times.Once);
    }

    [Fact]
    public async Task Recusa_Aluno_Fora_Da_Turma()
    {
        var resultado = await _service.LancarAsync(AvaliacaoId, Lote((Ana, 8m), (999, 7m)));

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Mensagem.Should().Contain("não estão matriculados");
        _notaRepository.Verify(x => x.SalvarAsync(), Times.Never);
    }

    [Fact]
    public async Task Recusa_Nota_Nova_Para_Aluno_Inativo()
    {
        var resultado = await _service.LancarAsync(AvaliacaoId, Lote((Caio, 6m)));

        resultado.Mensagem.Should().Contain("Caio");
        _notaRepository.Verify(x => x.SalvarAsync(), Times.Never);
    }

    [Fact]
    public async Task Recusa_Aluno_Repetido_E_Mais_De_Duas_Casas()
    {
        (await _service.LancarAsync(AvaliacaoId, Lote((Ana, 8m), (Ana, 9m)))).Mensagem.Should().Contain("mais de uma vez");
        (await _service.LancarAsync(AvaliacaoId, Lote((Ana, 8.333m)))).Mensagem.Should().Contain("2 casas");
    }

    [Fact]
    public async Task Avaliacao_Ou_Turma_Inativa_Fica_So_Para_Consulta()
    {
        _avaliacao.Ativo = false;
        (await _service.LancarAsync(AvaliacaoId, Lote((Ana, 8m)))).Mensagem.Should().Contain("avaliação está inativa");

        _avaliacao.Ativo = true;
        _turma.Ativo = false;
        (await _service.LancarAsync(AvaliacaoId, Lote((Ana, 8m)))).Mensagem.Should().Contain("turma está inativa");
    }

    [Fact]
    public async Task Professor_Sem_Acesso_Recebe_Nao_Encontrado()
    {
        _acesso.Setup(x => x.PodeGerenciarAvaliacaoAsync(TurmaId, 10, null)).ReturnsAsync(false);

        (await _service.ObterAsync(AvaliacaoId)).Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        (await _service.LancarAsync(AvaliacaoId, Lote((Ana, 8m)))).Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        _notaRepository.Verify(x => x.SalvarAsync(), Times.Never);
    }
}
