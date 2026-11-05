using System.Globalization;
using EduCore.Web.Client.Api;
using EduCore.Web.Client.Shared;
using EduCore.Web.Client.Usuarios.Models;
using EduCore.Web.Client.Usuarios.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.HomeViews;

public partial class AdminHome : IDisposable
{
    private const int ItensPorPagina = 10;

    private static readonly CompareInfo Comparador = CultureInfo.InvariantCulture.CompareInfo;

    private static readonly IReadOnlyList<OpcaoSeletor<TipoUsuarioFiltro>> OpcoesFiltro =
    [
        new(TipoUsuarioFiltro.Todos, "Todos"),
        new(TipoUsuarioFiltro.Alunos, "Alunos"),
        new(TipoUsuarioFiltro.Professores, "Professores")
    ];

    private readonly CancellationTokenSource _cts = new();
    private readonly PaginationState _paginacao = new() { ItemsPerPage = ItensPorPagina };
    private readonly GridSort<UsuarioListItem> _ordenarPorNome = GridSort<UsuarioListItem>.ByAscending(u => u.Nome);
    private readonly GridSort<UsuarioListItem> _ordenarPorTipo =
        GridSort<UsuarioListItem>.ByAscending(u => u.Tipo).ThenAscending(u => u.Nome);

    // Indicadores
    private UsuariosResumo? _resumo;
    private ApiError? _erroResumo;

    // Grid
    private IReadOnlyList<UsuarioListItem> _usuarios = [];
    private IQueryable<UsuarioListItem> _itensVisiveis = Enumerable.Empty<UsuarioListItem>().AsQueryable();
    private ApiError? _erroUsuarios;
    private bool _carregandoUsuarios = true;
    private TipoUsuarioFiltro _filtro = TipoUsuarioFiltro.Todos;
    private string _busca = string.Empty;
    private int _versaoConsulta;

    // Inativacao
    private FluentDialog _dialogoInativar = default!;
    private UsuarioListItem? _usuarioParaInativar;
    private ApiError? _erroInativar;
    private bool _inativando;

    // Feedback de sucesso (inativar aqui, ou cadastrar/editar e voltar)
    private Aviso? _avisoSucesso;
    private int _avisoSucessoId;

    [Inject]
    private IUsuarioApiClient UsuarioApi { get; set; } = default!;

    [Inject]
    private AvisoNavegacao Avisos { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private int TotalVisivel => _itensVisiveis.Count();

    private string TextoContagem => _carregandoUsuarios
        ? "Carregando..."
        : TotalVisivel == 1 ? "1 usuário" : $"{TotalVisivel} usuários";

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

    private string TituloVazio => TemBusca ? "Nenhum usuário encontrado" : "Nenhum usuário ativo";

    private string DescricaoVazio => TemBusca
        ? $"Nenhum resultado para \"{_busca.Trim()}\". Confira a grafia ou altere o filtro."
        : "Cadastre um novo usuário para começar.";

    protected override async Task OnInitializedAsync()
    {
        _avisoSucesso = Avisos.Consumir();

        // As duas consultas sao independentes: rodam em paralelo.
        await Task.WhenAll(CarregarResumoAsync(), CarregarUsuariosAsync());
    }

    private async Task CarregarResumoAsync()
    {
        _erroResumo = null;
        _resumo = null;

        var resultado = await UsuarioApi.GetResumoAsync(_cts.Token);

        if (resultado.Succeeded)
        {
            _resumo = resultado.Value;
        }
        else
        {
            _erroResumo = resultado.Error;
        }

        StateHasChanged();
    }

    private async Task CarregarUsuariosAsync()
    {
        // Se o filtro mudar antes da resposta chegar, so a consulta mais recente vale.
        var versao = ++_versaoConsulta;

        _carregandoUsuarios = true;
        _erroUsuarios = null;

        var resultado = await UsuarioApi.GetAtivosAsync(_filtro, _cts.Token);

        if (versao != _versaoConsulta)
        {
            return;
        }

        _carregandoUsuarios = false;

        if (resultado.Succeeded)
        {
            _usuarios = resultado.Value!;
        }
        else
        {
            _usuarios = [];
            _erroUsuarios = resultado.Error;
        }

        await AplicarBuscaAsync();
        StateHasChanged();
    }

    private Task AoAlterarFiltroAsync() => CarregarUsuariosAsync();

    /// <summary>
    /// Filtra em memória por nome ou e-mail, ignorando maiúsculas e acentos ("joao" encontra "João").
    /// </summary>
    private async Task AplicarBuscaAsync()
    {
        var termo = _busca.Trim();

        _itensVisiveis = (termo.Length == 0
                ? _usuarios
                : _usuarios.Where(u => Contem(u.Nome, termo) || Contem(u.Email, termo)).ToList())
            .AsQueryable();

        await _paginacao.SetCurrentPageIndexAsync(0);
    }

    private static bool Contem(string texto, string termo)
        => Comparador.IndexOf(texto, termo, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

    private static string IdBotaoAcoes(UsuarioListItem usuario) => $"acoes-usuario-{usuario.UsuarioId}";

    private void IrParaNovoUsuario() => Navigation.NavigateTo("usuarios/novo");

    private void IrParaEdicao(UsuarioListItem usuario) => Navigation.NavigateTo($"usuarios/{usuario.UsuarioId}");

    private async Task SolicitarInativacaoAsync(UsuarioListItem usuario)
    {
        _usuarioParaInativar = usuario;
        _erroInativar = null;
        await _dialogoInativar.ShowAsync();
    }

    private async Task CancelarInativacaoAsync()
    {
        await _dialogoInativar.HideAsync();
        _usuarioParaInativar = null;
    }

    private async Task ConfirmarInativacaoAsync()
    {
        if (_usuarioParaInativar is not { } usuario || _inativando)
        {
            return;
        }

        _inativando = true;
        _erroInativar = null;

        var resultado = await UsuarioApi.InativarAsync(usuario.UsuarioId, _cts.Token);

        _inativando = false;

        // 404: alguem ja removeu o usuario; a lista local tambem deve refletir isso.
        if (!resultado.Succeeded && resultado.Error.Kind != ApiErrorKind.NotFound)
        {
            _erroInativar = resultado.Error;
            return;
        }

        await _dialogoInativar.HideAsync();
        _usuarioParaInativar = null;

        await RemoverDaListaAsync(usuario);

        _avisoSucesso = new Aviso("Usuário inativado", $"{usuario.Nome} foi inativado(a) e não tem mais acesso ao sistema.");
        _avisoSucessoId++;
    }

    private async Task RemoverDaListaAsync(UsuarioListItem usuario)
    {
        _usuarios = _usuarios.Where(u => u.UsuarioId != usuario.UsuarioId).ToList();

        if (_resumo is not null)
        {
            _resumo = usuario.IsAluno
                ? _resumo with { AlunosAtivos = Math.Max(0, _resumo.AlunosAtivos - 1) }
                : _resumo with { ProfessoresAtivos = Math.Max(0, _resumo.ProfessoresAtivos - 1) };
        }

        // Mantem a pagina atual; so volta uma pagina se a atual ficou vazia.
        var paginaAtual = _paginacao.CurrentPageIndex;
        await AplicarBuscaAsync();

        var ultimaPagina = Math.Max(0, (TotalVisivel - 1) / ItensPorPagina);
        await _paginacao.SetCurrentPageIndexAsync(Math.Min(paginaAtual, ultimaPagina));
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
