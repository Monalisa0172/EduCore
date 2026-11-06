using System.Text.Json.Serialization;

namespace EduCore.API.Services.Academico;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SituacaoProjecao
{
    /// <summary>Faltam médias de bimestres anteriores: não dá para projetar.</summary>
    SemDados,

    /// <summary>A média anual já fica acima do mínimo com qualquer nota no 4º bimestre.</summary>
    Garantida,

    /// <summary>Precisa de uma nota alcançável (até 10) no 4º bimestre.</summary>
    Possivel,

    /// <summary>Nem com 10 no 4º bimestre chega ao mínimo: caso de recuperação.</summary>
    Impossivel
}

/// <summary>
/// Contas para orientar reforço (nota) e reposição (frequência) no fim do ano.
/// Média anual = média dos 4 bimestres; frequência = presenças ÷ aulas com chamada.
/// </summary>
public static class ProjecaoAprovacao
{
    /// <summary>
    /// Nota mínima no 4º bimestre para a média anual chegar ao mínimo, a partir das médias do 1º ao 3º.
    /// (m1 + m2 + m3 + x) ÷ 4 ≥ mínimo  ⇒  x ≥ 4 × mínimo − (m1 + m2 + m3).
    /// </summary>
    public static (SituacaoProjecao Situacao, decimal? Nota) NotaNecessaria(IReadOnlyList<decimal?> mediasAnteriores, decimal minimo)
    {
        if (mediasAnteriores.Count != 3 || mediasAnteriores.Any(m => m is null))
            return (SituacaoProjecao.SemDados, null);

        var necessaria = CalendarioLetivo.TotalBimestres * minimo - mediasAnteriores.Sum(m => m!.Value);

        return necessaria switch
        {
            <= 0 => (SituacaoProjecao.Garantida, 0),
            > 10 => (SituacaoProjecao.Impossivel, Arredondar(necessaria)),
            _ => (SituacaoProjecao.Possivel, Arredondar(necessaria))
        };
    }

    /// <summary>
    /// Aulas com presença necessárias (sem novas faltas) para a frequência voltar ao mínimo:
    /// (presenças + k) ÷ (aulas + k) ≥ mínimo  ⇒  k ≥ (mínimo × aulas − presenças) ÷ (1 − mínimo).
    /// Zero quando já está no mínimo.
    /// </summary>
    public static int AulasParaRepor(int aulas, int faltas, decimal minimoPercentual)
    {
        if (aulas == 0)
            return 0;

        var minimo = minimoPercentual / 100m;
        var presencas = aulas - faltas;
        var falta = minimo * aulas - presencas;

        return falta <= 0 ? 0 : (int)Math.Ceiling(falta / (1 - minimo));
    }

    // Arredonda para cima na 1a casa: a nota exibida e a que realmente basta.
    private static decimal Arredondar(decimal valor) => Math.Ceiling(valor * 10) / 10;
}
