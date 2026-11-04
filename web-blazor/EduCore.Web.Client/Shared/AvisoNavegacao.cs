namespace EduCore.Web.Client.Shared;

public sealed record Aviso(string Titulo, string Mensagem);

/// <summary>
/// Guarda uma mensagem de sucesso para a próxima página exibir (ex.: salvar e voltar ao painel).
/// Scoped: no WebAssembly vive enquanto a aba estiver aberta; o aviso é lido uma única vez.
/// </summary>
public sealed class AvisoNavegacao
{
    private Aviso? _pendente;

    public void Publicar(string titulo, string mensagem) => _pendente = new Aviso(titulo, mensagem);

    public Aviso? Consumir()
    {
        var aviso = _pendente;
        _pendente = null;
        return aviso;
    }
}
