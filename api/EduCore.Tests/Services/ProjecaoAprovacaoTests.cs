using EduCore.API.Services.Academico;
using FluentAssertions;

namespace EduCore.Tests.Services;

public class ProjecaoAprovacaoTests
{
    [Fact]
    public void Nota_Necessaria_No_Quarto_Bimestre()
    {
        // (5 + 5,5 + 5) = 15,5 -> precisa de 24 - 15,5 = 8,5
        var (situacao, nota) = ProjecaoAprovacao.NotaNecessaria([5m, 5.5m, 5m], 6m);

        situacao.Should().Be(SituacaoProjecao.Possivel);
        nota.Should().Be(8.5m);
    }

    [Fact]
    public void Arredonda_Para_Cima_Para_A_Nota_Realmente_Bastar()
    {
        // 24 - (5,43 + 5,5 + 5) = 8,07 -> 8,1
        ProjecaoAprovacao.NotaNecessaria([5.43m, 5.5m, 5m], 6m).Nota.Should().Be(8.1m);
    }

    [Fact]
    public void Garantida_Quando_Ja_Passa_Com_Zero()
    {
        ProjecaoAprovacao.NotaNecessaria([9m, 8m, 8m], 6m).Situacao.Should().Be(SituacaoProjecao.Garantida);
    }

    [Fact]
    public void Impossivel_Quando_Nem_Dez_Basta()
    {
        // 24 - (3 + 4 + 4) = 13
        var (situacao, nota) = ProjecaoAprovacao.NotaNecessaria([3m, 4m, 4m], 6m);

        situacao.Should().Be(SituacaoProjecao.Impossivel);
        nota.Should().Be(13m);
    }

    [Fact]
    public void Sem_Dados_Quando_Falta_Media_De_Bimestre()
    {
        ProjecaoAprovacao.NotaNecessaria([5m, null, 6m], 6m).Situacao.Should().Be(SituacaoProjecao.SemDados);
    }

    [Theory]
    [InlineData(20, 7, 75, 8)]  // 13/20 = 65%: (13 + 8) / 28 = 75%
    [InlineData(20, 5, 75, 0)]  // 75%: ja no minimo
    [InlineData(10, 4, 75, 6)]  // 6/10 -> (6 + 6) / 16 = 75%
    [InlineData(0, 0, 75, 0)]
    public void Aulas_Para_Repor(int aulas, int faltas, decimal minimo, int esperado)
    {
        ProjecaoAprovacao.AulasParaRepor(aulas, faltas, minimo).Should().Be(esperado);
    }
}
