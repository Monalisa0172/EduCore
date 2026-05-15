using EduCore.API.Entities;

namespace EduCore.API.Interfaces

{
    public interface IJwtService
    {
        string GenerateToken(Usuario user);
    }
}
