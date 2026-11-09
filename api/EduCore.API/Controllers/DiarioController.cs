using EduCore.API.Constants;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Diário de classe: o resumo de uma disciplina na turma para as telas do professor.
/// </summary>
[ApiController]
[Route("api/diario")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Professor}")]
public class DiarioController : ControllerBase
{
    private readonly DiarioService _diarioService;

    public DiarioController(DiarioService diarioService)
    {
        _diarioService = diarioService;
    }

    /// <summary>
    /// Bimestre vigente, pesos por bimestre (de toda a disciplina) e o que o usuário leciona nela.
    /// </summary>
    /// <response code="404">Turma ou disciplina não encontrada (ou que o professor não leciona).</response>
    [HttpGet]
    [ProducesResponseType(typeof(DiarioDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter([FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        return this.ParaResposta(await _diarioService.ObterAsync(turmaId, disciplinaId), Ok);
    }
}
