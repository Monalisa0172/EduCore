using EduCore.API.Services;
using EduCore.API.Interfaces;

namespace EduCore.API.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}