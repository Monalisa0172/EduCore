using EduCore.API.DTOs.Response;

namespace EduCore.API.Services.Academico;

/// <summary>
/// Cálculo do boletim (sem banco, para ser testado caso a caso):
/// - média do bimestre: Σ(nota × peso) ÷ Σ(pesos das avaliações com nota). Avaliação sem nota fica
///   pendente (não vira zero) e deixa a média parcial;
/// - média anual: média dos bimestres que já têm média;
/// - frequência: presenças ÷ aulas registradas;
/// - situação final só depois do fim do 4º bimestre.
/// </summary>
public static class CalculadoraBoletim
{
    public static List<BoletimDisciplinaDTO> Calcular(
        DadosDoBoletim dados, int anoLetivo, CalendarioLetivo calendario, DateOnly hoje)
    {
        var anoEncerrado = calendario.AnoEncerrado(anoLetivo, hoje);

        return dados.Disciplinas
            .OrderBy(d => d.Nome, StringComparer.CurrentCulture)
            .Select(d => CalcularDisciplina(
                d,
                dados.Avaliacoes.Where(a => a.DisciplinaId == d.Id).ToList(),
                dados.Presencas.Where(p => p.DisciplinaId == d.Id).ToList(),
                anoLetivo, anoEncerrado, calendario, hoje))
            .ToList();
    }

    private static BoletimDisciplinaDTO CalcularDisciplina(
        DisciplinaDoBoletim disciplina,
        List<AvaliacaoDoAluno> avaliacoes,
        List<PresencaDoAluno> presencas,
        int anoLetivo,
        bool anoEncerrado,
        CalendarioLetivo calendario,
        DateOnly hoje)
    {
        var bimestres = Enumerable.Range(1, CalendarioLetivo.TotalBimestres)
            .Select(b => CalcularBimestre(
                b,
                avaliacoes.Where(a => a.Bimestre == b).ToList(),
                presencas.Where(p => calendario.BimestreDaData(p.Data) == b).ToList(),
                calendario.BimestreEncerrado(anoLetivo, b, hoje)))
            .ToList();

        var medias = bimestres.Where(b => b.Media.HasValue).Select(b => b.Media!.Value).ToList();
        decimal? mediaAnual = medias.Count > 0 ? Arredondar(medias.Average()) : null;

        var aulas = presencas.Count;
        var faltas = presencas.Count(p => !p.Presente);
        var frequencia = Percentual(aulas - faltas, aulas);

        var situacao = anoEncerrado && mediaAnual.HasValue
            ? CriteriosAprovacao.Aprovado(mediaAnual.Value, frequencia) ? SituacaoFinal.Aprovado : SituacaoFinal.Reprovado
            : SituacaoFinal.EmAndamento;

        return new BoletimDisciplinaDTO
        {
            DisciplinaId = disciplina.Id,
            Nome = disciplina.Nome,
            Bimestres = bimestres,
            MediaAnual = mediaAnual,
            MediaAnualParcial = medias.Count < CalendarioLetivo.TotalBimestres || bimestres.Any(b => b.MediaParcial),
            Frequencia = frequencia,
            Aulas = aulas,
            Faltas = faltas,
            Nivel = CriteriosAprovacao.Nivel(mediaAnual, frequencia),
            Situacao = situacao
        };
    }

    private static BoletimBimestreDTO CalcularBimestre(
        int bimestre, List<AvaliacaoDoAluno> avaliacoes, List<PresencaDoAluno> presencas, bool encerrado)
    {
        var comNota = avaliacoes.Where(a => a.Nota.HasValue).ToList();
        var pesoLancado = comNota.Sum(a => a.Peso);
        var pesoPrevisto = avaliacoes.Sum(a => a.Peso);

        decimal? media = pesoLancado > 0
            ? Arredondar(comNota.Sum(a => a.Nota!.Value * a.Peso) / pesoLancado)
            : null;

        var aulas = presencas.Count;
        var faltas = presencas.Count(p => !p.Presente);
        var frequencia = Percentual(aulas - faltas, aulas);

        return new BoletimBimestreDTO
        {
            Bimestre = bimestre,
            Encerrado = encerrado,
            Media = media,
            MediaParcial = pesoPrevisto < 100 || comNota.Count < avaliacoes.Count,
            PesoPrevisto = pesoPrevisto,
            PesoLancado = pesoLancado,
            Frequencia = frequencia,
            Aulas = aulas,
            Faltas = faltas,
            Nivel = CriteriosAprovacao.Nivel(media, frequencia),
            Avaliacoes = avaliacoes
                .OrderBy(a => a.DataAplicacao)
                .ThenBy(a => a.Nome)
                .Select(a => new BoletimAvaliacaoDTO
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    SubDisciplinaNome = a.SubDisciplinaNome,
                    Peso = a.Peso,
                    DataAplicacao = a.DataAplicacao,
                    Nota = a.Nota
                })
                .ToList()
        };
    }

    private static decimal? Percentual(int parte, int total)
        => total == 0 ? null : Math.Round(parte * 100m / total, 1, MidpointRounding.AwayFromZero);

    private static decimal Arredondar(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
