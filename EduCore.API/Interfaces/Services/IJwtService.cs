namespace EduCore.API.Interfaces.Services;

using EduCore.API.Entities;

public interface IJwtService
{
    string GenerateToken(Usuario user);
}