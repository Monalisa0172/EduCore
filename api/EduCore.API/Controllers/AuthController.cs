namespace EduCore.API.Controllers;

using EduCore.API.Constants;
using EduCore.API.DTOs;
using EduCore.API.DTOs.Response;
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
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Credenciais inválidas.</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Login([FromBody] LoginDTO dto)
    {
        var response = _authService.Login(dto.Email, dto.Senha);

        if (response == null)
            return Problem(
                title: "Credenciais inválidas",
                detail: "E-mail ou senha incorretos.",
                statusCode: StatusCodes.Status401Unauthorized);

        return Ok(response);
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