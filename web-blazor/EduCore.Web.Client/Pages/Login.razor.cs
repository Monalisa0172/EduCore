using EduCore.Web.Client.Auth.Models;
using EduCore.Web.Client.Auth.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.JSInterop;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace EduCore.Web.Client.Pages;

public partial class Login : IAsyncDisposable
{
    private const string DefaultRedirect = "/";
    private const string ModuloJs = "./Pages/Login.razor.js";
    private const string EmailFieldId = "login-email";
    private const string PasswordFieldId = "login-password";

    private static readonly Icon ShowPasswordIcon = new Icons.Regular.Size20.Eye();
    private static readonly Icon HidePasswordIcon = new Icons.Regular.Size20.EyeOff();

    private readonly LoginRequest _model = new();
    private readonly CancellationTokenSource _cts = new();

    private EditContext _editContext = default!;
    private ValidationMessageStore _serverErrors = default!;
    private IJSObjectReference? _moduloJs;
    private LoginResult? _failure;
    private bool _isSubmitting;
    private bool _showPassword;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

    /// <summary>"redefinida" quando a pessoa acabou de criar uma nova senha.</summary>
    [SupplyParameterFromQuery(Name = "senha")]
    public string? Senha { get; set; }

    private bool SenhaRedefinida => string.Equals(Senha, "redefinida", StringComparison.OrdinalIgnoreCase);

    [Inject]
    private EmailInformado EmailInformado { get; set; } = default!;

    /// <summary>Guarda o e-mail já digitado para a tela "Esqueci minha senha" (sem colocá-lo na URL).</summary>
    private void GuardarEmail() => EmailInformado.Guardar(_model.Email);

    // Enquanto o WebAssembly carrega (prerender), o formulario fica bloqueado para evitar submit sem handler.
    private bool CanSubmit => RendererInfo.IsInteractive && !_isSubmitting;

    private Icon PasswordToggleIcon => _showPassword ? HidePasswordIcon : ShowPasswordIcon;

    private string PasswordToggleLabel => _showPassword ? "Ocultar senha" : "Mostrar senha";

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
        _serverErrors = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += HandleFieldChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !RendererInfo.IsInteractive)
        {
            return;
        }

        try
        {
            _moduloJs ??= await JS.InvokeAsync<IJSObjectReference>("import", _cts.Token, ModuloJs);
            await _moduloJs.InvokeVoidAsync("ocultarRevelarNativo", _cts.Token, PasswordFieldId);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or OperationCanceledException)
        {
            // Detalhe visual: sem JS, o campo continua funcionando.
        }
    }

    private async Task HandleSubmitAsync()
    {
        if (!CanSubmit)
        {
            return;
        }

        // Valores do preenchimento automatico podem ainda nao ter chegado ao modelo: le do campo antes de validar.
        await SincronizarCamposPreenchidosAsync();

        if (!_editContext.Validate())
        {
            return;
        }

        _isSubmitting = true;
        _failure = null;
        _serverErrors.Clear();

        try
        {
            var result = await AuthService.LoginAsync(_model, _cts.Token);

            if (result.Succeeded)
            {
                Navigation.NavigateTo(GetSafeReturnUrl(), replace: true);
                return;
            }

            ShowFailure(result);
        }
        catch (OperationCanceledException) when (_cts.IsCancellationRequested)
        {
            // Componente descartado durante a requisicao.
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    /// <summary>
    /// Copia para o modelo o que esta de fato nos campos. O navegador preenche a senha salva sem
    /// avisar o Blazor ate a primeira interacao; sem isso o primeiro clique em "Entrar" ia vazio.
    /// </summary>
    private async Task SincronizarCamposPreenchidosAsync()
    {
        try
        {
            _moduloJs ??= await JS.InvokeAsync<IJSObjectReference>("import", _cts.Token, ModuloJs);
            var valores = await _moduloJs.InvokeAsync<string?[]>("lerValores", _cts.Token, (object)new[] { EmailFieldId, PasswordFieldId });

            AtualizarSeDiferente(nameof(LoginRequest.Email), _model.Email, valores[0], v => _model.Email = v);
            AtualizarSeDiferente(nameof(LoginRequest.Password), _model.Password, valores[1], v => _model.Password = v);
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or OperationCanceledException)
        {
            // Sem acesso ao DOM: segue com o que o binding ja tem.
        }
    }

    private void AtualizarSeDiferente(string campo, string atual, string? lido, Action<string> definir)
    {
        if (string.IsNullOrEmpty(lido) || lido == atual)
        {
            return;
        }

        definir(lido);
        _editContext.NotifyFieldChanged(_editContext.Field(campo));
    }

    private void ShowFailure(LoginResult result)
    {
        _failure = result;

        foreach (var (field, messages) in result.FieldErrors)
        {
            var property = ResolveModelProperty(field);
            if (property is not null)
            {
                _serverErrors.Add(_editContext.Field(property), messages);
            }
        }

        // Nao mantem a senha em memoria/tela apos falha de credencial.
        if (result.Reason is LoginFailureReason.InvalidCredentials or LoginFailureReason.AccountLocked)
        {
            _model.Password = string.Empty;
        }

        _editContext.NotifyValidationStateChanged();
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        // O usuario corrigiu o campo: remove o erro vindo do servidor para aquele campo.
        _serverErrors.Clear(e.FieldIdentifier);
        _failure = null;
        _editContext.NotifyValidationStateChanged();
    }

    private void TogglePasswordVisibility() => _showPassword = !_showPassword;

    private static string? ResolveModelProperty(string apiField) => apiField.ToLowerInvariant() switch
    {
        "email" => nameof(LoginRequest.Email),
        "senha" or "password" => nameof(LoginRequest.Password),
        _ => null
    };

    /// <summary>
    /// Aceita apenas caminhos relativos da propria aplicacao (evita open redirect).
    /// </summary>
    private string GetSafeReturnUrl()
    {
        if (string.IsNullOrWhiteSpace(ReturnUrl)
            || !ReturnUrl.StartsWith('/')
            || ReturnUrl.StartsWith("//", StringComparison.Ordinal)
            || ReturnUrl.StartsWith("/\\", StringComparison.Ordinal)
            || !Uri.IsWellFormedUriString(ReturnUrl, UriKind.Relative))
        {
            return DefaultRedirect;
        }

        return ReturnUrl;
    }

    public async ValueTask DisposeAsync()
    {
        _editContext.OnFieldChanged -= HandleFieldChanged;
        _cts.Cancel();
        _cts.Dispose();

        if (_moduloJs is not null)
        {
            try
            {
                await _moduloJs.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Pagina ja descarregada.
            }
        }
    }
}
