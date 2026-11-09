using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EduCore.API.Infrastructure;

/// <summary>Erro inesperado já traduzido para a resposta (nunca leva detalhes internos).</summary>
public sealed record ErroTraduzido(int Status, string Titulo, string Mensagem);

/// <summary>
/// Último ponto de tratamento: qualquer exceção que escapar de um controller vira um ProblemDetails
/// padronizado, e o detalhe técnico fica só no log (com o traceId que também vai na resposta).
/// Erros previstos (não encontrado, validação, duplicado) continuam tratados no service via ResultadoOperacao.
/// </summary>
public sealed class TratadorErrosGlobal(IProblemDetailsService problemDetails, ILogger<TratadorErrosGlobal> logger) : IExceptionHandler
{
    // Erros do SQL Server que passam sozinhos (conexao, timeout, deadlock, banco em failover).
    private static readonly HashSet<int> ErrosTransitorios = [-2, -1, 2, 53, 121, 233, 1205, 4060, 10053, 10054, 10060, 10928, 10929, 40197, 40501, 40613];

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var metodo = httpContext.Request.Method;
        var caminho = httpContext.Request.Path.Value;

        // Quem pediu desistiu (fechou a aba, navegou): nao e erro do sistema e ninguem le a resposta.
        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug("Requisicao cancelada pelo cliente: {Metodo} {Caminho}", metodo, caminho);
            httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            return true;
        }

        var erro = Classificar(exception);

        if (erro.Status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Erro {Status} em {Metodo} {Caminho} (traceId {TraceId})", erro.Status, metodo, caminho, traceId);
        }
        else
        {
            logger.LogWarning(exception, "Conflito {Status} em {Metodo} {Caminho} (traceId {TraceId})", erro.Status, metodo, caminho, traceId);
        }

        httpContext.Response.StatusCode = erro.Status;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = erro.Status,
                Title = erro.Titulo,
                Detail = erro.Mensagem
            }
        });
    }

    public static ErroTraduzido Classificar(Exception exception)
    {
        // Duas pessoas salvando o mesmo registro ao mesmo tempo.
        if (exception is DbUpdateConcurrencyException)
        {
            return new(StatusCodes.Status409Conflict, "Registro alterado",
                "Este registro foi alterado por outra pessoa enquanto você editava. Atualize a página e tente de novo.");
        }

        var sql = EncontrarSqlException(exception);

        if (sql is not null)
        {
            switch (sql.Number)
            {
                // Indice unico: o service confere antes, mas duas gravacoes simultaneas podem passar juntas.
                case 2601 or 2627:
                    return new(StatusCodes.Status409Conflict, "Registro duplicado",
                        "Já existe um registro com estes dados. Atualize a página e confira.");

                // Chave estrangeira ou CHECK do banco.
                case 547:
                    return new(StatusCodes.Status409Conflict, "Conflito de dados",
                        "Não foi possível salvar porque os dados estão ligados a outros registros ou foram alterados. Atualize a página e tente de novo.");
            }

            if (ErrosTransitorios.Contains(sql.Number))
            {
                return Indisponivel();
            }
        }

        if (exception is TimeoutException or RetryLimitExceededException)
        {
            return Indisponivel();
        }

        return new(StatusCodes.Status500InternalServerError, "Erro inesperado",
            "Ocorreu um erro inesperado. Tente novamente; se continuar, procure o suporte.");
    }

    private static ErroTraduzido Indisponivel() => new(StatusCodes.Status503ServiceUnavailable, "Serviço temporariamente indisponível",
        "Não foi possível acessar os dados agora. Tente novamente em alguns instantes.");

    private static SqlException? EncontrarSqlException(Exception? exception)
    {
        for (var atual = exception; atual is not null; atual = atual.InnerException)
        {
            if (atual is SqlException sql)
            {
                return sql;
            }
        }

        return null;
    }
}
