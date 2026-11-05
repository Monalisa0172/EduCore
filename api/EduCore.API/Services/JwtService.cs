using EduCore.API.Entities;
using EduCore.API.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public int ExpirationMinutes =>
        int.TryParse(_config["Jwt:ExpirationMinutes"], out var minutes) && minutes > 0
            ? minutes
            : 120;

    public string GenerateToken(Usuario user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            // Nome para exibicao nos fronts (saudacao, cabecalho); nao e usado em autorizacao.
            new Claim(ClaimTypes.Name, string.IsNullOrWhiteSpace(user.Nome) ? user.Email ?? "" : user.Nome.Trim()),
            new Claim(ClaimTypes.Role, user.Tipo.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}