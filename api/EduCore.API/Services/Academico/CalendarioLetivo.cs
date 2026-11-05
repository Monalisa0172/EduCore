using System.Globalization;
using Microsoft.Extensions.Options;

namespace EduCore.API.Services.Academico;

/// <summary>
/// Datas dos bimestres (mês-dia, valem para qualquer ano letivo). Configurável em appsettings
/// ("CalendarioLetivo"); se a escola precisar de datas por ano, vira cadastro no admin.
/// </summary>
public class CalendarioLetivoOptions
{
    public const string Secao = "CalendarioLetivo";

    public List<PeriodoBimestreOptions> Bimestres { get; set; } = [];
}

public class PeriodoBimestreOptions
{
    public int Numero { get; set; }

    /// <summary>Início no formato MM-dd (ex.: 02-01).</summary>
    public string Inicio { get; set; } = string.Empty;

    /// <summary>Fim no formato MM-dd (ex.: 04-15).</summary>
    public string Fim { get; set; } = string.Empty;
}

/// <summary>
/// Em que bimestre cai uma data e qual é o bimestre vigente. Dias de recesso entre bimestres
/// contam para o bimestre que acabou de terminar.
/// </summary>
public class CalendarioLetivo
{
    public const int TotalBimestres = 4;

    private static readonly PeriodoBimestreOptions[] Padrao =
    [
        new() { Numero = 1, Inicio = "02-01", Fim = "04-15" },
        new() { Numero = 2, Inicio = "04-16", Fim = "06-30" },
        new() { Numero = 3, Inicio = "08-01", Fim = "09-30" },
        new() { Numero = 4, Inicio = "10-01", Fim = "12-15" }
    ];

    private readonly (int Numero, int InicioMes, int InicioDia, int FimMes, int FimDia)[] _periodos;

    public CalendarioLetivo(IOptions<CalendarioLetivoOptions> options)
    {
        var configurados = options.Value.Bimestres;
        var fonte = configurados.Count == TotalBimestres ? configurados : [.. Padrao];

        _periodos = fonte
            .OrderBy(b => b.Numero)
            .Select(b =>
            {
                var inicio = LerMesDia(b.Inicio, b.Numero);
                var fim = LerMesDia(b.Fim, b.Numero);
                return (b.Numero, inicio.Mes, inicio.Dia, fim.Mes, fim.Dia);
            })
            .ToArray();

        if (!_periodos.Select(p => p.Numero).SequenceEqual(Enumerable.Range(1, TotalBimestres)))
            throw new InvalidOperationException("CalendarioLetivo: configure os bimestres 1, 2, 3 e 4.");
    }

    public DateOnly Inicio(int anoLetivo, int bimestre)
    {
        var p = _periodos[bimestre - 1];
        return new DateOnly(anoLetivo, p.InicioMes, p.InicioDia);
    }

    public DateOnly Fim(int anoLetivo, int bimestre)
    {
        var p = _periodos[bimestre - 1];
        return new DateOnly(anoLetivo, p.FimMes, p.FimDia);
    }

    /// <summary>Bimestre da data: o último que já começou (antes do 1º, conta como 1º).</summary>
    public int BimestreDaData(DateOnly data)
    {
        for (var b = TotalBimestres; b >= 1; b--)
        {
            if (data >= Inicio(data.Year, b))
                return b;
        }

        return 1;
    }

    /// <summary>Bimestre a exibir por padrão: o de hoje no ano corrente; 4º para anos passados; 1º para futuros.</summary>
    public int BimestreVigente(int anoLetivo, DateOnly hoje)
        => hoje.Year < anoLetivo ? 1
            : hoje.Year > anoLetivo ? TotalBimestres
            : BimestreDaData(hoje);

    public bool BimestreEncerrado(int anoLetivo, int bimestre, DateOnly hoje) => hoje > Fim(anoLetivo, bimestre);

    public bool AnoEncerrado(int anoLetivo, DateOnly hoje) => BimestreEncerrado(anoLetivo, TotalBimestres, hoje);

    private static (int Mes, int Dia) LerMesDia(string valor, int bimestre)
    {
        if (!DateTime.TryParseExact($"2000-{valor}", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
            throw new InvalidOperationException($"CalendarioLetivo: data inválida \"{valor}\" no {bimestre}º bimestre (use MM-dd).");

        return (data.Month, data.Day);
    }
}
