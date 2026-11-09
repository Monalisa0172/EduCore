using EduCore.API.Interfaces.Repositories;
using EduCore.API.Repositories;
using EduCore.API.Services;
using EduCore.API.Interfaces.Services;
using EduCore.API.Services.Academico;
using EduCore.API.Services.Acesso;
using EduCore.API.Services.Email;

namespace EduCore.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Quem esta logado e o que pode acessar (por requisicao).
        services.AddHttpContextAccessor();
        services.AddScoped<IUsuarioAtual, UsuarioAtual>();
        services.AddScoped<IAcessoAcademico, AcessoAcademico>();

        // Relogio injetavel (testes controlam "hoje") e calendario dos bimestres.
        services.AddSingleton(TimeProvider.System);
        services.Configure<CalendarioLetivoOptions>(configuration.GetSection(CalendarioLetivoOptions.Secao));
        services.AddSingleton<CalendarioLetivo>();

        // E-mail: SMTP quando configurado; senao grava em arquivo (desenvolvimento).
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.Secao));
        var email = configuration.GetSection(EmailOptions.Secao).Get<EmailOptions>() ?? new EmailOptions();
        if (email.UsaSmtp)
            services.AddSingleton<IEnviadorEmail, EnviadorEmailSmtp>();
        else
            services.AddSingleton<IEnviadorEmail, EnviadorEmailArquivo>();

        // Esqueci minha senha: pedidos em fila, processados em segundo plano.
        // Sem a URL do front o link do e-mail sairia quebrado: a API nem sobe (erro claro na configuracao).
        services.AddOptions<RedefinicaoSenhaOptions>()
            .Bind(configuration.GetSection(RedefinicaoSenhaOptions.Secao))
            .Validate(o => Uri.TryCreate(o.UrlFront, UriKind.Absolute, out var url) && (url.Scheme == Uri.UriSchemeHttps || url.Scheme == Uri.UriSchemeHttp),
                "Configure RedefinicaoSenha:UrlFront com o endereço do front (ex.: https://educore.exemplo.com).")
            .ValidateOnStart();
        services.AddSingleton<FilaRedefinicaoSenha>();
        services.AddHostedService<ProcessadorRedefinicaoSenha>();
        services.AddScoped<RedefinicaoSenhaService>();
        services.AddScoped<IRedefinicaoSenhaRepository, RedefinicaoSenhaRepository>();

        services.AddScoped<AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<AlunoService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<UsuarioCadastroService>();
        services.AddScoped<ProfessorService>();
        services.AddScoped<DisciplinaService>();
        services.AddScoped<SubDisciplinaService>();
        services.AddScoped<TurmaService>();
        services.AddScoped<AtribuicaoService>();
        services.AddScoped<AvaliacaoService>();
        services.AddScoped<NotaService>();
        services.AddScoped<ChamadaService>();
        services.AddScoped<BoletimService>();
        services.AddScoped<DiarioService>();
        services.AddScoped<AcompanhamentoService>();

        services.AddScoped<IAlunoRepository, AlunoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProfessorRepository, ProfessorRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();
        services.AddScoped<ISubDisciplinaRepository, SubDisciplinaRepository>();
        services.AddScoped<ITurmaRepository, TurmaRepository>();
        services.AddScoped<IAtribuicaoRepository, AtribuicaoRepository>();
        services.AddScoped<IAvaliacaoRepository, AvaliacaoRepository>();
        services.AddScoped<INotaRepository, NotaRepository>();
        services.AddScoped<IPresencaRepository, PresencaRepository>();
        services.AddScoped<IBoletimRepository, BoletimRepository>();
        services.AddScoped<IAcompanhamentoRepository, AcompanhamentoRepository>();

        return services;
    }
}
