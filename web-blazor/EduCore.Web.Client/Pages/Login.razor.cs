using EduCore.Web.Client.Auth.Models;
using EduCore.Web.Client.Auth.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Icons = Microsoft.FluentUI.AspNetCore.Components.Icons;

namespace EduCore.Web.Client.Pages;

public partial class Login : IDisposable
{
    private const string DefaultRedirect = "/";

    private static readonly Icon ShowPasswordIcon = new Icons.Regular.Size20.Eye();
    private static readonly Icon HidePasswordIcon = new Icons.Regular.Size20.EyeOff();

    private readonly LoginRequest _model = new();
    private readonly CancellationTokenSource _cts = new();

    private EditContext _editContext = default!;
    private ValidationMessageStore _serverErrors = default!;
    private LoginResult? _failure;
    private bool _isSubmitting;
    private bool _showPassword;

    [Inject]
    private IAuthService AuthService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [SupplyParameterFromQuery]
    public string? ReturnUrl { get; set; }

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

    private async Task HandleValidSubmitAsync()
    {
        if (!CanSubmit)
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

    public void Dispose()
    {
        _editContext.OnFieldChanged -= HandleFieldChanged;
        _cts.Cancel();
        _cts.Dispose();
    }
}
