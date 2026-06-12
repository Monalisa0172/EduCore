using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

[ApiController]
[Route("api/avaliacoes")]
public class AvaliacaoController : ControllerBase
{
    private readonly AvaliacaoService _avaliacaoService;

    public AvaliacaoController(
        AvaliacaoService avaliacaoService)
    {
        _avaliacaoService = avaliacaoService;
    }

    /// <summary>
    /// Retorna todas as avaliações cadastradas
    /// </summary>
    [Authorize(
        Roles = $"{Roles.Admin},{Roles.Professor}")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var avaliacoes =
            await _avaliacaoService.GetAllAsync();

        return Ok(avaliacoes);
    }

    /// <summary>
    /// Retorna uma avaliação pelo ID
    /// </summary>
    [Authorize(
        Roles = $"{Roles.Admin},{Roles.Professor}")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var avaliacao =
            await _avaliacaoService.GetByIdAsync(id);

        if (avaliacao == null)
        {
            return NotFound(new
            {
                message = "Avaliação não encontrada"
            });
        }

        return Ok(avaliacao);
    }

    /// <summary>
    /// Retorna todas as avaliações de uma disciplina
    /// </summary>
    [Authorize(
        Roles = $"{Roles.Admin},{Roles.Professor}")]
    [HttpGet("disciplina/{disciplinaId}")]
    public async Task<IActionResult> GetByDisciplina(
        int disciplinaId)
    {
        var avaliacoes =
            await _avaliacaoService
                .GetByDisciplinaAsync(disciplinaId);

        return Ok(avaliacoes);
    }

    /// <summary>
    /// Cria uma nova avaliação
    /// </summary>
    [Authorize(
        Roles = $"{Roles.Admin},{Roles.Professor}")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAvaliacaoRequest request)
    {
        var avaliacao =
            await _avaliacaoService.CreateAsync(request);

        if (avaliacao == null)
        {
            return BadRequest(new
            {
                message =
                    "Disciplina não encontrada ou dados inválidos"
            });
        }

        return Ok(avaliacao);
    }

    /// <summary>
    /// Atualiza uma avaliação existente
    /// </summary>
    [Authorize(
        Roles = $"{Roles.Admin},{Roles.Professor}")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateAvaliacaoRequest request)
    {
        var atualizado =
            await _avaliacaoService
                .UpdateAsync(id, request);

        if (!atualizado)
        {
            return NotFound(new
            {
                message = "Avaliação não encontrada"
            });
        }

        return Ok(new
        {
            message =
                "Avaliação atualizada com sucesso"
        });
    }

    /// <summary>
    /// Desativa uma avaliação
    /// </summary>
    [Authorize(
        Roles = $"{Roles.Admin},{Roles.Professor}")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido =
            await _avaliacaoService.DeleteAsync(id);

        if (!removido)
        {
            return NotFound(new
            {
                message = "Avaliação não encontrada"
            });
        }

        return Ok(new
        {
            message =
                "Avaliação removida com sucesso"
        });
    }
}