using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/disciplinas")]
[Authorize(Roles = Roles.Admin)]
public class DisciplinaController : ControllerBase
{
    private readonly DisciplinaService _disciplinaService;

    public DisciplinaController(
        DisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    /// <summary>
    /// Lista todas as disciplinas (ativas e inativas) em ordem alfabética.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<DisciplinaResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _disciplinaService.GetAllAsync());
    }

    /// <summary>
    /// Busca uma disciplina por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var disciplina = await _disciplinaService.GetByIdAsync(id);

        return disciplina is null
            ? Problem(detail: "Disciplina não encontrada.", statusCode: StatusCodes.Status404NotFound, title: "Registro não encontrado")
            : Ok(disciplina);
    }

    /// <summary>
    /// Cria uma disciplina. O código é único e gravado em maiúsculas.
    /// </summary>
    /// <response code="201">Disciplina criada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Código já cadastrado.</response>
    [HttpPost]
    [ProducesResponseType(typeof(DisciplinaResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDisciplinaRequest request)
    {
        var resultado = await _disciplinaService.CreateAsync(request);

        return this.ParaResposta(resultado, disciplina =>
            CreatedAtAction(nameof(GetById), new { id = disciplina.Id }, disciplina));
    }

    /// <summary>
    /// Atualiza uma disciplina.
    /// </summary>
    /// <response code="200">Disciplina atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Disciplina não encontrada.</response>
    /// <response code="409">Código já usado por outra disciplina.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(DisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateDisciplinaRequest request)
    {
        var resultado = await _disciplinaService.UpdateAsync(id, request);

        return this.ParaResposta(resultado, Ok);
    }

    /// <summary>
    /// Inativa uma disciplina (o histórico é mantido).
    /// </summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(DisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id)
    {
        return this.ParaResposta(await _disciplinaService.InativarAsync(id), Ok);
    }

    /// <summary>
    /// Reativa uma disciplina inativa.
    /// </summary>
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(DisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(int id)
    {
        return this.ParaResposta(await _disciplinaService.ReativarAsync(id), Ok);
    }

    /// <summary>
    /// Inativa uma disciplina. Mantido por compatibilidade: prefira PATCH {id}/inativar.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(DisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(int id) => Inativar(id);
}
