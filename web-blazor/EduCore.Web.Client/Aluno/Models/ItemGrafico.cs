namespace EduCore.Web.Client.Aluno.Models;

/// <summary>Uma barra do gráfico: rótulo, valor (nulo = sem dados), situação e o texto do detalhe.</summary>
public sealed record ItemGrafico(string Rotulo, decimal? Valor, string ValorTexto, NivelDesempenho Nivel, string? Detalhe);

/// <summary>Linha de referência vertical (ex.: mínimo 6, mínimo 75%).</summary>
public sealed record ReferenciaGrafico(decimal Valor, string Rotulo);

/// <summary>Como cada situação aparece: rótulo, faixa e ícone. Cor nunca vai sozinha.</summary>
public static class Situacoes
{
    public static string Rotulo(NivelDesempenho nivel) => nivel switch
    {
        NivelDesempenho.Risco => "Risco",
        NivelDesempenho.Atencao => "Atenção",
        NivelDesempenho.Ok => "Bom",
        _ => "Sem dados"
    };

    public static string Classe(NivelDesempenho nivel) => nivel switch
    {
        NivelDesempenho.Risco => "risco",
        NivelDesempenho.Atencao => "atencao",
        NivelDesempenho.Ok => "ok",
        _ => "sem-dados"
    };
}
