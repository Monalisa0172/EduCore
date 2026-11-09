using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Academico;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class AcompanhamentoServiceTests
{
    private const int UsuarioId = 20;
    private const int ProfessorId = 5;
    private const int TurmaId = 12;
    private const int PortuguesId = 3;
    private const int GramaticaId = 8;
    private const int LiteraturaId = 9;
    private const int Ana = 1;
    private const int Bia = 2;

    private readonly Mock<IAcompanhamentoRepository> _repositorio = new();
    private readonly Mock<IAtribuicaoRepository> _atribuicoes = new();
    private readonly Mock<ITurmaRepository> _turmas = new();
    private readonly AcompanhamentoService _service;

    // Hoje: 06/11/2026 (4o bimestre).
    public AcompanhamentoServiceTests()
    {
        _service = new AcompanhamentoService(_repositorio.Object, _atribuicoes.Object, _turmas.Object,
            CalendarioPadrao.Criar(), new RelogioFixo(new DateOnly(2026, 11, 6)));

        _repositorio.Setup(x => x.GetProfessorIdPorUsuarioAsync(UsuarioId)).ReturnsAsync(ProfessorId);
        _atribuicoes.Setup(x => x.GetAtivasDoProfessorAsync(ProfessorId, null, null)).ReturnsAsync(
        [
            new AtribuicaoResponseDTO { TurmaId = TurmaId, TurmaNome = "9º Ano A", AnoLetivo = 2026, TurmaAtiva = true, DisciplinaId = PortuguesId, DisciplinaNome = "Português", SubDisciplinaId = GramaticaId, SubDisciplinaNome = "Gramática" }
        ]);
        _turmas.Setup(x => x.GetAlunosAsync(TurmaId)).ReturnsAsync(
        [
            new TurmaAlunoDTO { AlunoId = Ana, UsuarioId = 101, Nome = "Ana", Ativo = true },
            new TurmaAlunoDTO { AlunoId = Bia, UsuarioId = 102, Nome = "Bia", Ativo = true }
        ]);

        // Ana: 5 nos tres primeiros bimestres (precisa de 9 no 4o). Bia: 8 (ok).
        var avaliacoes = Enumerable.Range(1, 3)
            .Select(b => Avaliacao(10 + b, b, GramaticaId, new DateTime(2026, b * 3, 10), (Ana, 5m), (Bia, 8m)))
            .Concat(
            [
                // 4o bimestre: prova de gramatica aplicada so com a nota da Bia (pendente);
                // a de literatura e de outro professor (nao conta como pendencia dele).
                Avaliacao(20, 4, GramaticaId, new DateTime(2026, 10, 27), (Bia, 7m)),
                Avaliacao(21, 4, LiteraturaId, new DateTime(2026, 10, 28)),
                // Futura: ainda nao e pendencia.
                Avaliacao(22, 4, GramaticaId, new DateTime(2026, 12, 4))
            ])
            .ToList();
        _repositorio.Setup(x => x.GetAvaliacoesAsync(TurmaId, PortuguesId)).ReturnsAsync(avaliacoes);

        // Chamada: Ana com 3 faltas em 10 aulas (70%); ultima aula em 28/10.
        var presencas = Enumerable.Range(0, 10)
            .SelectMany(i => new[]
            {
                new PresencaDaTurma(Ana, new DateOnly(2026, 10, 19).AddDays(i), i >= 3),
                new PresencaDaTurma(Bia, new DateOnly(2026, 10, 19).AddDays(i), true)
            })
            .ToList();
        _repositorio.Setup(x => x.GetPresencasAsync(TurmaId, PortuguesId)).ReturnsAsync(presencas);
    }

    private static AvaliacaoDaTurma Avaliacao(int id, int bimestre, int subId, DateTime data, params (int Aluno, decimal Nota)[] notas)
        => new(id, $"Av {id}", subId, subId == GramaticaId ? "Gramática" : "Literatura", 50, data, bimestre,
            notas.Select(n => new NotaDoAluno(n.Aluno, n.Nota)).ToList());

    private async Task<AcompanhamentoItemDTO> ItemAsync() => (await _service.ObterPorUsuarioAsync(UsuarioId)).Valor!.Itens.Single();

    [Fact]
    public async Task Pendencias_So_Do_Que_O_Professor_Leciona_E_Ja_Aplicado()
    {
        var item = await ItemAsync();

        item.SubDisciplinas.Should().Equal("Gramática");
        item.AvaliacoesPendentes.Should().ContainSingle().Which.Should().BeEquivalentTo(new { Id = 20, NotasLancadas = 1 });
        item.PesoTotalBimestre.Should().Be(150, "o peso conta a disciplina inteira, todos os professores");
        item.UltimaChamada.Should().Be(new DateOnly(2026, 10, 28));
        item.DiasSemChamada.Should().Be(9);
    }

    [Fact]
    public async Task Projecao_De_Reforco_E_Reposicao()
    {
        var item = await ItemAsync();

        item.AlunosEmRisco.Should().Be(1);
        var ana = item.AlunosEmAlerta.Should().ContainSingle().Subject;
        ana.Nome.Should().Be("Ana");
        ana.ProjecaoNota.Should().Be(SituacaoProjecao.Possivel);
        ana.NotaNecessaria.Should().Be(9m);
        ana.Frequencia.Should().Be(70m);
        ana.AulasParaRepor.Should().Be(2);
    }

    [Fact]
    public async Task Media_Da_Turma_E_A_Media_Das_Medias_Anuais()
    {
        var item = await ItemAsync();

        // Ana: (5+5+5)/3 = 5 (sem nota no 4o); Bia: (8+8+8+7)/4 = 7,75 -> (5 + 7,75) / 2
        item.MediaTurma.Should().Be(6.38m);
        item.FrequenciaTurma.Should().Be(85m);
    }

    [Fact]
    public async Task Usuario_Que_Nao_E_Professor_Recebe_Nao_Encontrado()
    {
        (await _service.ObterPorUsuarioAsync(999)).Falha.Should().Be(FalhaOperacao.NaoEncontrado);
    }
}
