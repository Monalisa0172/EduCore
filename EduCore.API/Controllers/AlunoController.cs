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

    public AlunoController(AlunoService alunoService)
    {
        _alunoService = alunoService;
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
    /// Retorna um aluno pelo ID.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
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