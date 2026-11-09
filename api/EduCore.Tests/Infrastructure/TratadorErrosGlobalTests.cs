using System.Text.Json;
using EduCore.API.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace EduCore.Tests.Infrastructure;

public class TratadorErrosGlobalTests
{
    private const string SegredoInterno = "Server=sql01;Password=nao-pode-vazar";

    [Fact]
    public void Erro_Desconhecido_Vira_500_Sem_Detalhe_Interno()
    {
        var erro = TratadorErrosGlobal.Classificar(new InvalidOperationException(SegredoInterno));

        erro.Status.Should().Be(StatusCodes.Status500InternalServerError);
        erro.Mensagem.Should().NotContain("sql01");
    }

    [Fact]
    public void Gravacao_Concorrente_Vira_409()
    {
        TratadorErrosGlobal.Classificar(new DbUpdateConcurrencyException("x"))
            .Status.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void Timeout_Vira_503()
    {
        TratadorErrosGlobal.Classificar(new TimeoutException())
            .Status.Should().Be(StatusCodes.Status503ServiceUnavailable);
    }

    [Fact]
    public async Task Responde_ProblemDetails_Com_TraceId_E_Sem_A_Mensagem_Da_Excecao()
    {
        var (tratador, contexto) = Criar();

        var tratou = await tratador.TryHandleAsync(contexto, new InvalidOperationException(SegredoInterno), CancellationToken.None);

        tratou.Should().BeTrue();
        contexto.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        contexto.Response.ContentType.Should().Contain("application/problem+json");

        var corpo = LerCorpo(contexto);
        corpo.Should().NotContain("sql01").And.NotContain("Password");

        using var json = JsonDocument.Parse(corpo);
        json.RootElement.GetProperty("status").GetInt32().Should().Be(500);
        json.RootElement.GetProperty("title").GetString().Should().Be("Erro inesperado");
        json.RootElement.TryGetProperty("traceId", out _).Should().BeTrue("o traceId liga a resposta ao log");
    }

    [Fact]
    public async Task Requisicao_Cancelada_Pelo_Cliente_Nao_Gera_Corpo()
    {
        var (tratador, contexto) = Criar();
        using var cancelado = new CancellationTokenSource();
        cancelado.Cancel();
        contexto.RequestAborted = cancelado.Token;

        var tratou = await tratador.TryHandleAsync(contexto, new OperationCanceledException(), CancellationToken.None);

        tratou.Should().BeTrue();
        contexto.Response.StatusCode.Should().Be(StatusCodes.Status499ClientClosedRequest);
        LerCorpo(contexto).Should().BeEmpty();
    }

    private static (TratadorErrosGlobal, DefaultHttpContext) Criar()
    {
        var servicos = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var contexto = new DefaultHttpContext { RequestServices = servicos };
        contexto.Request.Method = "GET";
        contexto.Request.Path = "/api/turmas";
        contexto.Request.Headers.Accept = "application/json";
        contexto.Response.Body = new MemoryStream();

        var tratador = new TratadorErrosGlobal(
            servicos.GetRequiredService<IProblemDetailsService>(),
            NullLogger<TratadorErrosGlobal>.Instance);

        return (tratador, contexto);
    }

    private static string LerCorpo(HttpContext contexto)
    {
        contexto.Response.Body.Position = 0;
        return new StreamReader(contexto.Response.Body).ReadToEnd();
    }
}
