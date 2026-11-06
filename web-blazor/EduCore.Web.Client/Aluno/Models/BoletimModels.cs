using System.Text.Json.Serialization;

namespace EduCore.Web.Client.Aluno.Models;

/// <summary>
/// Faixa de desempenho calculada pela API (a regra fica lá: o front só exibe).
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
/// Boletim do aluno em um ano letivo (GET api/me/boletim e GET api/alunos/{id}/boletim).
/// </summary>
public sealed record Boletim(
    int AlunoId,
    string AlunoNome,
    int TurmaId,
    string TurmaNome,
    int AnoLetivo,
    IReadOnlyList<int> AnosLetivos,
    int BimestreVigente,
    bool AnoEncerrado,
    Criterios Criterios,
    IReadOnlyList<BoletimDisciplina> Disciplinas);

/// <summary>Limites usados no cálculo, para explicar as cores ("abaixo de 6", "até 85%").</summary>
public sealed record Criterios(decimal MediaMinima, decimal MediaAtencaoAte, decimal FrequenciaMinima, decimal FrequenciaAtencaoAte);

public sealed record BoletimDisciplina(
    int DisciplinaId,
    string Nome,
    IReadOnlyList<BoletimBimestre> Bimestres,
    decimal? MediaAnual,
    bool MediaAnualParcial,
    decimal? Frequencia,
    int Aulas,
    int Faltas,
    NivelDesempenho Nivel,
    SituacaoFinal Situacao)
{
    public BoletimBimestre Bimestre(int numero) => Bimestres.First(b => b.Bimestre == numero);
}

public sealed record BoletimBimestre(
    int Bimestre,
    bool Encerrado,
    decimal? Media,
    bool MediaParcial,
    decimal PesoPrevisto,
    decimal PesoLancado,
    decimal? Frequencia,
    int Aulas,
    int Faltas,
    NivelDesempenho Nivel,
    IReadOnlyList<BoletimAvaliacao> Avaliacoes);

public sealed record BoletimAvaliacao(int Id, string Nome, string? SubDisciplinaNome, decimal Peso, DateTime DataAplicacao, decimal? Nota);

/// <summary>
/// Recorte exibido nos gráficos e na tabela: um bimestre ou o ano (média anual e frequência do ano).
/// </summary>
public sealed record PeriodoBoletim(int? Bimestre)
{
    public static readonly PeriodoBoletim Ano = new((int?)null);

    public bool EhAno => Bimestre is null;

    public string Rotulo => Bimestre is { } b ? $"{b}º bimestre" : "Ano letivo";
}

/// <summary>Média, frequência e nível de uma disciplina no período escolhido.</summary>
public sealed record DesempenhoDisciplina(
    string Disciplina,
    decimal? Media,
    bool MediaParcial,
    NivelDesempenho NivelMedia,
    decimal? Frequencia,
    int Aulas,
    int Faltas,
    NivelDesempenho NivelFrequencia,
    NivelDesempenho Nivel)
{
    public static DesempenhoDisciplina De(BoletimDisciplina d, PeriodoBoletim periodo, Criterios c)
    {
        if (periodo.Bimestre is { } b)
        {
            var bim = d.Bimestre(b);
            return new(d.Nome, bim.Media, bim.MediaParcial, NivelDaMedia(bim.Media, c), bim.Frequencia, bim.Aulas, bim.Faltas, NivelDaFrequencia(bim.Frequencia, c), bim.Nivel);
        }

        return new(d.Nome, d.MediaAnual, d.MediaAnualParcial, NivelDaMedia(d.MediaAnual, c), d.Frequencia, d.Aulas, d.Faltas, NivelDaFrequencia(d.Frequencia, c), d.Nivel);
    }

    // Os limites vem da API (Criterios): o front so classifica cada grafico separadamente.
    private static NivelDesempenho NivelDaMedia(decimal? media, Criterios c) => media switch
    {
        null => NivelDesempenho.SemDados,
        var m when m < c.MediaMinima => NivelDesempenho.Risco,
        var m when m <= c.MediaAtencaoAte => NivelDesempenho.Atencao,
        _ => NivelDesempenho.Ok
    };

    private static NivelDesempenho NivelDaFrequencia(decimal? frequencia, Criterios c) => frequencia switch
    {
        null => NivelDesempenho.SemDados,
        var f when f < c.FrequenciaMinima => NivelDesempenho.Risco,
        var f when f <= c.FrequenciaAtencaoAte => NivelDesempenho.Atencao,
        _ => NivelDesempenho.Ok
    };
}
