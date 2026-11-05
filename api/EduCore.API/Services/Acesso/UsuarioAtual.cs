using System.Security.Claims;
using EduCore.API.Constants;

namespace EduCore.API.Services.Acesso;

/// <summary>
/// Lê o usuário do token JWT da requisição atual.
/// </summary>
public class UsuarioAtual : IUsuarioAtual
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsuarioAtual(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal Principal
        => _httpContextAccessor.HttpContext?.User
            ?? throw new InvalidOperationException("Não há requisição autenticada.");

    public int UsuarioId
        => int.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : throw new InvalidOperationException("Token sem identificador de usuário.");

    public bool EhAdmin => Principal.IsInRole(Roles.Admin);

    public bool EhProfessor => Principal.IsInRole(Roles.Professor);

    public bool EhAluno => Principal.IsInRole(Roles.Aluno);
}
