using EduCore.Web.Client.Auth.Models;

namespace EduCore.Web.Client.Auth.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
