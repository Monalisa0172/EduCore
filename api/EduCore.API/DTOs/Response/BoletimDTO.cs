using EduCore.API.Services.Academico;

namespace EduCore.API.DTOs.Response;

/// <summary>
/// Boletim do aluno em um ano letivo. Traz os 4 bimestres de cada disciplina; o front abre no
/// <see cref="BimestreVigente"/> e permite navegar pelos anteriores.
/// </summary>
public class BoletimDTO
{
    public int AlunoId { get; set; }

    public string AlunoNome { get; set; } = string.Empty;

    public int TurmaId { get; set; }

    public string TurmaNome { get; set; } = string.Empty;

    public int AnoLetivo { get; set; }

    /// <summary>Anos letivos em que o aluno tem matrícula (para trocar de ano no front).</summary>
    public List<int> AnosLetivos { get; set; } = [];

    public int BimestreVigente { get; set; }

    /// <summary>Verdadeiro depois do fim do 4º bimestre: a situação final passa a valer.</summary>
    public bool AnoEncerrado { get; set; }

    public CriteriosDTO Criterios { get; set; } = new();

    public List<BoletimDisciplinaDTO> Disciplinas { get; set; } = [];
}

/// <summary>
/// Os limites usados no cálculo, para o front explicar as cores ("abaixo de 6", "até 85%").
/// </summary>
public class CriteriosDTO
{
    public decimal MediaMinima { get; set; } = CriteriosAprovacao.MediaMinima;

    public decimal MediaAtencaoAte { get; set; } = CriteriosAprovacao.MediaAtencaoAte;

    public decimal FrequenciaMinima { get; set; } = CriteriosAprovacao.FrequenciaMinima;

    public decimal FrequenciaAtencaoAte { get; set; } = CriteriosAprovacao.FrequenciaAtencaoAte;
}

public class BoletimDisciplinaDTO
{
    public int DisciplinaId { get; set; }

    public string Nome { get; set; } = string.Empty;

    /// <summary>Sempre os 4 bimestres, em ordem.</summary>
    public List<BoletimBimestreDTO> Bimestres { get; set; } = [];

    /// <summary>Média dos bimestres que já têm média.</summary>
    public decimal? MediaAnual { get; set; }

    /// <summary>Verdadeiro enquanto faltar bimestre, avaliação ou nota.</summary>
    public bool MediaAnualParcial { get; set; }

    /// <summary>Frequência no ano, em %.</summary>
    public decimal? Frequencia { get; set; }

    public int Aulas { get; set; }

    public int Faltas { get; set; }

    /// <summary>Pior entre média anual e frequência (até agora).</summary>
    public NivelDesempenho Nivel { get; set; }

    public SituacaoFinal Situacao { get; set; }
}

public class BoletimBimestreDTO
{
    public int Bimestre { get; set; }

    public bool Encerrado { get; set; }

    /// <summary>Média ponderada das avaliações com nota: Σ(nota × peso) ÷ Σ(pesos com nota).</summary>
    public decimal? Media { get; set; }

    /// <summary>Verdadeiro se os pesos ainda não somam 100 ou há avaliação sem nota.</summary>
    public bool MediaParcial { get; set; }

    /// <summary>Soma dos pesos das avaliações do bimestre (fecha em 100).</summary>
    public decimal PesoPrevisto { get; set; }

    /// <summary>Soma dos pesos das avaliações que já têm nota.</summary>
    public decimal PesoLancado { get; set; }

    public decimal? Frequencia { get; set; }

    public int Aulas { get; set; }

    public int Faltas { get; set; }

    public NivelDesempenho Nivel { get; set; }

    public List<BoletimAvaliacaoDTO> Avaliacoes { get; set; } = [];
}

public class BoletimAvaliacaoDTO
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string? SubDisciplinaNome { get; set; }

    public decimal Peso { get; set; }

    public DateTime DataAplicacao { get; set; }

    /// <summary>Nulo enquanto o professor não lançou a nota.</summary>
    public decimal? Nota { get; set; }
}
