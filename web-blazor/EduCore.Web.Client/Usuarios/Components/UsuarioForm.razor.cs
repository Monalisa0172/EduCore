using System.Globalization;
using EduCore.Web.Client.Api;
using EduCore.Web.Client.Turmas.Models;
using EduCore.Web.Client.Usuarios.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;

namespace EduCore.Web.Client.Usuarios.Components;

public partial class UsuarioForm : IAsyncDisposable
{
    private const string ModuloJs = "./Usuarios/Components/UsuarioForm.razor.js";

    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly CancellationTokenSource _cts = new();

    private EditContext _editContext = default!;
    private ValidationMessageStore _mensagensServidor = default!;
    private UsuarioFormModel? _modeloAtual;

    private IReadOnlyList<TurmaOpcao>? _turmasAtuais;
    private List<TurmaSelecionavel> _turmas = [];

    private ApiError? _erroGeral;
    private int _erroGeralId;
    private bool _salvando;
    private bool _mostrarSenha;

    // Navegacao protegida
    private FluentDialog _dialogoDescartar = default!;
    private bool _liberarNavegacao;
    private string? _destinoPendente;
    private IJSObjectReference? _moduloJs;
    private DotNetObjectReference<UsuarioForm>? _referenciaDotNet;
    private bool _protecaoDeLinksAtiva;

    [Parameter, EditorRequired]
    public UsuarioFormModel Model { get; set; } = default!;

    /// <summary>
    /// Envia o formulário à API. Retorna o erro ou nulo em caso de sucesso.
    /// </summary>
    [Parameter, EditorRequired]
    public Func<UsuarioFormModel, CancellationToken, Task<ApiError?>> OnSalvar { get; set; } = default!;

    /// <summary>
    /// Turmas ativas; nulo enquanto carregam.
    /// </summary>
    [Parameter]
    public IReadOnlyList<TurmaOpcao>? Turmas { get; set; }

    [Parameter]
    public ApiError? ErroTurmas { get; set; }

    [Parameter]
    public EventCallback OnRecarregarTurmas { get; set; }

    /// <summary>
    /// Para onde voltar ao salvar ou cancelar.
    /// </summary>
    [Parameter]
    public string UrlRetorno { get; set; } = string.Empty;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private bool TemAlteracoesNaoSalvas => !_liberarNavegacao && _editContext.IsModified();

    private string TextoTurmasSelecionadas => Model.TurmaIds.Count switch
    {
        0 => "Nenhuma selecionada",
        1 => "1 selecionada",
        var n => $"{n} selecionadas"
    };

    protected override void OnParametersSet()
    {
        // O EditContext so e recriado quando a pagina entrega outro modelo.
        if (!ReferenceEquals(Model, _modeloAtual))
        {
            if (_editContext is not null)
            {
                _editContext.OnFieldChanged -= AoAlterarCampo;
            }

            _modeloAtual = Model;
            _editContext = new EditContext(Model);
            _mensagensServidor = new ValidationMessageStore(_editContext);
            _editContext.OnFieldChanged += AoAlterarCampo;
        }

        if (!ReferenceEquals(Turmas, _turmasAtuais))
        {
            _turmasAtuais = Turmas;
            _turmas = (Turmas ?? [])
                .Select(t => new TurmaSelecionavel(t) { Selecionada = Model.TurmaIds.Contains(t.Id) })
                .ToList();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _moduloJs = await JS.InvokeAsync<IJSObjectReference>("import", ModuloJs);
        }

        // Liga a captura de links so enquanto houver algo a perder (e desliga ao salvar/descartar).
        var deveProteger = TemAlteracoesNaoSalvas;
        if (_moduloJs is null || deveProteger == _protecaoDeLinksAtiva)
        {
            return;
        }

        _protecaoDeLinksAtiva = deveProteger;

        if (deveProteger)
        {
            _referenciaDotNet ??= DotNetObjectReference.Create(this);
            await _moduloJs.InvokeVoidAsync("ativar", _referenciaDotNet);
        }
        else
        {
            await _moduloJs.InvokeVoidAsync("desativar");
        }
    }

