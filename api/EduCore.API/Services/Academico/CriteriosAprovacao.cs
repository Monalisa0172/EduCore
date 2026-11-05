using System.Text.Json.Serialization;

namespace EduCore.API.Services.Academico;

/// <summary>
/// Faixa de desempenho para os fronts colorirem (Risco = vermelho, Atencao = âmbar, Ok = verde suave).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NivelDesempenho
{
    SemDados,
    Ok,
    Atencao,
    Risco
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SituacaoFinal
{
    EmAndamento,
    Aprovado,
    Reprovado
}

/// <summary>
/// Critérios de aprovação (anual, por disciplina) e faixas de alerta. A regra fica só aqui:
/// a API devolve o nível já calculado e os fronts (Blazor e React) apenas exibem.
/// </summary>
public static class CriteriosAprovacao
{
    /// <summary>Média anual mínima (média dos 4 bimestres).</summary>
    public const decimal MediaMinima = 6m;

    /// <summary>Até esta média (inclusive) o aluno está perto do limite.</summary>
    public const decimal MediaAtencaoAte = 7m;

    /// <summary>Frequência mínima, em %.</summary>
    public const decimal FrequenciaMinima = 75m;

    /// <summary>Até esta frequência (inclusive), em %, acende o alerta.</summary>
    public const decimal FrequenciaAtencaoAte = 85m;

    public static NivelDesempenho NivelDaMedia(decimal? media)
        => media switch
        {
            null => NivelDesempenho.SemDados,
            < MediaMinima => NivelDesempenho.Risco,
            <= MediaAtencaoAte => NivelDesempenho.Atencao,
            _ => NivelDesempenho.Ok
        };

    public static NivelDesempenho NivelDaFrequencia(decimal? frequencia)
        => frequencia switch
        {
            null => NivelDesempenho.SemDados,
            < FrequenciaMinima => NivelDesempenho.Risco,
            <= FrequenciaAtencaoAte => NivelDesempenho.Atencao,
            _ => NivelDesempenho.Ok
        };

    /// <summary>O pior entre média e frequência (o que tiver dado).</summary>
    public static NivelDesempenho Nivel(decimal? media, decimal? frequencia)
        => (NivelDesempenho)Math.Max((int)NivelDaMedia(media), (int)NivelDaFrequencia(frequencia));

    public static bool Aprovado(decimal media, decimal? frequencia)
        => media >= MediaMinima && (frequencia is null || frequencia >= FrequenciaMinima);
}
