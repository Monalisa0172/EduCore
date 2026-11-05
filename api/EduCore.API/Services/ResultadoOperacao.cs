namespace EduCore.API.Services;

public enum FalhaOperacao
{
    NaoEncontrado,
    Conflito,
    Invalido
}

/// <summary>
/// Resultado de uma operação de escrita: o valor em caso de sucesso ou o motivo da falha.
/// O controller traduz cada falha no status HTTP adequado (404, 409 ou 400).
/// </summary>
public sealed class ResultadoOperacao<T>
{
    private ResultadoOperacao(T? valor, FalhaOperacao? falha, string? campo, string? mensagem)
    {
        Valor = valor;
        Falha = falha;
        Campo = campo;
        Mensagem = mensagem;
    }

    public T? Valor { get; }

    public FalhaOperacao? Falha { get; }

    public bool Sucesso => Falha is null;

    /// <summary>Campo do request que causou a falha (nulo quando não se aplica a um campo).</summary>
    public string? Campo { get; }

    public string? Mensagem { get; }

    public static ResultadoOperacao<T> Ok(T valor) => new(valor, null, null, null);

    public static ResultadoOperacao<T> NaoEncontrado(string mensagem)
        => new(default, FalhaOperacao.NaoEncontrado, null, mensagem);

    public static ResultadoOperacao<T> Conflito(string campo, string mensagem)
        => new(default, FalhaOperacao.Conflito, campo, mensagem);

    public static ResultadoOperacao<T> Invalido(string campo, string mensagem)
        => new(default, FalhaOperacao.Invalido, campo, mensagem);
}
