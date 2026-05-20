using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/professores")]
public class ProfessorController : ControllerBase
{
    private readonly ProfessorService _professorService;

    public ProfessorController(ProfessorService professorService)
    {
        _professorService = professorService;
    }

    /// <summary>
    /// Retorna todos os professores cadastrados
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var professores = await _professorService.GetAllAsync();

        return Ok(professores);
    }

    /// <summary>
    /// Retorna um professor pelo ID
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var professor = await _professorService.GetByIdAsync(id);

        if (professor == null)
            return NotFound(new
            {
                message = "Professor não encontrado"
            });

        return Ok(professor);
    }

    /// <summary>
    /// Cria um novo professor
    /// </summary>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProfessorRequest request)
    {
        var professor = await _professorService.CreateAsync(request);

        if (professor == null)
            return BadRequest(new
            {
                message = "E-mail já cadastrado"
            });

        return Ok(professor);
    }
}