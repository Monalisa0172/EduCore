namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// Avisa a interface que o usuário entrou ou saiu, sem acoplar o AuthService ao provider concreto.
/// </summary>
public interface IAuthenticationStateNotifier
{
    void NotifyAuthenticationChanged();
}
