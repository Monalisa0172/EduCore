namespace EduCore.API.Controllers;

using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/alunos")]
public class AlunoController : ControllerBase
{
    private readonly AlunoService _alunoService;
    private readonly BoletimService _boletimService;

    public AlunoController(AlunoService alunoService, BoletimService boletimService)
    {
        _alunoService = alunoService;
        _boletimService = boletimService;
    }

    /// <summary>
    /// Retorna todos os alunos cadastrados.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var alunos = await _alunoService.GetAllAsync();

        return Ok(alunos);
    }

    /// <summary>
    /// Retorna um aluno pelo ID. Só admin: o aluno consulta os próprios dados em api/me.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var aluno = await _alunoService.GetByIdAsync(id);

        if (aluno == null)
            return NotFound(new
            {
                message = "Aluno não encontrado"
            });

        return Ok(aluno);
    }

    /// <summary>
    /// Boletim do aluno no ano letivo (padrão: o mais recente com matrícula).
    /// </summary>
    /// <param name="id">Id do aluno.</param>
    /// <param name="anoLetivo">Ano letivo; vazio para o mais recente.</param>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}/boletim")]
    [ProducesResponseType(typeof(DTOs.Response.BoletimDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Boletim(int id, [FromQuery] int? anoLetivo)
    {
        return this.ParaResposta(await _boletimService.ObterAsync(id, anoLetivo), Ok);
    }

    /// <summary>
    /// Cria um novo aluno no sistema.
    /// </summary>
    /// <remarks>
    /// Endpoint responsável por:
    /// - criar usuário
    /// - criptografar senha
    /// - vincular aluno
    /// </remarks>
    /// <response code="200">
    /// Aluno criado com sucesso
    /// </response>
    /// <response code="400">
    /// E-mail já cadastrado
    /// </response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAlunoRequest request)
    {
        var aluno = await _alunoService.CreateAsync(request);

        if (aluno == null)
            return BadRequest(new
            {
                message = "E-mail já cadastrado"
            });

        return Ok(aluno);
    }
}