using System.Globalization;
using Microsoft.Extensions.Options;

namespace EduCore.API.Services.Email;

/// <summary>
/// Desenvolvimento: em vez de enviar, grava o e-mail como HTML numa pasta e registra no log.
/// Basta abrir o arquivo (ou copiar o link do log) para seguir o fluxo.
/// </summary>
public class EnviadorEmailArquivo : IEnviadorEmail
{
    private readonly string _pasta;
    private readonly ILogger<EnviadorEmailArquivo> _logger;
    private readonly TimeProvider _relogio;

    public EnviadorEmailArquivo(IOptions<EmailOptions> opcoes, IHostEnvironment ambiente, ILogger<EnviadorEmailArquivo> logger, TimeProvider relogio)
    {
        _pasta = Path.Combine(ambiente.ContentRootPath, opcoes.Value.PastaArquivos);
        _logger = logger;
        _relogio = relogio;
    }

    public async Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_pasta);

        var momento = _relogio.GetLocalNow().ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        var arquivo = Path.Combine(_pasta, $"{momento}-{Guid.NewGuid():N}.html");

        await File.WriteAllTextAsync(arquivo, mensagem.Html, cancellationToken);

        _logger.LogInformation(
            "E-mail de desenvolvimento para {Para} ({Assunto}) salvo em {Arquivo}.{NovaLinha}{Texto}",
            mensagem.Para, mensagem.Assunto, arquivo, Environment.NewLine, mensagem.Texto);
    }
}
