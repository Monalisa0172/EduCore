using EduCore.API.Constants;
using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Acesso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EduCore.API.Controllers;

/// <summary>
/// Dados do usuário logado. Não recebe ids: tudo é resolvido pelo token, então ninguém
/// consegue ver dados de outra pessoa trocando um número na URL.
/// </summary>
[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IAcessoAcademico _acesso;
    private readonly IAtribuicaoRepository _atribuicaoRepository;
    private readonly BoletimService _boletimService;

    public MeController(IAcessoAcademico acesso, IAtribuicaoRepository atribuicaoRepository, BoletimService boletimService)
    {
        _acesso = acesso;
        _atribuicaoRepository = atribuicaoRepository;
        _boletimService = boletimService;
    }

    /// <summary>
    /// Professor: turmas e disciplinas que leciona (atribuições ativas), do ano mais recente ao mais antigo.
    /// </summary>
    [Authorize(Roles = Roles.Professor)]
    [HttpGet("atribuicoes")]
    [ProducesResponseType(typeof(List<AtribuicaoResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MinhasAtribuicoes()
    {
        var professorId = await _acesso.ProfessorIdAsync();

        return Ok(professorId is null ? [] : await _atribuicaoRepository.GetAtivasDoProfessorAsync(professorId.Value));
    }

    /// <summary>
    /// Aluno: boletim do ano letivo (padrão: o mais recente), com os 4 bimestres e o bimestre vigente.
    /// </summary>
    /// <param name="anoLetivo">Ano letivo; vazio para o mais recente.</param>
    [Authorize(Roles = Roles.Aluno)]
    [HttpGet("boletim")]
    [ProducesResponseType(typeof(BoletimDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MeuBoletim([FromQuery] int? anoLetivo)
    {
        return this.ParaResposta(await _boletimService.MeuBoletimAsync(anoLetivo), Ok);
    }
}
