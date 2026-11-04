namespace EduCore.API.Controllers;

using EduCore.API.Constants;
using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Enums;
using EduCore.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly UsuarioService _usuarioService;
    private readonly UsuarioCadastroService _cadastroService;

    public UsuarioController(
        UsuarioService usuarioService,
        UsuarioCadastroService cadastroService)
    {
        _usuarioService = usuarioService;
        _cadastroService = cadastroService;
    }

    /// <summary>
    /// Retorna um aluno ou professor para edição.
    /// </summary>
    /// <response code="200">Dados do usuário, com turmas ativas (aluno) ou dados funcionais (professor).</response>
    /// <response code="404">Usuário não encontrado (ou não é aluno/professor).</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDetalheDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _cadastroService.GetDetalheAsync(id);

        if (usuario is null)
            return NotFound(new
            {
                message = "Usuário não encontrado"
            });

        return Ok(usuario);
    }

    /// <summary>
    /// Cadastra um aluno (com turmas) ou um professor (com dados funcionais) numa única transação.
    /// </summary>
    /// <response code="201">Usuário criado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="409">E-mail já cadastrado.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ProducesResponseType(typeof(UsuarioDetalheDTO), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioRequest request)
    {
        var resultado = await _cadastroService.CreateAsync(request);

        return ParaResposta(resultado, usuario =>
            CreatedAtAction(nameof(GetById), new { id = usuario.UsuarioId }, usuario));
    }

    /// <summary>
    /// Atualiza um aluno ou professor. Nova senha é opcional.
    /// </summary>
    /// <response code="200">Usuário atualizado.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Usuário não encontrado.</response>
    /// <response code="409">E-mail já cadastrado para outro usuário.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(UsuarioDetalheDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioRequest request)
    {
        var resultado = await _cadastroService.UpdateAsync(id, request);

        return ParaResposta(resultado, Ok);
    }

    private IActionResult ParaResposta(
        UsuarioCadastroResultado resultado,
        Func<UsuarioDetalheDTO, IActionResult> sucesso)
    {
        switch (resultado.Falha)
        {
            case null:
                return sucesso(resultado.Usuario!);

            case FalhaCadastro.NaoEncontrado:
                return NotFound(new
                {
                    message = "Usuário não encontrado"
                });

            case FalhaCadastro.EmailEmUso:
                // 409 com o erro no campo Email: o front destaca o campo certo.
                ModelState.AddModelError(nameof(SalvarUsuarioRequest.Email), "Já existe um usuário com este e-mail.");
                return ValidationProblem(
                    detail: "Já existe um usuário com este e-mail.",
                    statusCode: StatusCodes.Status409Conflict,
                    title: "E-mail já cadastrado",
                    modelStateDictionary: ModelState);

            default:
                ModelState.AddModelError(resultado.Campo!, resultado.Mensagem!);
                return ValidationProblem(ModelState);
        }
    }

    /// <summary>
    /// Lista alunos e professores ativos em ordem alfabética.
    /// </summary>
    /// <param name="tipo">Opcional: Aluno ou Professor. Sem valor, retorna os dois.</param>
    /// <response code="200">Lista de usuários ativos.</response>
    /// <response code="400">Tipo não suportado.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("ativos")]
    [ProducesResponseType(typeof(List<UsuarioListItemDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAtivos([FromQuery] TipoUsuarioEnum? tipo)
    {
        if (tipo is not null && !UsuarioService.TipoListavel(tipo.Value))
        {
            ModelState.AddModelError(nameof(tipo), "Informe Aluno ou Professor.");
            return ValidationProblem(ModelState);
        }

        return Ok(await _usuarioService.GetAtivosAsync(tipo));
    }

    /// <summary>
    /// Retorna a quantidade de professores e alunos ativos.
    /// </summary>
    /// <response code="200">Totais por perfil.</response>
    [Authorize(Roles = Roles.Admin)]
    [HttpGet("resumo")]
    [ProducesResponseType(typeof(UsuariosResumoDTO), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetResumo()
    {
        return Ok(await _usuarioService.GetResumoAsync());
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