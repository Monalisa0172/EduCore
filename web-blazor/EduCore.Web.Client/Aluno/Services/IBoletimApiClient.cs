using EduCore.Web.Client.Aluno.Models;
using EduCore.Web.Client.Api;

namespace EduCore.Web.Client.Aluno.Services;

public interface IBoletimApiClient
{
    /// <summary>Boletim do aluno logado; sem ano, o mais recente com matrícula.</summary>
    Task<ApiResult<Boletim>> MeuBoletimAsync(int? anoLetivo, CancellationToken cancellationToken = default);

    /// <summary>Boletim de qualquer aluno (admin).</summary>
    Task<ApiResult<Boletim>> BoletimDoAlunoAsync(int alunoId, int? anoLetivo, CancellationToken cancellationToken = default);
}

public sealed class BoletimApiClient(
    HttpClient httpClient,
    Auth.Services.ITokenStore tokenStore,
    Auth.Services.IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IBoletimApiClient
{
    public Task<ApiResult<Boletim>> MeuBoletimAsync(int? anoLetivo, CancellationToken cancellationToken = default)
        => GetAsync<Boletim>($"api/me/boletim{Ano(anoLetivo)}", cancellationToken);

    public Task<ApiResult<Boletim>> BoletimDoAlunoAsync(int alunoId, int? anoLetivo, CancellationToken cancellationToken = default)
        => GetAsync<Boletim>($"api/alunos/{alunoId}/boletim{Ano(anoLetivo)}", cancellationToken);

    private static string Ano(int? anoLetivo) => anoLetivo is { } a ? $"?anoLetivo={a}" : string.Empty;
}
