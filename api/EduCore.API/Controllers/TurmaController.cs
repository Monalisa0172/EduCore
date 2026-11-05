using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/turmas")]
public class TurmaController : ControllerBase
{
    private readonly TurmaService _turmaService;

    public TurmaController(
        TurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    /// <summary>
    /// Lista as turmas (ativas e inativas), do ano letivo mais recente para o mais antigo.
    /// </summary>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(List<TurmaResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _turmaService.GetAllAsync());
    }

    /// <summary>
    /// Busca uma turma pelo ID.
    /// </summary>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TurmaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var turma = await _turmaService.GetByIdAsync(id);

        return turma is null ? TurmaNaoEncontrada() : Ok(turma);
    }

    /// <summary>
    /// Lista os alunos matriculados na turma, em ordem alfabética.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}/alunos")]
    [ProducesResponseType(typeof(List<TurmaAlunoDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAlunos(int id)
    {
        var alunos = await _turmaService.GetAlunosAsync(id);

        return alunos is null ? TurmaNaoEncontrada() : Ok(alunos);
    }

    /// <summary>
    /// Cria uma turma. O nome é único dentro do ano letivo.
    /// </summary>
    /// <response code="201">Turma criada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">Já existe a turma no mesmo ano letivo.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(TurmaResponseDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateTurmaRequest request)
    {
        var resultado = await _turmaService.CreateAsync(request);

        return this.ParaResposta(resultado, turma =>
            CreatedAtAction(nameof(GetById), new { id = turma.Id }, turma));
    }

    /// <summary>
    /// Atualiza nome e ano letivo da turma.
    /// </summary>
    /// <response code="200">Turma atualizada.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Turma não encontrada.</response>
    /// <response code="409">Já existe outra turma com o mesmo nome no ano letivo.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TurmaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateTurmaRequest request)
    {
        return this.ParaResposta(await _turmaService.UpdateAsync(id, request), Ok);
    }

    /// <summary>
    /// Inativa a turma: as matrículas ficam no histórico e ela deixa de aceitar alunos.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/inativar")]
    [ProducesResponseType(typeof(TurmaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Inativar(int id)
    {
        return this.ParaResposta(await _turmaService.InativarAsync(id), Ok);
    }

    /// <summary>
    /// Reativa uma turma inativa.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id:int}/reativar")]
    [ProducesResponseType(typeof(TurmaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reativar(int id)
    {
        return this.ParaResposta(await _turmaService.ReativarAsync(id), Ok);
    }

    /// <summary>
    /// Inativa a turma. Mantido por compatibilidade: prefira PATCH {id}/inativar.
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(TurmaResponseDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public Task<IActionResult> Delete(int id) => Inativar(id);

    private ObjectResult TurmaNaoEncontrada()
        => Problem(detail: "Turma não encontrada.", statusCode: StatusCodes.Status404NotFound, title: "Registro não encontrado");
}
