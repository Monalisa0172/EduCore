using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Controller responsável pelos vínculos
/// entre professores e turmas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProfessorTurmaController
    : ControllerBase
{
    private readonly ProfessorTurmaService
        _service;

    public ProfessorTurmaController(
        ProfessorTurmaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retorna todos os vínculos
    /// entre professores e turmas.
    /// </summary>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _service.GetAllAsync();

        return Ok(result);
    }

    /// <summary>
    /// Retorna um vínculo específico.
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult>
        GetById(int id)
    {
        var result =
            await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound(new
            {
                message =
                    "Vínculo não encontrado"
            });

        return Ok(result);
    }

    /// <summary>
    /// Cria um vínculo entre
    /// professor e turma.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult>
        Create(
            [FromBody]
            CreateProfessorTurmaRequest request)
    {
        var result =
            await _service.CreateAsync(
                request);

        return Ok(result);
    }

    /// <summary>
    /// Inativa um vínculo.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult>
        Inativar(int id)
    {
        var result =
            await _service.InativarAsync(id);

        if (!result)
            return NotFound(new
            {
                message =
                    "Vínculo não encontrado"
            });

        return Ok(new
        {
            message =
                "Vínculo inativado com sucesso"
        });
    }
}