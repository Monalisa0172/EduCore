namespace EduCore.API.Interfaces.Services;

using EduCore.API.Entities;

public interface IJwtService
{
    int ExpirationMinutes { get; }

    string GenerateToken(Usuario user);
}