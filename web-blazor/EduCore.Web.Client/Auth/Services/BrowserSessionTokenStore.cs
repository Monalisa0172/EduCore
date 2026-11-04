using System.Text.Json;
using EduCore.Web.Client.Auth.Models;
using Microsoft.JSInterop;

namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// Persiste o token no sessionStorage: sobrevive ao F5, mas é descartado ao fechar a aba
/// e não é compartilhado entre abas (menor exposição que o localStorage).
/// </summary>
public sealed class BrowserSessionTokenStore(IJSRuntime jsRuntime, TimeProvider timeProvider) : ITokenStore
{
    private const string StorageKey = "auth.token";

    private AuthToken? _cached;

    public async ValueTask SaveAsync(AuthToken token, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(token);

        var json = JsonSerializer.Serialize(token);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, StorageKey, json);
        _cached = token;
    }

    public async ValueTask<AuthToken?> GetAsync(CancellationToken cancellationToken = default)
    {
        _cached ??= await ReadFromStorageAsync(cancellationToken);

        if (_cached is not null && _cached.IsExpired(timeProvider.GetUtcNow()))
        {
            await ClearAsync(cancellationToken);
        }

        return _cached;
    }

    public async ValueTask ClearAsync(CancellationToken cancellationToken = default)
    {
        _cached = null;
        await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, StorageKey);
    }

    private async ValueTask<AuthToken?> ReadFromStorageAsync(CancellationToken cancellationToken)
    {
        var json = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", cancellationToken, StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<AuthToken>(json);
        }
        catch (JsonException)
        {
            // Conteudo adulterado ou de versao anterior: descarta.
            await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, StorageKey);
            return null;
        }
    }
}
