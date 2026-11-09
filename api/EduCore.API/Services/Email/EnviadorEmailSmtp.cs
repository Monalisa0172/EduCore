using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EduCore.API.Services.Email;

/// <summary>
/// Envio por SMTP com MailKit (STARTTLS na porta 587). Serve para qualquer provedor (Brevo, Resend, etc.).
/// </summary>
public class EnviadorEmailSmtp : IEnviadorEmail
{
    private readonly EmailOptions _opcoes;

    public EnviadorEmailSmtp(IOptions<EmailOptions> opcoes)
    {
        _opcoes = opcoes.Value;
    }

    public async Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_opcoes.RemetenteNome, _opcoes.RemetenteEndereco));
        email.To.Add(new MailboxAddress(mensagem.NomePara, mensagem.Para));
        email.Subject = mensagem.Assunto;
        email.Body = new BodyBuilder { HtmlBody = mensagem.Html, TextBody = mensagem.Texto }.ToMessageBody();

        using var cliente = new SmtpClient();
        // TLS obrigatorio: 465 = SSL direto; demais portas = STARTTLS (falha se o servidor nao oferecer, em vez de enviar sem criptografia).
        var seguranca = _opcoes.Smtp.Porta == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        // Este enviador so e registrado com Host preenchido (EmailOptions.UsaSmtp).
        await cliente.ConnectAsync(_opcoes.Smtp.Host!, _opcoes.Smtp.Porta, seguranca, cancellationToken);

        if (!string.IsNullOrWhiteSpace(_opcoes.Smtp.Usuario))
        {
            await cliente.AuthenticateAsync(_opcoes.Smtp.Usuario, _opcoes.Smtp.Senha ?? string.Empty, cancellationToken);
        }

        await cliente.SendAsync(email, cancellationToken);
        await cliente.DisconnectAsync(quit: true, cancellationToken);
    }
}
