using EduCore.Web.Client.Auth.Models;

namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// Abstração do armazenamento do token, permitindo trocar a estratégia
/// (sessionStorage, memória, cookie HttpOnly via BFF) sem alterar os consumidores.
/// </summary>
public interface ITokenStore
{
    ValueTask SaveAsync(AuthToken token, CancellationToken cancellationToken = default);

    ValueTask<AuthToken?> GetAsync(CancellationToken cancellationToken = default);

    ValueTask ClearAsync(CancellationToken cancellationToken = default);
}
