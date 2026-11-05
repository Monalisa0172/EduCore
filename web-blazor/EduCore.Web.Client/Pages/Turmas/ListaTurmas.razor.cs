using System.Globalization;
using EduCore.Web.Client.Api;
using EduCore.Web.Client.Shared;
using EduCore.Web.Client.Turmas.Components;
using EduCore.Web.Client.Turmas.Models;
using EduCore.Web.Client.Turmas.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.Turmas;

public partial class ListaTurmas : IDisposable
{
    private const int ItensPorPagina = 10;

    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;

    private static readonly IReadOnlyList<ItemTrilha> Trilha = [ItemTrilha.Inicio, new("Turmas")];

    private static readonly IReadOnlyList<OpcaoSeletor<FiltroStatus>> OpcoesFiltro =
    [
        new(FiltroStatus.Ativos, "Ativas"),
        new(FiltroStatus.Inativos, "Inativas"),
        new(FiltroStatus.Todos, "Todas")
    ];

    private readonly CancellationTokenSource _cts = new();
    private readonly PaginationState _paginacao = new() { ItemsPerPage = ItensPorPagina };
    // Direcao "crescente" do grid = ano mais recente primeiro e nome em ordem alfabetica.
    private readonly GridSort<TurmaItem> _ordenarPorAno =
        GridSort<TurmaItem>.ByDescending(t => t.AnoLetivo).ThenAscending(t => t.Nome);
    private readonly GridSort<TurmaItem> _ordenarPorAlunos =
        GridSort<TurmaItem>.ByAscending(t => t.QuantidadeAlunos).ThenAscending(t => t.Nome);

    private List<TurmaItem> _turmas = [];
    private IQueryable<TurmaItem> _itensVisiveis = Enumerable.Empty<TurmaItem>().AsQueryable();
    private ApiError? _erroLista;
    private bool _carregando = true;
    private FiltroStatus _filtro = FiltroStatus.Ativos;
    private string _busca = string.Empty;

    // Atalho do card "Turmas sem alunos"
    private bool _somenteSemAlunos;

    private FluentDataGrid<TurmaItem>? _grid;
    private TurmaFormDialog _formulario = default!;

    // Inativacao (com confirmacao)
    private ConfirmDialog _confirmarInativacao = default!;
    private TurmaItem? _alvoInativacao;
    private ApiError? _erroInativacao;
    private bool _processando;

    private AvisoPagina? _aviso;
    private int _avisoId;

    [Inject]
    private ITurmaApiClient TurmaApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private int TotalVisivel => _itensVisiveis.Count();

    // Indicadores (sobre todas as turmas, independente do filtro)
    private int QuantidadeAtivas => _turmas.Count(t => t.Ativo);

    private int QuantidadeMatriculas => _turmas.Where(t => t.Ativo).Sum(t => t.QuantidadeAlunos);

    private int QuantidadeSemAlunos => _turmas.Count(t => t.Ativo && t.SemAlunos);

    private string TextoInativas => (_turmas.Count - QuantidadeAtivas) switch
    {
        0 => "Nenhuma inativa",
        1 => "1 inativa",
        var n => $"{n} inativas"
    };

    private string TextoContagem => _carregando
        ? "Carregando..."
        : TotalVisivel == 1 ? "1 turma" : $"{TotalVisivel} turmas";

    private string TextoResumoPaginacao
    {
        get
        {
            var total = _paginacao.TotalItemCount ?? 0;
            if (total == 0)
            {
                return "Nenhum registro";
            }

            var inicio = (_paginacao.CurrentPageIndex * ItensPorPagina) + 1;
            var fim = Math.Min(inicio + ItensPorPagina - 1, total);
            return $"Mostrando {inicio}–{fim} de {total}";
        }
    }

    private bool TemBusca => !string.IsNullOrWhiteSpace(_busca);

    private string TituloVazio => TemBusca
        ? "Nenhuma turma encontrada"
        : _filtro switch
        {
            FiltroStatus.Inativos => "Nenhuma turma inativa",
            FiltroStatus.Todos => "Nenhuma turma cadastrada",
            _ => "Nenhuma turma ativa"
        };

    private string DescricaoVazio => TemBusca
        ? $"Nenhum resultado para \"{_busca.Trim()}\". Confira a grafia ou altere o filtro."
        : _filtro == FiltroStatus.Inativos
            ? "As turmas inativadas aparecem aqui e podem ser reativadas."
            : "Cadastre a primeira turma do ano letivo para começar as matrículas.";

    protected override Task OnInitializedAsync() => CarregarAsync();

    private async Task CarregarAsync()
    {
        _carregando = true;
        _erroLista = null;

        var resultado = await TurmaApi.ListarAsync(_cts.Token);

        _carregando = false;

        if (resultado.Succeeded)
        {
            _turmas = [.. resultado.Value!];
        }
        else
        {
            _turmas = [];
            _erroLista = resultado.Error;
        }

        await AplicarFiltrosAsync();
    }

