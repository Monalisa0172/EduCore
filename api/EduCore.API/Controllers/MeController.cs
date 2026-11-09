using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Acesso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

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
    private readonly AlteracaoSenhaService _alteracaoSenha;

    public MeController(
        IAcessoAcademico acesso,
        IAtribuicaoRepository atribuicaoRepository,
        BoletimService boletimService,
        AlteracaoSenhaService alteracaoSenha)
    {
        _acesso = acesso;
        _atribuicaoRepository = atribuicaoRepository;
        _boletimService = boletimService;
        _alteracaoSenha = alteracaoSenha;
    }

    /// <summary>
    /// Troca a senha do usuário logado (qualquer perfil). Exige a senha atual.
    /// Mesmo limite de tentativas do login: impede adivinhar a senha atual por força bruta.
    /// </summary>
    [HttpPut("senha")]
    [EnableRateLimiting(AuthController.PoliticaLogin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaRequest request)
    {
        return this.ParaResposta(await _alteracaoSenha.AlterarAsync(request), _ => NoContent());
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
