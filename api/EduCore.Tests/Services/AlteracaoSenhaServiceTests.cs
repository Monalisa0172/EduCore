using EduCore.API.DTOs.Request;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Acesso;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class AlteracaoSenhaServiceTests
{
    private static readonly DateTime Agora = new(2026, 11, 9, 13, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IRedefinicaoSenhaRepository> _repositorio = new();
    private readonly Usuario _ana = new() { Id = 7, Nome = "Ana Souza", Email = "ana@escola.com", Ativo = true, Senha = BCrypt.Net.BCrypt.HashPassword("SenhaAtual1") };
    private readonly List<RedefinicaoSenha> _abertas = [];
    private readonly AlteracaoSenhaService _service;

    public AlteracaoSenhaServiceTests()
    {
        var usuarioAtual = new Mock<IUsuarioAtual>();
        usuarioAtual.SetupGet(x => x.UsuarioId).Returns(_ana.Id);

        _repositorio.Setup(x => x.GetUsuarioAtivoAsync(_ana.Id)).ReturnsAsync(() => _ana.Ativo ? _ana : null);
        _repositorio.Setup(x => x.GetAbertasAsync(_ana.Id)).ReturnsAsync(() => _abertas.Where(r => r.UsadoEm == null).ToList());

        _service = new AlteracaoSenhaService(_repositorio.Object, usuarioAtual.Object, new RelogioUtc(Agora));
    }

    [Fact]
    public async Task Troca_A_Senha_E_Invalida_Links_De_Redefinicao_Abertos()
    {
        var link = new RedefinicaoSenha { UsuarioId = _ana.Id, TokenHash = "X", CriadoEm = Agora.AddMinutes(-5), ExpiraEm = Agora.AddMinutes(25) };
        _abertas.Add(link);

        var resultado = await _service.AlterarAsync(new AlterarSenhaRequest { SenhaAtual = "SenhaAtual1", NovaSenha = "SenhaNova22" });

        resultado.Sucesso.Should().BeTrue();
        BCrypt.Net.BCrypt.Verify("SenhaNova22", _ana.Senha).Should().BeTrue();
        link.UsadoEm.Should().Be(Agora);
        _repositorio.Verify(x => x.SalvarAsync(), Times.Once);
    }

    [Fact]
    public async Task Senha_Atual_Errada_E_Recusada_No_Campo()
    {
        var hashAntes = _ana.Senha;

        var resultado = await _service.AlterarAsync(new AlterarSenhaRequest { SenhaAtual = "errada", NovaSenha = "SenhaNova22" });

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Campo.Should().Be(nameof(AlterarSenhaRequest.SenhaAtual));
        _ana.Senha.Should().Be(hashAntes);
        _repositorio.Verify(x => x.SalvarAsync(), Times.Never);
    }

    [Fact]
    public async Task Nova_Senha_Igual_A_Atual_E_Recusada()
    {
        var resultado = await _service.AlterarAsync(new AlterarSenhaRequest { SenhaAtual = "SenhaAtual1", NovaSenha = "SenhaAtual1" });

        resultado.Falha.Should().Be(FalhaOperacao.Invalido);
        resultado.Campo.Should().Be(nameof(AlterarSenhaRequest.NovaSenha));
    }

    [Fact]
    public async Task Senha_Legada_Em_Texto_Puro_Nunca_Confere()
    {
        _ana.Senha = "SenhaAtual1";

        var resultado = await _service.AlterarAsync(new AlterarSenhaRequest { SenhaAtual = "SenhaAtual1", NovaSenha = "SenhaNova22" });

        resultado.Campo.Should().Be(nameof(AlterarSenhaRequest.SenhaAtual));
    }

    [Fact]
    public async Task Usuario_Inativado_Nao_Troca()
    {
        _ana.Ativo = false;

        var resultado = await _service.AlterarAsync(new AlterarSenhaRequest { SenhaAtual = "SenhaAtual1", NovaSenha = "SenhaNova22" });

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
    }

    private sealed class RelogioUtc(DateTime agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(agora, TimeSpan.Zero);
    }
}
