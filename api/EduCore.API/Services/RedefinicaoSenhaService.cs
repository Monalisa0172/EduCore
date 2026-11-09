using System.Security.Cryptography;
using System.Text;
using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Email;
using Microsoft.Extensions.Options;

namespace EduCore.API.Services;

public class RedefinicaoSenhaOptions
{
    public const string Secao = "RedefinicaoSenha";

    /// <summary>Validade do link enviado por e-mail.</summary>
    public int ValidadeMinutos { get; set; } = 30;

    /// <summary>Pedidos repetidos dentro deste intervalo não geram outro e-mail (evita spam na caixa da pessoa).</summary>
    public int IntervaloMinimoSegundos { get; set; } = 120;

    /// <summary>Endereço do front (o link aponta para {UrlFront}/redefinir-senha). Vem da configuração, nunca da requisição.</summary>
    public string UrlFront { get; set; } = string.Empty;
}

/// <summary>
/// "Esqueci minha senha": link de uso único e com validade, enviado por e-mail.
/// - A resposta do pedido é sempre a mesma, exista ou não a conta (não revela e-mails cadastrados).
/// - Só o hash SHA-256 do token fica no banco; o token em si só existe no e-mail.
/// - Pedido novo invalida os anteriores; usar o link invalida todos os abertos do usuário.
/// </summary>
public class RedefinicaoSenhaService
{
    private const int BytesDoToken = 32;
    private const string LinkInvalido = "Este link de redefinição é inválido, já foi usado ou expirou. Peça um novo.";

    private readonly IRedefinicaoSenhaRepository _repositorio;
    private readonly IEnviadorEmail _enviadorEmail;
    private readonly RedefinicaoSenhaOptions _opcoes;
    private readonly TimeProvider _relogio;
    private readonly ILogger<RedefinicaoSenhaService> _logger;

    public RedefinicaoSenhaService(
        IRedefinicaoSenhaRepository repositorio,
        IEnviadorEmail enviadorEmail,
        IOptions<RedefinicaoSenhaOptions> opcoes,
        TimeProvider relogio,
        ILogger<RedefinicaoSenhaService> logger)
    {
        _repositorio = repositorio;
        _enviadorEmail = enviadorEmail;
        _opcoes = opcoes.Value;
        _relogio = relogio;
        _logger = logger;
    }

    private DateTime Agora => _relogio.GetUtcNow().UtcDateTime;

    /// <summary>
    /// Registra o pedido e envia o link. Não informa se a conta existe: o chamador responde sempre igual.
    /// </summary>
    public async Task SolicitarAsync(string email, CancellationToken cancellationToken = default)
    {
        var usuario = await _repositorio.GetUsuarioAtivoPorEmailAsync(email.Trim().ToLowerInvariant());
        if (usuario is null)
        {
            return;
        }

        var abertas = await _repositorio.GetAbertasAsync(usuario.Id);

        // Pedido repetido em sequencia: o e-mail anterior ainda vale, nao manda outro.
        if (abertas.Any(r => r.ExpiraEm > Agora && (Agora - r.CriadoEm).TotalSeconds < _opcoes.IntervaloMinimoSegundos))
        {
            return;
        }

        foreach (var anterior in abertas)
        {
            anterior.UsadoEm = Agora;
        }

        var token = GerarToken();
        _repositorio.Adicionar(new RedefinicaoSenha
        {
            UsuarioId = usuario.Id,
            TokenHash = Hash(token),
            CriadoEm = Agora,
            ExpiraEm = Agora.AddMinutes(_opcoes.ValidadeMinutos)
        });

        await _repositorio.SalvarAsync();

        var link = $"{_opcoes.UrlFront.TrimEnd('/')}/redefinir-senha?token={Uri.EscapeDataString(token)}";

        try
        {
            await _enviadorEmail.EnviarAsync(
                ModelosEmail.RedefinicaoSenha(usuario.Email!, usuario.Nome ?? string.Empty, link, _opcoes.ValidadeMinutos),
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Falha de envio nao muda a resposta (nao revela a conta); fica registrada para o suporte.
            _logger.LogError(ex, "Falha ao enviar o e-mail de redefinicao de senha do usuario {UsuarioId}.", usuario.Id);
        }
    }

    /// <summary>O link ainda pode ser usado? (a tela avisa antes de a pessoa digitar a senha).</summary>
    public async Task<bool> TokenValidoAsync(string token)
        => Valida(await _repositorio.GetPorHashAsync(Hash(token)));

    public async Task<ResultadoOperacao<bool>> RedefinirAsync(RedefinirSenhaRequest request)
    {
        var redefinicao = await _repositorio.GetPorHashAsync(Hash(request.Token));
        if (!Valida(redefinicao))
        {
            return ResultadoOperacao<bool>.Invalido(nameof(RedefinirSenhaRequest.Token), LinkInvalido);
        }

        redefinicao!.Usuario.Senha = BCrypt.Net.BCrypt.HashPassword(request.NovaSenha);

        // Este e qualquer outro link aberto do usuario deixam de valer.
        foreach (var aberta in await _repositorio.GetAbertasAsync(redefinicao.UsuarioId))
        {
            aberta.UsadoEm = Agora;
        }

        redefinicao.UsadoEm = Agora;
        await _repositorio.SalvarAsync();

        return ResultadoOperacao<bool>.Ok(true);
    }

    private bool Valida(RedefinicaoSenha? redefinicao)
        => redefinicao is { UsadoEm: null } && redefinicao.ExpiraEm > Agora && redefinicao.Usuario is { Ativo: true };

    /// <summary>32 bytes aleatórios (criptograficamente seguros), em Base64 para URL.</summary>
    private static string GerarToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(BytesDoToken))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>SHA-256 do token em hexadecimal (64 caracteres): é o que fica no banco.</summary>
    public static string Hash(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token ?? string.Empty)));
}
