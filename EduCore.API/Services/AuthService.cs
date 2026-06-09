namespace EduCore.API.Services;

using EduCore.API.Data;
using EduCore.API.Interfaces.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(
        AppDbContext context,
        IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public string? Login(string email, string senha)
    {
        var user = _context.Usuarios
            .FirstOrDefault(x => x.Email == email);

        if (user == null)
            return null;

        if (!user.Ativo)
            return null;

        var senhaValida =
            BCrypt.Net.BCrypt.Verify(senha, user.Senha);

        if (!senhaValida)
            return null;

        return _jwtService.GenerateToken(user);
    }
}