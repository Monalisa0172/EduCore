using EduCore.API.Services.Academico;
using Microsoft.Extensions.Options;

namespace EduCore.Tests.Services;

/// <summary>Relógio parado numa data, para testar regras que dependem de "hoje".</summary>
internal sealed class RelogioFixo(DateOnly hoje) : TimeProvider
{
    public override TimeZoneInfo LocalTimeZone => TimeZoneInfo.Utc;

    public override DateTimeOffset GetUtcNow() => new(hoje.ToDateTime(new TimeOnly(10, 0)), TimeSpan.Zero);
}

internal static class CalendarioPadrao
{
    /// <summary>Calendário com as datas padrão: 1º 01/02-15/04, 2º 16/04-30/06, 3º 01/08-30/09, 4º 01/10-15/12.</summary>
    public static CalendarioLetivo Criar() => new(Options.Create(new CalendarioLetivoOptions()));
}
