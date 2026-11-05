using EduCore.Web.Client.Api;
using EduCore.Web.Client.Shared;
using EduCore.Web.Client.Turmas.Models;
using EduCore.Web.Client.Turmas.Services;
using EduCore.Web.Client.Usuarios.Models;
using EduCore.Web.Client.Usuarios.Services;
using Microsoft.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.Usuarios;

public partial class EditarUsuario : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private UsuarioDetalhe? _usuario;
    private UsuarioFormModel? _modelo;
    private ApiError? _erro;

    private IReadOnlyList<TurmaOpcao>? _turmas;
    private ApiError? _erroTurmas;

    [Parameter]
    public int UsuarioId { get; set; }

    /// <summary>
    /// Página de origem (ex.: /turmas/5): salvar ou cancelar volta para ela em vez do painel.
    /// </summary>
    [SupplyParameterFromQuery(Name = "voltar")]
    public string? Voltar { get; set; }

    /// <summary>Nome da página de origem, exibido na trilha (ex.: nome da turma).</summary>
    [SupplyParameterFromQuery(Name = "origem")]
    public string? Origem { get; set; }

    /// <summary>Caminho relativo seguro para o retorno; vazio = painel.</summary>
    private string UrlRetorno => CaminhoInterno(Voltar) ?? string.Empty;

    private IReadOnlyList<ItemTrilha>? Trilha
    {
        get
        {
            var retorno = CaminhoInterno(Voltar);
            if (retorno is null || !retorno.StartsWith("turmas/", StringComparison.Ordinal))
            {
                return null;
            }

            var origem = string.IsNullOrWhiteSpace(Origem) ? "Turma" : Origem.Trim();
            return [ItemTrilha.Inicio, new("Turmas", "turmas"), new(origem, retorno), new("Editar usuário")];
        }
    }

    /// <summary>
    /// Aceita só caminhos da própria aplicação (evita open redirect) e devolve sem a barra inicial.
    /// </summary>
    private static string? CaminhoInterno(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)
            || !url.StartsWith('/')
            || url.StartsWith("//", StringComparison.Ordinal)
            || url.StartsWith("/\\", StringComparison.Ordinal)
            || !Uri.IsWellFormedUriString(url, UriKind.Relative))
        {
            return null;
        }

        return url.TrimStart('/');
    }

    [Inject]
    private IUsuarioApiClient UsuarioApi { get; set; } = default!;

    [Inject]
    private ITurmaApiClient TurmaApi { get; set; } = default!;

    [Inject]
    private AvisoNavegacao Avisos { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    // OnParametersSet (e nao OnInitialized): ir de /usuarios/1 para /usuarios/2 reaproveita a mesma instancia.
    protected override Task OnParametersSetAsync() => CarregarAsync();

    private async Task CarregarAsync()
    {
        _erro = null;
        _usuario = null;
        _modelo = null;

        // Independentes: usuario e turmas carregam em paralelo.
        var usuarioTask = UsuarioApi.GetByIdAsync(UsuarioId, _cts.Token);
        await Task.WhenAll(usuarioTask, CarregarTurmasAsync());

        var resultado = await usuarioTask;
        if (resultado.Succeeded)
        {
            _usuario = resultado.Value!;
            _modelo = UsuarioFormModel.ParaEdicao(_usuario);
        }
        else
        {
            _erro = resultado.Error;
        }
    }

    private async Task CarregarTurmasAsync()
    {
        _erroTurmas = null;
        _turmas = null;

        var resultado = await TurmaApi.GetAtivasAsync(_cts.Token);

        if (resultado.Succeeded)
        {
            _turmas = resultado.Value;
        }
        else
        {
            _erroTurmas = resultado.Error;
        }
    }

    private async Task<ApiError?> AtualizarAsync(UsuarioFormModel modelo, CancellationToken cancellationToken)
    {
        var resultado = await UsuarioApi.AtualizarAsync(UsuarioId, modelo.ParaAtualizacao(), cancellationToken);
        if (!resultado.Succeeded)
        {
            return resultado.Error;
        }

        Avisos.Publicar("Alterações salvas", $"Os dados de {resultado.Value!.Nome} foram atualizados.");

        return null;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
