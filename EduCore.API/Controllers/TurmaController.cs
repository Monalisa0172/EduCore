using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TurmaController : ControllerBase
{
    private readonly TurmaService _turmaService;

    public TurmaController(
        TurmaService turmaService)
    {
        _turmaService = turmaService;
    }

    /// <summary>
    /// Retorna todas as turmas cadastradas
    /// </summary>
    /// <returns>Lista de turmas</returns>
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result =
            await _turmaService.GetAllAsync();

        return Ok(result);
    }

    /// <summary>
    /// Retorna uma turma pelo ID
    /// </summary>
    /// <param name="id">Id da turma</param>
    /// <returns>Dados da turma</returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        int id)
    {
        var result =
            await _turmaService.GetByIdAsync(id);

        if (result == null)
            return NotFound(new
            {
                message =
                    "Turma não encontrada"
            });

        return Ok(result);
    }

    /// <summary>
    /// Cria uma nova turma
    /// </summary>
    /// <param name="request">
    /// Dados da turma
    /// </param>
    /// <returns>Turma criada</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateTurmaRequest request)
    {
        var result =
            await _turmaService.CreateAsync(request);

        return Ok(result);
    }

    /// <summary>
    /// Inativa uma turma
    /// </summary>
    /// <param name="id">
    /// Id da turma
    /// </param>
    /// <returns>
    /// Resultado da operação
    /// </returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Inativar(
        int id)
    {
        var result =
            await _turmaService.InativarAsync(id);

        if (!result)
            return NotFound(new
            {
                message =
                    "Turma não encontrada"
            });

        return Ok(new
        {
            message =
                "Turma inativada com sucesso"
        });
    }
}