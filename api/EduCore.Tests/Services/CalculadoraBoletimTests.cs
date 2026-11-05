using EduCore.API.Services.Academico;
using FluentAssertions;

namespace EduCore.Tests.Services;

public class CalculadoraBoletimTests
{
    private const int Ano = 2026;
    private const int Mat = 1;

    private readonly CalendarioLetivo _calendario = CalendarioPadrao.Criar();
    private static readonly DisciplinaDoBoletim Matematica = new(Mat, "Matemática");

    private static AvaliacaoDoAluno Av(int bimestre, decimal peso, decimal? nota, int id = 0)
        => new(id, Mat, $"Av {id}", null, peso, new DateTime(Ano, 3, 1), bimestre, nota);

    private static IEnumerable<PresencaDoAluno> Aulas(int presentes, int faltas, string inicio = "2026-03-02")
    {
        var dia = DateOnly.Parse(inicio);
        for (var i = 0; i < presentes + faltas; i++)
            yield return new PresencaDoAluno(Mat, dia.AddDays(i), i < presentes);
    }

    private BoletimDisciplinaCalc Calcular(IEnumerable<AvaliacaoDoAluno> avaliacoes, IEnumerable<PresencaDoAluno>? presencas = null, string hoje = "2026-05-10")
    {
        var dados = new DadosDoBoletim([Matematica], avaliacoes.ToList(), (presencas ?? []).ToList());
        return new(CalculadoraBoletim.Calcular(dados, Ano, _calendario, DateOnly.Parse(hoje)).Single());
    }

    private sealed record BoletimDisciplinaCalc(EduCore.API.DTOs.Response.BoletimDisciplinaDTO D)
    {
        public EduCore.API.DTOs.Response.BoletimBimestreDTO B(int n) => D.Bimestres[n - 1];
    }

    [Fact]
    public void Media_Ponderada_Do_Bimestre_Fechado()
    {
        // 8 (peso 40) e 6 (peso 60): (320 + 360) / 100 = 6,8
        var r = Calcular([Av(1, 40, 8), Av(1, 60, 6)]);

        r.B(1).Media.Should().Be(6.8m);
        r.B(1).MediaParcial.Should().BeFalse();
        r.B(1).PesoPrevisto.Should().Be(100);
    }

    [Fact]
    public void Avaliacao_Sem_Nota_Fica_Pendente_E_Nao_Vira_Zero()
    {
        var r = Calcular([Av(1, 40, 9), Av(1, 60, null)]);

        r.B(1).Media.Should().Be(9m, "só a avaliação com nota entra na conta");
        r.B(1).MediaParcial.Should().BeTrue();
        r.B(1).PesoLancado.Should().Be(40);
    }

    [Fact]
    public void Pesos_Abaixo_De_100_Deixam_A_Media_Parcial()
    {
        var r = Calcular([Av(2, 30, 7)]);

        r.B(2).Media.Should().Be(7m);
        r.B(2).MediaParcial.Should().BeTrue();
    }

    [Fact]
    public void Bimestre_Sem_Avaliacao_Nao_Tem_Media()
    {
        var r = Calcular([Av(1, 100, 8)]);

        r.B(3).Media.Should().BeNull();
        r.B(3).Nivel.Should().Be(NivelDesempenho.SemDados);
        r.D.Bimestres.Should().HaveCount(4);
    }

    [Fact]
    public void Media_Anual_E_A_Media_Dos_Bimestres_Com_Media()
    {
        var r = Calcular([Av(1, 100, 8), Av(2, 100, 5)]);

        r.D.MediaAnual.Should().Be(6.5m);
        r.D.MediaAnualParcial.Should().BeTrue();
    }

    [Fact]
    public void Frequencia_Por_Bimestre_E_No_Ano()
    {
        // 1o bimestre: 3 presencas e 1 falta (75%); 3o bimestre: 4 presencas.
        var r = Calcular([], Aulas(3, 1).Concat(Aulas(4, 0, "2026-08-03")));

        r.B(1).Frequencia.Should().Be(75m);
        r.B(1).Faltas.Should().Be(1);
        r.B(3).Frequencia.Should().Be(100m);
        r.D.Frequencia.Should().Be(87.5m);
        r.D.Aulas.Should().Be(8);
    }

    [Theory]
    [InlineData(5.99, NivelDesempenho.Risco)]
    [InlineData(6.00, NivelDesempenho.Atencao)]
    [InlineData(7.00, NivelDesempenho.Atencao)]
    [InlineData(7.01, NivelDesempenho.Ok)]
    public void Faixas_Da_Media(decimal media, NivelDesempenho esperado)
    {
        CriteriosAprovacao.NivelDaMedia(media).Should().Be(esperado);
    }

    [Theory]
    [InlineData(74.9, NivelDesempenho.Risco)]
    [InlineData(75.0, NivelDesempenho.Atencao)]
    [InlineData(85.0, NivelDesempenho.Atencao)]
    [InlineData(85.1, NivelDesempenho.Ok)]
    public void Faixas_Da_Frequencia(decimal frequencia, NivelDesempenho esperado)
    {
        CriteriosAprovacao.NivelDaFrequencia(frequencia).Should().Be(esperado);
    }

    [Fact]
    public void Nivel_E_O_Pior_Entre_Media_E_Frequencia()
    {
        CriteriosAprovacao.Nivel(9.5m, 80m).Should().Be(NivelDesempenho.Atencao);
        CriteriosAprovacao.Nivel(5m, 100m).Should().Be(NivelDesempenho.Risco);
        CriteriosAprovacao.Nivel(null, 90m).Should().Be(NivelDesempenho.Ok);
        CriteriosAprovacao.Nivel(null, null).Should().Be(NivelDesempenho.SemDados);
    }

    [Fact]
    public void Situacao_Final_So_Depois_Do_Fim_Do_Ano()
    {
        var avaliacoes = Enumerable.Range(1, 4).Select(b => Av(b, 100, 8)).ToList();

        Calcular(avaliacoes, hoje: "2026-11-01").D.Situacao.Should().Be(SituacaoFinal.EmAndamento);
        Calcular(avaliacoes, hoje: "2026-12-20").D.Situacao.Should().Be(SituacaoFinal.Aprovado);
    }

    [Fact]
    public void Reprova_Por_Frequencia_Mesmo_Com_Media_Boa()
    {
        var avaliacoes = Enumerable.Range(1, 4).Select(b => Av(b, 100, 9)).ToList();

        var r = Calcular(avaliacoes, Aulas(7, 3), hoje: "2027-01-10");

        r.D.Frequencia.Should().Be(70m);
        r.D.Situacao.Should().Be(SituacaoFinal.Reprovado);
        r.D.Nivel.Should().Be(NivelDesempenho.Risco);
    }

    [Fact]
    public void Reprova_Por_Media_Anual_Abaixo_De_6()
    {
        var r = Calcular([Av(1, 100, 7), Av(2, 100, 5), Av(3, 100, 5), Av(4, 100, 6)], hoje: "2027-01-10");

        r.D.MediaAnual.Should().Be(5.75m);
        r.D.Situacao.Should().Be(SituacaoFinal.Reprovado);
    }
}
