using System.Text.Json.Serialization;
using EduCore.Web.Client.Aluno.Models;

namespace EduCore.Web.Client.Professor.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SituacaoProjecao
{
    SemDados,
    Garantida,
    Possivel,
    Impossivel
}

/// <summary>
/// Acompanhamento do professor pela coordenação (GET api/usuarios/{id}/acompanhamento).
/// </summary>
public sealed record Acompanhamento(int ProfessorId, IReadOnlyList<AcompanhamentoItem> Itens);

public sealed record AcompanhamentoItem(
    int TurmaId,
    string TurmaNome,
    int AnoLetivo,
    int DisciplinaId,
    string DisciplinaNome,
    IReadOnlyList<string> SubDisciplinas,
    int BimestreVigente,
    bool AnoEncerrado,
    int AlunosAtivos,
    decimal PesoTotalBimestre,
    IReadOnlyList<AvaliacaoPendente> AvaliacoesPendentes,
    DateOnly? UltimaChamada,
    int? DiasSemChamada,
    int AulasNoBimestre,
    decimal? MediaTurma,
    decimal? FrequenciaTurma,
    int AlunosEmRisco,
    int AlunosEmAtencao,
    IReadOnlyList<AlunoEmAlerta> AlunosEmAlerta)
{
    /// <summary>Uma semana sem chamada já merece atenção.</summary>
    public const int DiasSemChamadaAlerta = 7;

    public string Titulo => SubDisciplinas.Count == 0 ? DisciplinaNome : $"{DisciplinaNome} · {string.Join(", ", SubDisciplinas)}";

    public bool PesosIncompletos => PesoTotalBimestre < 100;

    public bool ChamadaAtrasada => DiasSemChamada is null or > DiasSemChamadaAlerta;

    public int Pendencias => (PesosIncompletos ? 1 : 0) + AvaliacoesPendentes.Count + (ChamadaAtrasada ? 1 : 0);

    /// <summary>A projeção (reforço e reposição) vale no último bimestre, antes de o ano fechar.</summary>
    public bool ProjecaoDisponivel => BimestreVigente == 4 && !AnoEncerrado;
}

public sealed record AvaliacaoPendente(int Id, string Nome, DateTime DataAplicacao, int Bimestre, int NotasLancadas);

public sealed record AlunoEmAlerta(
    int AlunoId,
    int UsuarioId,
    string Nome,
    decimal? MediaAnual,
    bool MediaAnualParcial,
    decimal? Frequencia,
    int Aulas,
    int Faltas,
    NivelDesempenho NivelMedia,
    NivelDesempenho NivelFrequencia,
    NivelDesempenho Nivel,
    SituacaoProjecao ProjecaoNota,
    decimal? NotaNecessaria,
    int AulasParaRepor);
