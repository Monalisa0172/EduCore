using System.Globalization;
using EduCore.Web.Client.Api;
using EduCore.Web.Client.Atribuicoes.Models;
using EduCore.Web.Client.Auth.Services;
using EduCore.Web.Client.Professor.Models;

namespace EduCore.Web.Client.Professor.Services;

public sealed class DiarioApiClient(
    HttpClient httpClient,
    ITokenStore tokenStore,
    IAuthenticationStateNotifier authenticationNotifier)
    : EduCoreApiClientBase(httpClient, tokenStore, authenticationNotifier), IDiarioApiClient
{
    private const string Avaliacoes = "api/avaliacoes";
    private const string Chamadas = "api/chamadas";

    public Task<ApiResult<IReadOnlyList<AtribuicaoItem>>> MinhasAtribuicoesAsync(CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<AtribuicaoItem>>("api/me/atribuicoes", cancellationToken);

    public Task<ApiResult<DiarioInfo>> ObterDiarioAsync(int turmaId, int disciplinaId, CancellationToken cancellationToken = default)
        => GetAsync<DiarioInfo>($"api/diario?turmaId={turmaId}&disciplinaId={disciplinaId}", cancellationToken);

    public Task<ApiResult<IReadOnlyList<AvaliacaoItem>>> ListarAvaliacoesAsync(int turmaId, int disciplinaId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<AvaliacaoItem>>($"{Avaliacoes}?turmaId={turmaId}&disciplinaId={disciplinaId}", cancellationToken);

    public Task<ApiResult<AvaliacaoItem>> CriarAvaliacaoAsync(SalvarAvaliacaoRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarAvaliacaoRequest, AvaliacaoItem>(HttpMethod.Post, Avaliacoes, request, cancellationToken);

    public Task<ApiResult<AvaliacaoItem>> AtualizarAvaliacaoAsync(int id, SalvarAvaliacaoRequest request, CancellationToken cancellationToken = default)
        => SendJsonAsync<SalvarAvaliacaoRequest, AvaliacaoItem>(HttpMethod.Put, $"{Avaliacoes}/{id}", request, cancellationToken);

    public Task<ApiResult<AvaliacaoItem>> InativarAvaliacaoAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<AvaliacaoItem>(new HttpRequestMessage(HttpMethod.Patch, $"{Avaliacoes}/{id}/inativar"), cancellationToken);

    public Task<ApiResult<AvaliacaoItem>> ReativarAvaliacaoAsync(int id, CancellationToken cancellationToken = default)
        => SendAsync<AvaliacaoItem>(new HttpRequestMessage(HttpMethod.Patch, $"{Avaliacoes}/{id}/reativar"), cancellationToken);

    public Task<ApiResult<NotasDaAvaliacao>> ObterNotasAsync(int avaliacaoId, CancellationToken cancellationToken = default)
        => GetAsync<NotasDaAvaliacao>($"{Avaliacoes}/{avaliacaoId}/notas", cancellationToken);

    public Task<ApiResult<NotasDaAvaliacao>> LancarNotasAsync(int avaliacaoId, IReadOnlyList<NotaLancamento> notas, CancellationToken cancellationToken = default)
        => SendJsonAsync<object, NotasDaAvaliacao>(HttpMethod.Put, $"{Avaliacoes}/{avaliacaoId}/notas", new { notas }, cancellationToken);

    public Task<ApiResult<ChamadaInfo>> ObterChamadaAsync(int turmaId, int disciplinaId, DateOnly data, CancellationToken cancellationToken = default)
        => GetAsync<ChamadaInfo>($"{Chamadas}?turmaId={turmaId}&disciplinaId={disciplinaId}&data={Iso(data)}", cancellationToken);

    public Task<ApiResult<IReadOnlyList<AulaRegistrada>>> ListarAulasAsync(int turmaId, int disciplinaId, CancellationToken cancellationToken = default)
        => GetAsync<IReadOnlyList<AulaRegistrada>>($"{Chamadas}/aulas?turmaId={turmaId}&disciplinaId={disciplinaId}", cancellationToken);

    public Task<ApiResult<ChamadaInfo>> RegistrarChamadaAsync(int turmaId, int disciplinaId, DateOnly data, IReadOnlyList<PresencaLancamento> presencas, CancellationToken cancellationToken = default)
        => SendJsonAsync<object, ChamadaInfo>(HttpMethod.Put, Chamadas, new { turmaId, disciplinaId, data = Iso(data), presencas }, cancellationToken);

    private static string Iso(DateOnly data) => data.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
