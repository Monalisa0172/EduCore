using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Chamada por aula (turma + disciplina + dia). Professor só nas disciplinas que leciona.
/// </summary>
[ApiController]
[Route("api/chamadas")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Professor}")]
public class ChamadaController : ControllerBase
{
    private readonly ChamadaService _chamadaService;

    public ChamadaController(ChamadaService chamadaService)
    {
        _chamadaService = chamadaService;
    }

    /// <summary>
    /// Lista da turma no dia, com a presença de cada aluno (vazia se a chamada ainda não foi feita).
    /// </summary>
    /// <param name="turmaId">Turma.</param>
    /// <param name="disciplinaId">Disciplina.</param>
    /// <param name="data">Dia da aula (aaaa-mm-dd).</param>
    [HttpGet]
    [ProducesResponseType(typeof(ChamadaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter([FromQuery] int turmaId, [FromQuery] int disciplinaId, [FromQuery] DateOnly data)
    {
        return this.ParaResposta(await _chamadaService.ObterAsync(turmaId, disciplinaId, data), Ok);
    }

    /// <summary>
    /// Aulas com chamada registrada (data, presentes e total), da mais recente para a mais antiga.
    /// </summary>
    [HttpGet("aulas")]
    [ProducesResponseType(typeof(List<AulaRegistradaDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarAulas([FromQuery] int turmaId, [FromQuery] int disciplinaId)
    {
        return this.ParaResposta(await _chamadaService.ListarAulasAsync(turmaId, disciplinaId), Ok);
    }

    /// <summary>
    /// Registra (ou corrige) a chamada do dia de uma vez. Refazer o mesmo dia atualiza, sem duplicar.
    /// </summary>
    /// <response code="200">Chamada atualizada.</response>
    /// <response code="400">Data futura ou fora do ano letivo, aluno fora da turma, turma ou disciplina inativa.</response>
    /// <response code="404">Turma ou disciplina não encontrada (ou que o professor não leciona).</response>
    [HttpPut]
    [ProducesResponseType(typeof(ChamadaDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarChamadaRequest request)
    {
        return this.ParaResposta(await _chamadaService.RegistrarAsync(request), Ok);
    }
}
