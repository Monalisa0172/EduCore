using EduCore.API.Interfaces.Repositories;
using EduCore.API.Repositories;
using EduCore.API.Services;
using EduCore.API.Interfaces.Services;
using EduCore.API.Services.Academico;
using EduCore.API.Services.Acesso;

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

        return services;
    }
}
