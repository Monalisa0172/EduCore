namespace EduCore.Web.Client.Auth.Services;

/// <summary>
/// Leva o e-mail digitado no login para a tela "Esqueci minha senha" sem colocá-lo na URL
/// (endereços ficam no histórico do navegador e em logs de acesso). Só em memória.
/// </summary>
public sealed class EmailInformado
{
    private string? _email;

    public void Guardar(string? email) => _email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

    /// <summary>Devolve e esquece (uso único).</summary>
    public string? Consumir()
    {
        var email = _email;
        _email = null;
        return email;
    }
}
