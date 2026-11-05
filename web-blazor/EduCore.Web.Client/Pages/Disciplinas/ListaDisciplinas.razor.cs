using System.Globalization;
using EduCore.Web.Client.Api;
using EduCore.Web.Client.Disciplinas.Components;
using EduCore.Web.Client.Disciplinas.Models;
using EduCore.Web.Client.Disciplinas.Services;
using EduCore.Web.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.Disciplinas;

public partial class ListaDisciplinas : IDisposable
{
    private const int ItensPorPagina = 10;

    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;

    private readonly CancellationTokenSource _cts = new();
    private readonly PaginationState _paginacao = new() { ItemsPerPage = ItensPorPagina };
    private readonly GridSort<DisciplinaItem> _ordenarPorNome = GridSort<DisciplinaItem>.ByAscending(d => d.Nome);
    private readonly GridSort<DisciplinaItem> _ordenarPorCarga =
        GridSort<DisciplinaItem>.ByAscending(d => d.CargaHoraria).ThenAscending(d => d.Nome);
    private readonly GridSort<DisciplinaItem> _ordenarPorSubDisciplinas =
        GridSort<DisciplinaItem>.ByAscending(d => d.QuantidadeSubDisciplinas).ThenAscending(d => d.Nome);

    private List<DisciplinaItem> _disciplinas = [];
    private IQueryable<DisciplinaItem> _itensVisiveis = Enumerable.Empty<DisciplinaItem>().AsQueryable();
    private ApiError? _erroLista;
    private bool _carregando = true;
    private FiltroStatus _filtro = FiltroStatus.Ativos;
    private string _busca = string.Empty;

    private DisciplinaFormDialog _formulario = default!;

    // Inativacao (com confirmacao)
    private ConfirmDialog _confirmarInativacao = default!;
    private DisciplinaItem? _alvoInativacao;
    private ApiError? _erroInativacao;
    private bool _processando;

    // Feedback no topo da pagina
    private AvisoPagina? _aviso;
    private int _avisoId;

    [Inject]
    private IDisciplinaApiClient DisciplinaApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private int TotalVisivel => _itensVisiveis.Count();

    private string TextoContagem => _carregando
        ? "Carregando..."
        : TotalVisivel == 1 ? "1 disciplina" : $"{TotalVisivel} disciplinas";

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
        ? "Nenhuma disciplina encontrada"
        : _filtro switch
        {
            FiltroStatus.Inativos => "Nenhuma disciplina inativa",
            FiltroStatus.Todos => "Nenhuma disciplina cadastrada",
            _ => "Nenhuma disciplina ativa"
        };

    private string DescricaoVazio => TemBusca
        ? $"Nenhum resultado para \"{_busca.Trim()}\". Confira a grafia ou altere o filtro."
        : _filtro == FiltroStatus.Inativos
            ? "As disciplinas inativadas aparecem aqui e podem ser reativadas."
            : "Cadastre a primeira disciplina para montar a grade da escola.";

    protected override Task OnInitializedAsync() => CarregarAsync();

    private async Task CarregarAsync()
    {
        _carregando = true;
        _erroLista = null;

        var resultado = await DisciplinaApi.ListarAsync(_cts.Token);

        _carregando = false;

        if (resultado.Succeeded)
        {
            _disciplinas = [.. resultado.Value!];
        }
        else
        {
            _disciplinas = [];
            _erroLista = resultado.Error;
        }

        await AplicarFiltrosAsync();
    }

    /// <summary>
    /// Filtra em memória por status e por nome/código, ignorando maiúsculas e acentos.
    /// </summary>
    private async Task AplicarFiltrosAsync()
    {
        var termo = _busca.Trim();

        _itensVisiveis = _disciplinas
            .Where(d => _filtro.Aceita(d.Ativo))
            .Where(d => termo.Length == 0 || Contem(d.Nome, termo) || Contem(d.Codigo, termo))
            .ToList()
            .AsQueryable();

        await _paginacao.SetCurrentPageIndexAsync(0);
    }

    private static bool Contem(string texto, string termo)
        => Comparador.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private static string IdBotaoAcoes(DisciplinaItem disciplina) => $"acoes-disciplina-{disciplina.Id}";

    private void Abrir(DisciplinaItem disciplina) => Navigation.NavigateTo($"disciplinas/{disciplina.Id}");

    private Task NovaAsync() => _formulario.AbrirNovaAsync();

    private Task EditarAsync(DisciplinaItem disciplina) => _formulario.AbrirEdicaoAsync(disciplina);

    private async Task AoSalvarAsync(DisciplinaSalva salva)
    {
        var disciplina = salva.Disciplina;
        await SubstituirNaListaAsync(disciplina);

        MostrarAviso(salva.Nova
            ? new AvisoPagina("Disciplina cadastrada", $"{disciplina.Nome} ({disciplina.Codigo}) foi cadastrada. Abra a disciplina para adicionar subdisciplinas.")
            : new AvisoPagina("Disciplina atualizada", $"As alterações em {disciplina.Nome} foram salvas."));
    }

    private async Task SolicitarInativacaoAsync(DisciplinaItem disciplina)
    {
        _alvoInativacao = disciplina;
        _erroInativacao = null;
        await _confirmarInativacao.AbrirAsync();
    }

    private async Task ConfirmarInativacaoAsync()
    {
        if (_alvoInativacao is not { } disciplina || _processando)
        {
            return;
        }

        _processando = true;
        _erroInativacao = null;

        var resultado = await DisciplinaApi.InativarAsync(disciplina.Id, _cts.Token);

        _processando = false;

        if (!resultado.Succeeded)
        {
            _erroInativacao = resultado.Error;
            return;
        }

        await _confirmarInativacao.FecharAsync();
        _alvoInativacao = null;

        await SubstituirNaListaAsync(resultado.Value!);
        MostrarAviso(new AvisoPagina("Disciplina inativada", $"{disciplina.Nome} foi inativada. Para vê-la, selecione \"Inativas\" ou \"Todas\"."));
    }

    private async Task ReativarAsync(DisciplinaItem disciplina)
    {
        var resultado = await DisciplinaApi.ReativarAsync(disciplina.Id, _cts.Token);

        if (!resultado.Succeeded)
        {
            MostrarAviso(new AvisoPagina(resultado.Error.Title, resultado.Error.Message, Erro: true));
            return;
        }

        await SubstituirNaListaAsync(resultado.Value!);
        MostrarAviso(new AvisoPagina("Disciplina reativada", $"{disciplina.Nome} está ativa novamente."));
    }

    /// <summary>
    /// Atualiza (ou inclui) o item na lista local sem recarregar tudo, mantendo a página atual quando possível.
    /// </summary>
    private async Task SubstituirNaListaAsync(DisciplinaItem disciplina)
    {
        var indice = _disciplinas.FindIndex(d => d.Id == disciplina.Id);
        if (indice >= 0)
        {
            _disciplinas[indice] = disciplina;
        }
        else
        {
            _disciplinas.Add(disciplina);
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
