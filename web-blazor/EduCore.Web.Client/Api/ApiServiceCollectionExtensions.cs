using EduCore.Web.Client.Aluno.Services;
using EduCore.Web.Client.Atribuicoes.Services;
using EduCore.Web.Client.Auth.Services;
using EduCore.Web.Client.Disciplinas.Services;
using EduCore.Web.Client.Professor.Services;
using EduCore.Web.Client.Turmas.Services;
using EduCore.Web.Client.Usuarios.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace EduCore.Web.Client.Api;

public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Registra autenticação e clientes da EduCore API. Chamado no Client (WebAssembly) e no Server (prerender).
    /// </summary>
    public static IServiceCollection AddEduCoreApi(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new EduCoreApiOptions();
        configuration.GetSection(EduCoreApiOptions.SectionName).Bind(options);
        options.Validate();

        services.AddSingleton(Options.Create(options));
        services.TryAddSingleton(TimeProvider.System);

        // Autenticacao
        services.AddScoped<ITokenStore, BrowserSessionTokenStore>();
        services.AddAuthorizationCore();
        services.AddCascadingAuthenticationState();
        services.AddScoped<JwtAuthenticationStateProvider>();
        services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
        services.AddScoped<IAuthenticationStateNotifier>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());
        services.AddHttpClient<IAuthService, AuthService>(ConfigureClient);

        // Clientes da API (anexam o JWT em cada chamada)
        services.AddHttpClient<IUsuarioApiClient, UsuarioApiClient>(ConfigureClient);
        services.AddHttpClient<ITurmaApiClient, TurmaApiClient>(ConfigureClient);
        services.AddHttpClient<IDisciplinaApiClient, DisciplinaApiClient>(ConfigureClient);
        services.AddHttpClient<ISubDisciplinaApiClient, SubDisciplinaApiClient>(ConfigureClient);
        services.AddHttpClient<IAtribuicaoApiClient, AtribuicaoApiClient>(ConfigureClient);
        services.AddHttpClient<IDiarioApiClient, DiarioApiClient>(ConfigureClient);
        services.AddHttpClient<IBoletimApiClient, BoletimApiClient>(ConfigureClient);
        services.AddHttpClient<IAcompanhamentoApiClient, AcompanhamentoApiClient>(ConfigureClient);

        return services;

        void ConfigureClient(HttpClient client)
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }
    }
}
