using System.Net;

namespace EduCore.API.Services.Email;

/// <summary>
/// Textos dos e-mails. HTML com estilos inline e tabela (é o que os clientes de e-mail respeitam).
/// </summary>
public static class ModelosEmail
{
    private const string Azul = "#1E4FA3";
    private const string Ambar = "#F2A900";

    public static MensagemEmail RedefinicaoSenha(string email, string nome, string link, int validadeMinutos)
    {
        var primeiroNome = WebUtility.HtmlEncode((nome ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "olá");
        var linkHtml = WebUtility.HtmlEncode(link);

        var html = $$"""
            <!doctype html>
            <html lang="pt-BR">
            <body style="margin:0;padding:24px;background:#f3f4f6;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:520px;margin:0 auto;background:#ffffff;border-radius:12px;overflow:hidden;">
                <tr><td style="background:{{Azul}};border-bottom:4px solid {{Ambar}};padding:20px 28px;color:#ffffff;font-size:20px;font-weight:600;">EduCore</td></tr>
                <tr><td style="padding:28px;">
                  <p style="margin:0 0 16px;font-size:16px;">Olá, {{primeiroNome}}!</p>
                  <p style="margin:0 0 24px;font-size:15px;line-height:1.5;">Recebemos um pedido para redefinir a senha da sua conta no EduCore. Para criar uma nova senha, use o botão abaixo:</p>
                  <p style="margin:0 0 24px;"><a href="{{linkHtml}}" style="display:inline-block;padding:12px 24px;background:{{Azul}};color:#ffffff;text-decoration:none;border-radius:6px;font-weight:600;">Criar nova senha</a></p>
                  <p style="margin:0 0 8px;font-size:13px;color:#4b5563;">O link vale por {{validadeMinutos}} minutos e só pode ser usado uma vez.</p>
                  <p style="margin:0 0 24px;font-size:13px;color:#4b5563;">Se você não pediu a troca, ignore este e-mail: sua senha continua a mesma.</p>
                  <p style="margin:0;font-size:12px;color:#6b7280;word-break:break-all;">Se o botão não funcionar, copie e cole no navegador:<br>{{linkHtml}}</p>
                </td></tr>
              </table>
            </body>
            </html>
            """;

        var texto = $"""
            Olá, {primeiroNome}!

            Recebemos um pedido para redefinir a senha da sua conta no EduCore.
            Para criar uma nova senha, abra o link abaixo (vale por {validadeMinutos} minutos e só pode ser usado uma vez):

            {link}

            Se você não pediu a troca, ignore este e-mail: sua senha continua a mesma.
            """;

        return new MensagemEmail(email, nome ?? string.Empty, "Redefinição de senha do EduCore", html, texto);
    }
}
