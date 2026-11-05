using EduCore.API.Services.Academico;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace EduCore.Tests.Services;

public class CalendarioLetivoTests
{
    private readonly CalendarioLetivo _calendario = CalendarioPadrao.Criar();

    [Theory]
    [InlineData("2026-01-20", 1)] // antes do ano letivo comecar: conta como 1o
    [InlineData("2026-02-01", 1)]
    [InlineData("2026-04-15", 1)]
    [InlineData("2026-04-16", 2)]
    [InlineData("2026-07-15", 2)] // recesso de julho: conta para o bimestre que acabou
    [InlineData("2026-08-01", 3)]
    [InlineData("2026-10-01", 4)]
    [InlineData("2026-12-28", 4)]
    public void Bimestre_Da_Data(string data, int esperado)
    {
        _calendario.BimestreDaData(DateOnly.Parse(data)).Should().Be(esperado);
    }

    [Fact]
    public void Bimestre_Vigente_Considera_O_Ano_Letivo()
    {
        var hoje = new DateOnly(2026, 5, 10);

        _calendario.BimestreVigente(2026, hoje).Should().Be(2);
        _calendario.BimestreVigente(2025, hoje).Should().Be(4, "ano passado: mostra o fechamento");
        _calendario.BimestreVigente(2027, hoje).Should().Be(1, "ano que nem começou");
    }

    [Fact]
    public void Ano_So_Encerra_Depois_Do_Fim_Do_Quarto_Bimestre()
    {
        _calendario.AnoEncerrado(2026, new DateOnly(2026, 12, 15)).Should().BeFalse();
        _calendario.AnoEncerrado(2026, new DateOnly(2026, 12, 16)).Should().BeTrue();
        _calendario.BimestreEncerrado(2026, 1, new DateOnly(2026, 4, 16)).Should().BeTrue();
    }

    [Fact]
    public void Usa_As_Datas_Configuradas()
    {
        var calendario = new CalendarioLetivo(Options.Create(new CalendarioLetivoOptions
        {
            Bimestres =
            [
                new() { Numero = 1, Inicio = "03-01", Fim = "04-30" },
                new() { Numero = 2, Inicio = "05-01", Fim = "07-10" },
                new() { Numero = 3, Inicio = "08-01", Fim = "09-30" },
                new() { Numero = 4, Inicio = "10-01", Fim = "12-10" }
            ]
        }));

        calendario.BimestreDaData(new DateOnly(2026, 4, 20)).Should().Be(1);
        calendario.Fim(2026, 4).Should().Be(new DateOnly(2026, 12, 10));
    }

    [Fact]
    public void Data_Invalida_Na_Configuracao_Falha_Na_Subida()
    {
        var criar = () => new CalendarioLetivo(Options.Create(new CalendarioLetivoOptions
        {
            Bimestres =
            [
                new() { Numero = 1, Inicio = "02-30", Fim = "04-30" },
                new() { Numero = 2, Inicio = "05-01", Fim = "07-10" },
                new() { Numero = 3, Inicio = "08-01", Fim = "09-30" },
                new() { Numero = 4, Inicio = "10-01", Fim = "12-10" }
            ]
        }));

        criar.Should().Throw<InvalidOperationException>().WithMessage("*1º bimestre*");
    }
}