    /// <summary>
    /// Chamado pelo módulo JS ao clicar num link interno: navegar pelo NavigationManager aciona o NavigationLock.
    /// </summary>
    [JSInvokable]
    public void NavegarProtegido(string url) => Navigation.NavigateTo(url);

    /// <summary>
    /// O erro devolvido pela API vale até o usuário mexer no campo.
    /// </summary>
    private void AoAlterarCampo(object? sender, FieldChangedEventArgs e)
        => _mensagensServidor.Clear(e.FieldIdentifier);

    private void SincronizarTurmas()
    {
        Model.TurmaIds = _turmas.Where(t => t.Selecionada).Select(t => t.Turma.Id).ToList();
        _editContext.NotifyFieldChanged(FieldIdentifier.Create(() => Model.TurmaIds));
    }

    private async Task SalvarAsync()
    {
        if (_salvando)
        {
            return;
        }

        _salvando = true;
        _erroGeral = null;
        _mensagensServidor.Clear();

        var erro = await OnSalvar(Model, _cts.Token);

        _salvando = false;

        if (erro is null)
        {
            _liberarNavegacao = true;
            Navigation.NavigateTo(UrlRetorno);
            return;
        }

        _erroGeral = ExibirErro(erro);
        _erroGeralId++;
        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Coloca cada erro da API embaixo do campo correspondente e devolve o resumo para o topo das ações.
    /// </summary>
    private ApiError ExibirErro(ApiError erro)
    {
        if (erro.FieldErrors is not { Count: > 0 } errosPorCampo)
        {
            return erro;
        }

        var semCampo = new List<string>();

        foreach (var (chave, mensagens) in errosPorCampo)
        {
            var campo = UsuarioFormModel.CampoDoErroDaApi(chave);
            if (campo is null)
            {
                semCampo.AddRange(mensagens);
            }
            else
            {
                _mensagensServidor.Add(new FieldIdentifier(Model, campo), mensagens);
            }
        }

        var mensagem = semCampo.Count > 0
            ? string.Join(" ", semCampo)
            : erro.Kind == ApiErrorKind.Conflict
                ? erro.Message
                : "Revise os campos destacados e tente novamente.";

        return erro with { Title = "Não foi possível salvar", Message = mensagem };
    }

    private void Cancelar() => Navigation.NavigateTo(UrlRetorno);

    private async Task AoNavegarAsync(LocationChangingContext contexto)
    {
        if (!TemAlteracoesNaoSalvas)
        {
            return;
        }

        contexto.PreventNavigation();
        _destinoPendente = contexto.TargetLocation;
        await _dialogoDescartar.ShowAsync();
    }

    private async Task ContinuarEditandoAsync()
    {
        _destinoPendente = null;
        await _dialogoDescartar.HideAsync();
    }

    private async Task DescartarAsync()
    {
        _liberarNavegacao = true;
        await _dialogoDescartar.HideAsync();
        Navigation.NavigateTo(_destinoPendente ?? UrlRetorno);
    }

    public async ValueTask DisposeAsync()
    {
        _editContext.OnFieldChanged -= AoAlterarCampo;
        _cts.Cancel();
        _cts.Dispose();

        if (_moduloJs is not null)
        {
            await _moduloJs.InvokeVoidAsync("desativar");
            await _moduloJs.DisposeAsync();
        }

        _referenciaDotNet?.Dispose();
    }

    /// <summary>
    /// Checkbox ligado a uma turma: o @bind precisa de uma propriedade gravável por item.
    /// </summary>
    private sealed class TurmaSelecionavel(TurmaOpcao turma)
    {
        public TurmaOpcao Turma { get; } = turma;

        public bool Selecionada { get; set; }
    }
}
