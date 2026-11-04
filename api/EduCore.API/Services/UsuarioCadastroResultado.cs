using EduCore.API.DTOs.Response;

namespace EduCore.API.Services;

public enum FalhaCadastro
{
    NaoEncontrado,
    EmailEmUso,
    Invalido
}

/// <summary>
/// Resultado de criar/editar usuário: o controller traduz cada falha no status HTTP adequado.
/// </summary>
public sealed class UsuarioCadastroResultado
{
    private UsuarioCadastroResultado(
        UsuarioDetalheDTO? usuario,
        FalhaCadastro? falha,
        string? campo = null,
        string? mensagem = null)
    {
        Usuario = usuario;
        Falha = falha;
        Campo = campo;
        Mensagem = mensagem;
    }

    public UsuarioDetalheDTO? Usuario { get; }

    public FalhaCadastro? Falha { get; }

    /// <summary>
    /// Campo do request que causou a falha de validação.
    /// </summary>
    public string? Campo { get; }

    public string? Mensagem { get; }

    public static UsuarioCadastroResultado Sucesso(UsuarioDetalheDTO usuario) => new(usuario, null);

    public static UsuarioCadastroResultado NaoEncontrado() => new(null, FalhaCadastro.NaoEncontrado);

    public static UsuarioCadastroResultado EmailEmUso() => new(null, FalhaCadastro.EmailEmUso);

    public static UsuarioCadastroResultado Invalido(string campo, string mensagem)
        => new(null, FalhaCadastro.Invalido, campo, mensagem);
}
