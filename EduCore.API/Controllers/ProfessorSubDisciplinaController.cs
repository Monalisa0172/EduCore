using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessorSubDisciplinaController
    : ControllerBase
{
    private readonly ProfessorSubDisciplinaService
        _service;

    public ProfessorSubDisciplinaController(
        ProfessorSubDisciplinaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lista todos os vínculos entre professores e subdisciplinas
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();

        return Ok(result);
    }

    /// <summary>
    /// Busca um vínculo de professor e subdisciplina por ID
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(new
            {
                message = "Vínculo não encontrado"
            });

        return Ok(result);
    }

    /// <summary>
    /// Cria um vínculo entre professor e subdisciplina
    /// </summary>
   [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateProfessorSubDisciplinaRequest request)
    {
        var result =
            await _service.CreateAsync(request);

        if (result == null)
            return BadRequest(new
            {
                message =
                    "Professor/SubDisciplina inválido ou vínculo já existente"
            });

        return Ok(result);
    }

    /// <summary>
    /// Inativa o vínculo entre professor e subdisciplina
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Inativar(int id)
    {
        var result =
            await _service.InativarAsync(id);

        if (!result)
            return NotFound(new
            {
                message = "Vínculo não encontrado"
            });

        return Ok(new
        {
            message =
                "Vínculo inativado com sucesso"
        });
    }
}