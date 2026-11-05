using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Notas de uma avaliação (grade da turma). Professor só nas avaliações do que leciona.
/// </summary>
[ApiController]
[Route("api/avaliacoes/{avaliacaoId:int}/notas")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Professor}")]
public class NotaController : ControllerBase
{
    private readonly NotaService _notaService;

    public NotaController(NotaService notaService)
    {
        _notaService = notaService;
    }

    /// <summary>
    /// Todos os alunos da turma com a nota de cada um (vazia se ainda não lançada).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(NotasDaAvaliacaoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Obter(int avaliacaoId)
    {
        return this.ParaResposta(await _notaService.ObterAsync(avaliacaoId), Ok);
    }

    /// <summary>
    /// Lança ou corrige as notas de uma vez (tudo ou nada). Valor vazio remove a nota do aluno.
    /// </summary>
    /// <response code="200">Grade atualizada.</response>
    /// <response code="400">Nota fora de 0 a 10, aluno fora da turma, avaliação ou turma inativa.</response>
    /// <response code="404">Avaliação não encontrada (ou de uma turma/disciplina que o professor não leciona).</response>
    [HttpPut]
    [ProducesResponseType(typeof(NotasDaAvaliacaoDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Lancar(int avaliacaoId, [FromBody] LancarNotasRequest request)
    {
        return this.ParaResposta(await _notaService.LancarAsync(avaliacaoId, request), Ok);
    }
}
