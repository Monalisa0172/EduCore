namespace EduCore.API.Services;

using EduCore.API.Data;
using EduCore.API.DTOs.Response;
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

    public LoginResponseDTO? Login(string email, string senha)
    {
        var user = _context.Usuarios
            .FirstOrDefault(x => x.Email == email);

        if (user == null)
            return null;

        if (!user.Ativo)
            return null;

        if (!SenhaConfere(senha, user.Senha))
            return null;

        return new LoginResponseDTO(
            _jwtService.GenerateToken(user),
            "Bearer",
            _jwtService.ExpirationMinutes * 60);
    }

    private static bool SenhaConfere(string senha, string? hash)
    {
        if (string.IsNullOrEmpty(hash))
            return false;

        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Senha gravada sem hash BCrypt: trata como credencial invalida (401), nao como erro 500.
            return false;
        }
    }
}