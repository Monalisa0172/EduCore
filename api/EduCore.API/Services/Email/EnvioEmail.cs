namespace EduCore.API.Services.Email;

/// <summary>E-mail pronto para envio (HTML e texto simples, para clientes que não exibem HTML).</summary>
public sealed record MensagemEmail(string Para, string NomePara, string Assunto, string Html, string Texto);

/// <summary>
/// Envio de e-mail. Em produção, SMTP (qualquer provedor); sem SMTP configurado, grava em arquivo
/// (desenvolvimento), para testar o fluxo sem conta em provedor nenhum.
/// </summary>
public interface IEnviadorEmail
{
    Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken = default);
}

public class EmailOptions
{
    public const string Secao = "Email";

    public string RemetenteNome { get; set; } = "EduCore";

    public string RemetenteEndereco { get; set; } = "nao-responda@educore.local";

    public SmtpOptions Smtp { get; set; } = new();

    /// <summary>Pasta (relativa ao projeto) onde o envio em arquivo grava os e-mails.</summary>
    public string PastaArquivos { get; set; } = "emails-dev";

    /// <summary>Com servidor SMTP configurado, os e-mails saem de verdade.</summary>
    public bool UsaSmtp => !string.IsNullOrWhiteSpace(Smtp.Host);
}

public class SmtpOptions
{
    public string? Host { get; set; }

    public int Porta { get; set; } = 587;

    public string? Usuario { get; set; }

    /// <summary>Segredo: nunca no appsettings (User Secrets em desenvolvimento, variável de ambiente Email__Smtp__Senha em produção).</summary>
    public string? Senha { get; set; }
}
