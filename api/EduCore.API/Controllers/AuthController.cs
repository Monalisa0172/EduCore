namespace EduCore.API.Controllers;

using EduCore.API.Constants;
using EduCore.API.DTOs;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    /// <summary>Limite de tentativas por IP para login (ver Program.cs).</summary>
    public const string PoliticaLogin = "login";

    /// <summary>Limite de tentativas por IP para esqueci/redefinir senha (ver Program.cs).</summary>
    public const string PoliticaSenha = "senha";

    private readonly AuthService _authService;
    private readonly RedefinicaoSenhaService _redefinicaoSenhaService;
    private readonly FilaRedefinicaoSenha _filaRedefinicao;

    public AuthController(AuthService authService, RedefinicaoSenhaService redefinicaoSenhaService, FilaRedefinicaoSenha filaRedefinicao)
    {
        _authService = authService;
        _redefinicaoSenhaService = redefinicaoSenhaService;
        _filaRedefinicao = filaRedefinicao;
    }

    /// <summary>
    /// Realiza autenticação do usuário e retorna um token JWT.
    /// </summary>
    /// <param name="dto">Credenciais de acesso do usuário.</param>
    /// <returns>Token JWT para autenticação.</returns>
    /// <response code="200">Login realizado com sucesso.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="401">Credenciais inválidas.</response>
    /// <response code="429">Muitas tentativas: aguarde (cabeçalho Retry-After).</response>
    [HttpPost("login")]
    [EnableRateLimiting(PoliticaLogin)]
    [ProducesResponseType(typeof(LoginResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
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
    /// Esqueci minha senha: envia um link de redefinição para o e-mail, se houver conta ativa com ele.
    /// </summary>
    /// <remarks>
    /// A resposta é sempre a mesma (e no mesmo tempo), exista ou não a conta: não revela e-mails cadastrados.
    /// O link vale por 30 minutos e só pode ser usado uma vez.
    /// </remarks>
    /// <response code="202">Pedido recebido.</response>
    /// <response code="400">E-mail inválido.</response>
    /// <response code="429">Muitas tentativas: aguarde (cabeçalho Retry-After).</response>
    [HttpPost("esqueci-senha")]
    [EnableRateLimiting(PoliticaSenha)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public IActionResult EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        _filaRedefinicao.Enfileirar(request.Email);

        return Accepted(new
        {
            message = "Se houver uma conta com este e-mail, enviamos um link para criar uma nova senha."
        });
    }

    /// <summary>
    /// Confere se o link de redefinição ainda vale (para a tela avisar antes de a pessoa digitar a senha).
    /// </summary>
    /// <response code="200">{ valido: true|false }</response>
    [HttpGet("redefinir-senha/validar")]
    [EnableRateLimiting(PoliticaSenha)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ValidarLink([FromQuery] string? token)
    {
        var valido = !string.IsNullOrWhiteSpace(token) && token.Length <= 100 && await _redefinicaoSenhaService.TokenValidoAsync(token);

        return Ok(new { valido });
    }

    /// <summary>
    /// Define a nova senha a partir do link enviado por e-mail. O link deixa de valer em seguida.
    /// </summary>
    /// <response code="200">Senha redefinida.</response>
    /// <response code="400">Link inválido, usado ou expirado; ou senha fora da regra (8 a 100 caracteres).</response>
    /// <response code="429">Muitas tentativas: aguarde (cabeçalho Retry-After).</response>
    [HttpPost("redefinir-senha")]
    [EnableRateLimiting(PoliticaSenha)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request)
    {
        return this.ParaResposta(
            await _redefinicaoSenhaService.RedefinirAsync(request),
            _ => Ok(new { message = "Senha redefinida. Entre com a nova senha." }));
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
