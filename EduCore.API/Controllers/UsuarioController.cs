namespace EduCore.API.Controllers;

using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _usuarioService;

    public UsuarioController(
        UsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    /// <summary>
    /// Inativa um usuário do sistema.
    /// </summary>
    /// <param name="id">
    /// Id do usuário
    /// </param>
    /// <response code="200">
    /// Usuário inativado com sucesso
    /// </response>
    /// <response code="404">
    /// Usuário não encontrado
    /// </response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id}/inativar")]
    public async Task<IActionResult> Inativar(int id)
    {
        var sucesso =
            await _usuarioService.InativarAsync(id);

        if (!sucesso)
            return NotFound(new
            {
                message = "Usuário não encontrado"
            });

        return Ok(new
        {
            message = "Usuário inativado com sucesso"
        });
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPatch("{id}/email")]
    public async Task<IActionResult> AtualizarEmail(
    int id,
    [FromBody] UpdateUsuarioEmailRequest request)
    {
        var atualizado =
            await _usuarioService
                .AtualizarEmailAsync(id, request.Email);

        if (!atualizado)
            return BadRequest(new
            {
                message = "Usuário não encontrado ou e-mail já cadastrado"
            });

        return Ok(new
        {
            message = "E-mail atualizado com sucesso"
        });
    }
}