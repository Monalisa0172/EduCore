namespace EduCore.API.Services.Acesso;

/// <summary>
/// Quem está fazendo a requisição (lido do token). Abstraído para os services serem testáveis.
/// </summary>
public interface IUsuarioAtual
{
    int UsuarioId { get; }

    bool EhAdmin { get; }

    bool EhProfessor { get; }

    bool EhAluno { get; }
}
