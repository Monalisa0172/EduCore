using EduCore.API.Interfaces.Repositories;
using EduCore.API.Repositories;
using EduCore.API.Services;
using EduCore.API.Interfaces.Services;

namespace EduCore.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<AlunoService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<ProfessorService>();
        services.AddScoped<DisciplinaService>();

        services.AddScoped<IAlunoRepository, AlunoRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IProfessorRepository, ProfessorRepository>();
        services.AddScoped<IFuncionarioRepository, FuncionarioRepository>();
        services.AddScoped<IDisciplinaRepository, DisciplinaRepository>();

        return services;
    }
}