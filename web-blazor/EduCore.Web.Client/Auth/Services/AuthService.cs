using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EduCore.Web.Client.Api;
using EduCore.Web.Client.Auth.Models;
using Microsoft.Extensions.Options;

namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// Autentica na API e traduz cada resposta HTTP em um <see cref="LoginResult"/> com mensagem clara para o usuário.
/// </summary>
public sealed class AuthService(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier,
    TimeProvider timeProvider,
    IOptions<EduCoreApiOptions> options) : IAuthService
{
    private const string DefaultTokenType = "Bearer";

    private readonly EduCoreApiOptions _options = options.Value;

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Contrato da EduCore API (LoginDTO): { email, senha }.
        var payload = new { email = request.Email.Trim(), senha = request.Password };

        try
        {
            using var response = await httpClient.PostAsJsonAsync(_options.LoginPath, payload, cancellationToken);

            return response.IsSuccessStatusCode
                ? await HandleSuccessAsync(response, cancellationToken)
                : await HandleFailureAsync(response, cancellationToken);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return LoginResult.Failure(LoginFailureReason.Timeout, LoginMessages.TimeoutTitle, LoginMessages.Timeout);
        }
        catch (HttpRequestException)
        {
            return LoginResult.Failure(LoginFailureReason.Unavailable, LoginMessages.UnavailableTitle, LoginMessages.Unavailable);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            return InvalidResponse();
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        await tokenStore.ClearAsync(cancellationToken);
        authenticationNotifier.NotifyAuthenticationChanged();
    }

    private async Task<LoginResult> HandleSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
        if (body is null || string.IsNullOrWhiteSpace(body.Token) || body.ExpiresIn <= 0)
        {
            return InvalidResponse();
        }

        var token = new AuthToken(
            body.Token,
            string.IsNullOrWhiteSpace(body.Type) ? DefaultTokenType : body.Type,
            timeProvider.GetUtcNow().AddSeconds(body.ExpiresIn));

        await tokenStore.SaveAsync(token, cancellationToken);
        authenticationNotifier.NotifyAuthenticationChanged();
        return LoginResult.Success();
    }

    private async Task<LoginResult> HandleFailureAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var problem = await ApiErrors.TryReadProblemAsync(response, cancellationToken);

        return response.StatusCode switch
        {
            HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity => LoginResult.Failure(
                LoginFailureReason.Validation,
                LoginMessages.ValidationTitle,
                problem?.Detail ?? LoginMessages.Validation,
                problem?.Errors),

            // Nao repassa o detalhe do servidor para nao revelar se o e-mail existe.
            HttpStatusCode.Unauthorized => LoginResult.Failure(
                LoginFailureReason.InvalidCredentials,
                LoginMessages.InvalidCredentialsTitle,
                LoginMessages.InvalidCredentials),

            HttpStatusCode.Forbidden => LoginResult.Failure(
                LoginFailureReason.AccessDenied,
                LoginMessages.AccessDeniedTitle,
                problem?.Detail ?? LoginMessages.AccessDenied),

            HttpStatusCode.Locked => LoginResult.Failure(
                LoginFailureReason.AccountLocked,
                LoginMessages.AccountLockedTitle,
                problem?.Detail ?? LoginMessages.AccountLocked),

            HttpStatusCode.TooManyRequests => LoginResult.Failure(
                LoginFailureReason.TooManyAttempts,
                LoginMessages.TooManyAttemptsTitle,
                BuildTooManyAttemptsMessage(response)),

            HttpStatusCode.RequestTimeout or HttpStatusCode.GatewayTimeout => LoginResult.Failure(
                LoginFailureReason.Timeout,
                LoginMessages.TimeoutTitle,
                LoginMessages.Timeout),

            >= HttpStatusCode.InternalServerError => LoginResult.Failure(
                LoginFailureReason.ServerError,
                LoginMessages.ServerErrorTitle,
                LoginMessages.ServerError),

            _ => LoginResult.Failure(
                LoginFailureReason.Unexpected,
                LoginMessages.UnexpectedTitle,
                string.Format(CultureInfo.CurrentCulture, LoginMessages.Unexpected, (int)response.StatusCode))
        };
    }

    private string BuildTooManyAttemptsMessage(HttpResponseMessage response)
    {
        var retryAfter = response.Headers.RetryAfter;
        var seconds = retryAfter?.Delta?.TotalSeconds
            ?? (retryAfter?.Date - timeProvider.GetUtcNow())?.TotalSeconds;

        return seconds is > 0
            ? string.Format(CultureInfo.CurrentCulture, LoginMessages.TooManyAttemptsWithDelay, Math.Ceiling(seconds.Value))
            : LoginMessages.TooManyAttempts;
    }

    private static LoginResult InvalidResponse()
        => LoginResult.Failure(LoginFailureReason.InvalidResponse, LoginMessages.InvalidResponseTitle, LoginMessages.InvalidResponse);
}
