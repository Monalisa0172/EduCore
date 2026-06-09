using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/subdisciplinas")]
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
    /// Lista todas as subdisciplinas
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var subDisciplinas =
            await _subDisciplinaService
                .GetAllAsync();

        return Ok(subDisciplinas);
    }

    /// <summary>
    /// Busca uma subdisciplina por ID
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var subDisciplina =
            await _subDisciplinaService
                .GetByIdAsync(id);

        if (subDisciplina == null)
        {
            return NotFound(new
            {
                message =
                    "Subdisciplina não encontrada"
            });
        }

        return Ok(subDisciplina);
    }

    /// <summary>
    /// Busca subdisciplinas por disciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("disciplina/{disciplinaId}")]
    public async Task<IActionResult>
        GetByDisciplina(int disciplinaId)
    {
        var subDisciplinas =
            await _subDisciplinaService
                .GetByDisciplinaAsync(disciplinaId);

        return Ok(subDisciplinas);
    }

    /// <summary>
    /// Cria uma nova subdisciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateSubDisciplinaRequest request)
    {
        var subDisciplina =
            await _subDisciplinaService
                .CreateAsync(request);

        if (subDisciplina == null)
        {
            return BadRequest(new
            {
                message =
                    "Disciplina inválida ou subdisciplina já cadastrada"
            });
        }

        return Ok(subDisciplina);
    }

    /// <summary>
    /// Atualiza uma subdisciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody]
        CreateSubDisciplinaRequest request)
    {
        var atualizado =
            await _subDisciplinaService
                .UpdateAsync(id, request);

        if (!atualizado)
        {
            return NotFound(new
            {
                message =
                    "Subdisciplina não encontrada"
            });
        }

        return Ok(new
        {
            message =
                "Subdisciplina atualizada com sucesso"
        });
    }

    /// <summary>
    /// Inativa uma subdisciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido =
            await _subDisciplinaService
                .DeleteAsync(id);

        if (!removido)
        {
            return NotFound(new
            {
                message =
                    "Subdisciplina não encontrada"
            });
        }

        return Ok(new
        {
            message =
                "Subdisciplina inativada com sucesso"
        });
    }
}