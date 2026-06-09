namespace EduCore.API.Controllers;

using EduCore.API.Constants;
using EduCore.API.DTOs;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Realiza autenticação do usuário e retorna um token JWT.
    /// </summary>
    /// <param name="dto">Credenciais de acesso do usuário.</param>
    /// <returns>Token JWT para autenticação.</returns>
    /// <response code="200">Login realizado com sucesso.</response>
    /// <response code="401">Credenciais inválidas.</response>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDTO dto)
    {
        var token = _authService.Login(dto.Email, dto.Senha);

        if (token == null)
            return Unauthorized(new { message = "Credenciais inválidas" });

        return Ok(new
        {
            token,
            type = "Bearer"
        });
    }

    /// <summary>
    /// Endpoint protegido para validação de autenticação e autorização.
    /// </summary>
    /// <returns>Mensagem de confirmação de acesso autorizado.</returns>
    /// <response code="200">Usuário autenticado com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    /// <response code="403">Usuário sem permissão de acesso.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("teste")]
    public IActionResult Teste()
    {
        return Ok("Você está autenticado!");
    }
}