    /// <summary>
    /// Filtra em memória por status e por nome/ano, ignorando maiúsculas e acentos.
    /// </summary>
    private async Task AplicarFiltrosAsync()
    {
        var termo = _busca.Trim();

        _itensVisiveis = _turmas
            .Where(t => _filtro.Aceita(t.Ativo))
            .Where(t => !_somenteSemAlunos || t.SemAlunos)
            .Where(t => termo.Length == 0
                || Contem(t.Nome, termo)
                || t.AnoLetivo.ToString(CultureInfo.InvariantCulture).Contains(termo, StringComparison.Ordinal))
            .ToList()
            .AsQueryable();

        await _paginacao.SetCurrentPageIndexAsync(0);
        await AtualizarGridAsync();
    }

    /// <summary>
    /// O FluentDataGrid continua exibindo os dados anteriores quando só o Items muda num filtro em memória:
    /// renderiza a página para o grid receber a lista nova e pede que ele releia os dados.
    /// </summary>
    private async Task AtualizarGridAsync()
    {
        if (_grid is null)
        {
            return;
        }

        StateHasChanged();
        await Task.Yield();
        await _grid.RefreshDataAsync();
    }

    private static bool Contem(string texto, string termo)
        => Comparador.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private static string IdBotaoAcoes(TurmaItem turma) => $"acoes-turma-{turma.Id}";

    private Task AoAlterarFiltroAsync()
    {
        // Escolher outro status sai do atalho "sem alunos".
        _somenteSemAlunos = false;
        return AplicarFiltrosAsync();
    }

    private Task MostrarSemAlunosAsync()
    {
        _somenteSemAlunos = true;
        _filtro = FiltroStatus.Ativos;
        _busca = string.Empty;
        return AplicarFiltrosAsync();
    }

    private Task MostrarTodasAsync()
    {
        _somenteSemAlunos = false;
        return AplicarFiltrosAsync();
    }

    private void Abrir(TurmaItem turma) => Navigation.NavigateTo($"turmas/{turma.Id}");

    private Task NovaAsync() => _formulario.AbrirNovaAsync();

    private Task EditarAsync(TurmaItem turma) => _formulario.AbrirEdicaoAsync(turma);

    private async Task AoSalvarAsync(TurmaSalva salva)
    {
        var turma = salva.Turma;
        await SubstituirNaListaAsync(turma);

        MostrarAviso(salva.Nova
            ? new AvisoPagina("Turma cadastrada", $"{turma.Nome} ({turma.AnoLetivo}) já pode receber matrículas no cadastro de cada aluno.")
            : new AvisoPagina("Turma atualizada", $"As alterações em {turma.Nome} foram salvas."));
    }

    private async Task SolicitarInativacaoAsync(TurmaItem turma)
    {
        _alvoInativacao = turma;
        _erroInativacao = null;
        await _confirmarInativacao.AbrirAsync();
    }

    private async Task ConfirmarInativacaoAsync()
    {
        if (_alvoInativacao is not { } turma || _processando)
        {
            return;
        }

        _processando = true;
        _erroInativacao = null;

        var resultado = await TurmaApi.InativarAsync(turma.Id, _cts.Token);

        _processando = false;

        if (!resultado.Succeeded)
        {
            _erroInativacao = resultado.Error;
            return;
        }

        await _confirmarInativacao.FecharAsync();
        _alvoInativacao = null;

        await SubstituirNaListaAsync(resultado.Value!);
        MostrarAviso(new AvisoPagina("Turma inativada", $"{turma.Nome} foi inativada. Para vê-la, selecione \"Inativas\" ou \"Todas\"."));
    }

    private async Task ReativarAsync(TurmaItem turma)
    {
        var resultado = await TurmaApi.ReativarAsync(turma.Id, _cts.Token);

        if (!resultado.Succeeded)
        {
            MostrarAviso(new AvisoPagina(resultado.Error.Title, resultado.Error.Message, Erro: true));
            return;
        }

        await SubstituirNaListaAsync(resultado.Value!);
        MostrarAviso(new AvisoPagina("Turma reativada", $"{turma.Nome} voltou a aceitar matrículas."));
    }

    /// <summary>
    /// Atualiza (ou inclui) o item na lista local sem recarregar tudo, mantendo a página atual quando possível.
    /// </summary>
    private async Task SubstituirNaListaAsync(TurmaItem turma)
    {
        var indice = _turmas.FindIndex(t => t.Id == turma.Id);
        if (indice >= 0)
        {
            _turmas[indice] = turma;
        }
        else
        {
            _turmas.Add(turma);
        }

        var paginaAtual = _paginacao.CurrentPageIndex;
        await AplicarFiltrosAsync();

        var ultimaPagina = Math.Max(0, (TotalVisivel - 1) / ItensPorPagina);
        await _paginacao.SetCurrentPageIndexAsync(Math.Min(paginaAtual, ultimaPagina));
    }

    private void MostrarAviso(AvisoPagina aviso)
    {
        _aviso = aviso;
        _avisoId++;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
