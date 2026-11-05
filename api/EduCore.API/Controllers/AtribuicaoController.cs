using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Atribuição de professores às disciplinas (ou subdisciplinas) de cada turma.
/// </summary>
[ApiController]
[Route("api/atribuicoes")]
[Authorize(Roles = Roles.Admin)]
public class AtribuicaoController : ControllerBase
{
    private readonly AtribuicaoService _atribuicaoService;

    public AtribuicaoController(AtribuicaoService atribuicaoService)
    {
        _atribuicaoService = atribuicaoService;
    }

    /// <summary>
    /// Lista as atribuições. Filtros opcionais por turma, professor, disciplina e situação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AtribuicaoResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int? turmaId,
        [FromQuery] int? professorId,
        [FromQuery] int? disciplinaId,
        [FromQuery] bool? ativo)
    {
        return Ok(await _atribuicaoService.ListarAsync(turmaId, professorId, disciplinaId, ativo));
    }

    /// <summary>
    /// Busca uma atribuição pelo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AtribuicaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var atribuicao = await _atribuicaoService.GetByIdAsync(id);

        return atribuicao is null
            ? Problem(detail: "Atribuição não encontrada.", statusCode: StatusCodes.Status404NotFound, title: "Registro não encontrado")
            : Ok(atribuicao);
    }

    /// <summary>
    /// Atribui um professor a uma disciplina (inteira ou por subdisciplina) de uma turma.
    /// </summary>
    /// <response code="201">Atribuição criada.</response>
    /// <response code="400">Turma, disciplina, subdisciplina ou professor inexistente ou inativo.</response>
    /// <response code="409">Já existe professor para esse item, ou a disciplina já está atribuída de outra forma.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AtribuicaoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] AtribuicaoRequest request)
    {
        return this.ParaResposta(await _atribuicaoService.CriarAsync(request), atribuicao =>
            CreatedAtAction(nameof(GetById), new { id = atribuicao.Id }, atribuicao));
    }

    /// <summary>
    /// Troca o professor da atribuição (substituição), mantendo turma e disciplina.
    /// </summary>
    [HttpPatch("{id:int}/professor")]
    [ProducesResponseType(typeof(AtribuicaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TrocarProfessor(int id, [FromBody] TrocarProfessorRequest request)
    {
        return this.ParaResposta(await _atribuicaoService.TrocarProfessorAsync(id, request), Ok);
    }

    /// <summary>
    /// Encerra a atribuição; o histórico de quem lecionou é mantido.
    /// </summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(AtribuicaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id)
    {
        return this.ParaResposta(await _atribuicaoService.InativarAsync(id), Ok);
    }

    /// <summary>
    /// Reativa uma atribuição encerrada (se não houver conflito com as atuais).
    /// </summary>
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(AtribuicaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reativar(int id)
    {
        return this.ParaResposta(await _atribuicaoService.ReativarAsync(id), Ok);
    }
}
