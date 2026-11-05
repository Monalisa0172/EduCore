using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Avaliações por turma, disciplina e bimestre.
/// </summary>
[ApiController]
[Route("api/avaliacoes")]
[Authorize(Roles = $"{Roles.Admin},{Roles.Professor}")]
public class AvaliacaoController : ControllerBase
{
    private readonly AvaliacaoService _avaliacaoService;

    public AvaliacaoController(AvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    /// <summary>
    /// Lista as avaliações. Filtros opcionais por turma, disciplina, bimestre (1 a 4) e situação.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AvaliacaoResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar(
        [FromQuery] int? turmaId,
        [FromQuery] int? disciplinaId,
        [FromQuery] int? bimestre,
        [FromQuery] bool? ativo)
    {
        return Ok(await _avaliacaoService.ListarAsync(turmaId, disciplinaId, bimestre, ativo));
    }

    /// <summary>
    /// Busca uma avaliação pelo ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(AvaliacaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var avaliacao = await _avaliacaoService.GetByIdAsync(id);

        return avaliacao is null
            ? Problem(detail: "Avaliação não encontrada.", statusCode: StatusCodes.Status404NotFound, title: "Registro não encontrado")
            : Ok(avaliacao);
    }

    /// <summary>
    /// Cria uma avaliação. Os pesos do bimestre (turma + disciplina) somam no máximo 100.
    /// </summary>
    /// <response code="201">Avaliação criada.</response>
    /// <response code="400">Dados inválidos, disciplina sem professor na turma ou peso acima do disponível.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AvaliacaoResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreateAvaliacaoRequest request)
    {
        return this.ParaResposta(await _avaliacaoService.CriarAsync(request), avaliacao =>
            CreatedAtAction(nameof(GetById), new { id = avaliacao.Id }, avaliacao));
    }

    /// <summary>
    /// Atualiza nome, peso, data, bimestre e subdisciplina. Turma e disciplina não mudam.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(AvaliacaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] UpdateAvaliacaoRequest request)
    {
        return this.ParaResposta(await _avaliacaoService.AtualizarAsync(id, request), Ok);
    }

    /// <summary>
    /// Inativa a avaliação: as notas ficam guardadas e o peso sai da média do bimestre.
    /// </summary>
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(AvaliacaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id)
    {
        return this.ParaResposta(await _avaliacaoService.InativarAsync(id), Ok);
    }

    /// <summary>
    /// Reativa a avaliação, se o peso dela ainda couber no bimestre.
    /// </summary>
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(AvaliacaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(int id)
    {
        return this.ParaResposta(await _avaliacaoService.ReativarAsync(id), Ok);
    }

    /// <summary>
    /// Inativa a avaliação. Mantido por compatibilidade: prefira PATCH {id}/inativar.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(AvaliacaoResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(int id) => Inativar(id);
}
