using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/disciplinas")]
public class DisciplinaController : ControllerBase
{
    private readonly DisciplinaService _disciplinaService;

    public DisciplinaController(
        DisciplinaService disciplinaService)
    {
        _disciplinaService = disciplinaService;
    }

    /// <summary>
    /// Lista todas as disciplinas
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var disciplinas = await _disciplinaService
            .GetAllAsync();

        return Ok(disciplinas);
    }

    /// <summary>
    /// Busca uma disciplina por ID
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var disciplina = await _disciplinaService
            .GetByIdAsync(id);

        if (disciplina == null)
        {
            return NotFound(new
            {
                message = "Disciplina não encontrada"
            });
        }

        return Ok(disciplina);
    }

    /// <summary>
    /// Cria uma nova disciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDisciplinaRequest request)
    {
        var disciplina = await _disciplinaService
            .CreateAsync(request);

        if (disciplina == null)
        {
            return BadRequest(new
            {
                message = "Código da disciplina já cadastrado"
            });
        }

        return Ok(disciplina);
    }

    /// <summary>
    /// Atualiza uma disciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] CreateDisciplinaRequest request)
    {
        var atualizado = await _disciplinaService
            .UpdateAsync(id, request);

        if (!atualizado)
        {
            return NotFound(new
            {
                message = "Disciplina não encontrada"
            });
        }

        return Ok(new
        {
            message = "Disciplina atualizada com sucesso"
        });
    }

    /// <summary>
    /// Inativa uma disciplina
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _disciplinaService
            .DeleteAsync(id);

        if (!removido)
        {
            return NotFound(new
            {
                message = "Disciplina não encontrada"
            });
        }

        return Ok(new
        {
            message = "Disciplina inativada com sucesso"
        });
    }
}