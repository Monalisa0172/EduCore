using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Email;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace EduCore.Tests.Services;

public class RedefinicaoSenhaServiceTests
{
    private static readonly DateTime Agora = new(2026, 11, 9, 13, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRedefinicaoSenhaRepository> _repositorio = new();
    private readonly Mock<IEnviadorEmail> _email = new();
    private readonly List<RedefinicaoSenha> _registros = [];
    private readonly Usuario _ana = new() { Id = 7, Nome = "Ana Souza", Email = "ana@escola.com", Ativo = true, Senha = "hash-antigo" };
    private readonly RedefinicaoSenhaService _service;
    private MensagemEmail? _enviado;

    public RedefinicaoSenhaServiceTests()
    {
        var opcoes = Options.Create(new RedefinicaoSenhaOptions { ValidadeMinutos = 30, IntervaloMinimoSegundos = 120, UrlFront = "https://educore.exemplo.com/" });
        _service = new RedefinicaoSenhaService(_repositorio.Object, _email.Object, opcoes, new RelogioUtc(Agora), NullLogger<RedefinicaoSenhaService>.Instance);

        _repositorio.Setup(x => x.GetUsuarioAtivoPorEmailAsync("ana@escola.com")).ReturnsAsync(_ana);
        _repositorio.Setup(x => x.GetAbertasAsync(_ana.Id)).ReturnsAsync(() => _registros.Where(r => r.UsadoEm == null).ToList());
        _repositorio.Setup(x => x.Adicionar(It.IsAny<RedefinicaoSenha>())).Callback<RedefinicaoSenha>(r => { r.Usuario = _ana; _registros.Add(r); });
        _repositorio.Setup(x => x.GetPorHashAsync(It.IsAny<string>())).ReturnsAsync((string h) => _registros.FirstOrDefault(r => r.TokenHash == h));
        _email.Setup(x => x.EnviarAsync(It.IsAny<MensagemEmail>(), It.IsAny<CancellationToken>()))
            .Callback<MensagemEmail, CancellationToken>((m, _) => _enviado = m)
            .Returns(Task.CompletedTask);
    }

    private string TokenDoEmail()
    {
        var marcador = "token=";
        var inicio = _enviado!.Texto.IndexOf(marcador, StringComparison.Ordinal) + marcador.Length;
        var fim = _enviado.Texto.IndexOfAny(['\r', '\n'], inicio);
        return Uri.UnescapeDataString(_enviado.Texto[inicio..fim]);
    }

    [Fact]
    public async Task Envia_Link_E_Guarda_So_O_Hash()
    {
        await _service.SolicitarAsync("  ANA@escola.com ");

        _enviado!.Para.Should().Be("ana@escola.com");
        _enviado.Texto.Should().Contain("https://educore.exemplo.com/redefinir-senha?token=");
        var token = TokenDoEmail();
        token.Length.Should().BeGreaterThan(40, "32 bytes aleatórios");

        var registro = _registros.Single();
        registro.TokenHash.Should().Be(RedefinicaoSenhaService.Hash(token)).And.HaveLength(64).And.NotContain(token);
        registro.ExpiraEm.Should().Be(Agora.AddMinutes(30));
        _repositorio.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Email_Sem_Conta_Nao_Faz_Nada()
    {
        await _service.SolicitarAsync("ninguem@escola.com");

        _email.Verify(x => x.EnviarAsync(It.IsAny<MensagemEmail>(), It.IsAny<CancellationToken>()), Times.Never);
        _registros.Should().BeEmpty();
    }

    [Fact]
    public async Task Pedido_Repetido_Em_Seguida_Nao_Manda_Outro_Email()
    {
        _registros.Add(new RedefinicaoSenha { UsuarioId = _ana.Id, TokenHash = "X", CriadoEm = Agora.AddSeconds(-30), ExpiraEm = Agora.AddMinutes(29), Usuario = _ana });

        await _service.SolicitarAsync("ana@escola.com");

        _email.Verify(x => x.EnviarAsync(It.IsAny<MensagemEmail>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Pedido_Novo_Invalida_O_Link_Anterior()
    {
        var antigo = new RedefinicaoSenha { UsuarioId = _ana.Id, TokenHash = "ANTIGO", CriadoEm = Agora.AddMinutes(-10), ExpiraEm = Agora.AddMinutes(20), Usuario = _ana };
        _registros.Add(antigo);

        await _service.SolicitarAsync("ana@escola.com");

        antigo.UsadoEm.Should().Be(Agora);
        _registros.Should().HaveCount(2);
    }

    [Fact]
    public async Task Redefine_A_Senha_E_O_Link_Deixa_De_Valer()
    {
        await _service.SolicitarAsync("ana@escola.com");
        var token = TokenDoEmail();

        (await _service.TokenValidoAsync(token)).Should().BeTrue();
        var resultado = await _service.RedefinirAsync(new RedefinirSenhaRequest { Token = token, NovaSenha = "NovaSenha123" });

        resultado.Sucesso.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("NovaSenha123", _ana.Senha).Should().BeTrue();
        (await _service.TokenValidoAsync(token)).Should().BeFalse("o link é de uso único");
        (await _service.RedefinirAsync(new RedefinirSenhaRequest { Token = token, NovaSenha = "OutraSenha123" })).Sucesso.Should().BeFalse();
    }

    [Fact]
    public async Task Link_Expirado_Ou_Desconhecido_E_Recusado()
    {
        _registros.Add(new RedefinicaoSenha { UsuarioId = _ana.Id, TokenHash = RedefinicaoSenhaService.Hash("vencido"), CriadoEm = Agora.AddMinutes(-40), ExpiraEm = Agora.AddMinutes(-10), Usuario = _ana });

        var vencido = await _service.RedefinirAsync(new RedefinirSenhaRequest { Token = "vencido", NovaSenha = "NovaSenha123" });
        var desconhecido = await _service.RedefinirAsync(new RedefinirSenhaRequest { Token = "qualquer", NovaSenha = "NovaSenha123" });

        vencido.Falha.Should().Be(FalhaOperacao.Invalido);
        vencido.Mensagem.Should().Contain("expirou");
        desconhecido.Falha.Should().Be(FalhaOperacao.Invalido);
        _ana.Senha.Should().Be("hash-antigo");
    }

    [Fact]
    public async Task Usuario_Inativado_Depois_Do_Pedido_Nao_Redefine()
    {
        await _service.SolicitarAsync("ana@escola.com");
        var token = TokenDoEmail();
        _ana.Ativo = false;

        (await _service.RedefinirAsync(new RedefinirSenhaRequest { Token = token, NovaSenha = "NovaSenha123" })).Sucesso.Should().BeFalse();
    }

    [Fact]
    public async Task Falha_No_Envio_Nao_Estoura()
    {
        _email.Setup(x => x.EnviarAsync(It.IsAny<MensagemEmail>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("SMTP fora"));

        var acao = () => _service.SolicitarAsync("ana@escola.com");

        await acao.Should().NotThrowAsync();
    }

    private sealed class RelogioUtc(DateTime agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(agora, TimeSpan.Zero);
    }
}
