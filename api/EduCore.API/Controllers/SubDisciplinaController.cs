using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/subdisciplinas")]
[Authorize(Roles = Roles.Admin)]
public class SubDisciplinaController : ControllerBase
{
    private readonly SubDisciplinaService
        _subDisciplinaService;

    public SubDisciplinaController(
        SubDisciplinaService subDisciplinaService)
    {
        _subDisciplinaService = subDisciplinaService;
    }

    /// <summary>
    /// Lista todas as subdisciplinas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SubDisciplinaResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _subDisciplinaService.GetAllAsync());
    }

    /// <summary>
    /// Busca uma subdisciplina por ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SubDisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var subDisciplina = await _subDisciplinaService.GetByIdAsync(id);

        return subDisciplina is null
            ? Problem(detail: "Subdisciplina não encontrada.", statusCode: StatusCodes.Status404NotFound, title: "Registro não encontrado")
            : Ok(subDisciplina);
    }

    /// <summary>
    /// Lista as subdisciplinas (ativas e inativas) de uma disciplina, em ordem alfabética.
    /// </summary>
    [HttpGet("disciplina/{disciplinaId:int}")]
    [ProducesResponseType(typeof(List<SubDisciplinaResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDisciplina(int disciplinaId)
    {
        return Ok(await _subDisciplinaService.GetByDisciplinaAsync(disciplinaId));
    }

    /// <summary>
    /// Cria uma subdisciplina numa disciplina ativa.
    /// </summary>
    /// <response code="201">Subdisciplina criada.</response>
    /// <response code="400">Dados inválidos ou disciplina inativa.</response>
    /// <response code="409">Já existe uma subdisciplina com este nome na disciplina.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SubDisciplinaResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSubDisciplinaRequest request)
    {
        var resultado = await _subDisciplinaService.CreateAsync(request);

        return this.ParaResposta(resultado, subDisciplina =>
            CreatedAtAction(nameof(GetById), new { id = subDisciplina.Id }, subDisciplina));
    }

    /// <summary>
    /// Atualiza uma subdisciplina.
    /// </summary>
    /// <response code="200">Subdisciplina atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Subdisciplina não encontrada.</response>
    /// <response code="409">Nome já usado nesta disciplina.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SubDisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateSubDisciplinaRequest request)
    {
        return this.ParaResposta(await _subDisciplinaService.UpdateAsync(id, request), Ok);
    }

    /// <summary>
    /// Inativa uma subdisciplina (o histórico é mantido).
    /// </summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(SubDisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id)
    {
        return this.ParaResposta(await _subDisciplinaService.InativarAsync(id), Ok);
    }

    /// <summary>
    /// Reativa uma subdisciplina. A disciplina precisa estar ativa.
    /// </summary>
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(SubDisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(int id)
    {
        return this.ParaResposta(await _subDisciplinaService.ReativarAsync(id), Ok);
    }

    /// <summary>
    /// Inativa uma subdisciplina. Mantido por compatibilidade: prefira PATCH {id}/inativar.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(SubDisciplinaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(int id) => Inativar(id);
}
