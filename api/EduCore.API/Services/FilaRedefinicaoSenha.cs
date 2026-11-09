using System.Threading.Channels;

namespace EduCore.API.Services;

/// <summary>
/// Fila dos pedidos de "esqueci minha senha". A API responde na hora e o envio acontece em segundo plano:
/// assim o tempo de resposta é o mesmo exista ou não a conta (não dá para descobrir e-mails cadastrados
/// medindo a demora).
/// </summary>
public class FilaRedefinicaoSenha
{
    // Limitada: sob abuso, pedidos excedentes sao descartados em vez de consumir memoria.
    private readonly Channel<string> _canal = Channel.CreateBounded<string>(new BoundedChannelOptions(500)
    {
        FullMode = BoundedChannelFullMode.DropWrite,
        SingleReader = true
    });

    public bool Enfileirar(string email) => _canal.Writer.TryWrite(email);

    public IAsyncEnumerable<string> LerTodosAsync(CancellationToken cancellationToken) => _canal.Reader.ReadAllAsync(cancellationToken);
}

/// <summary>
/// Processa a fila: cada pedido roda num escopo próprio (DbContext novo), e uma falha não derruba os seguintes.
/// </summary>
public class ProcessadorRedefinicaoSenha : BackgroundService
{
    private readonly FilaRedefinicaoSenha _fila;
    private readonly IServiceScopeFactory _escopos;
    private readonly ILogger<ProcessadorRedefinicaoSenha> _logger;

    public ProcessadorRedefinicaoSenha(FilaRedefinicaoSenha fila, IServiceScopeFactory escopos, ILogger<ProcessadorRedefinicaoSenha> logger)
    {
        _fila = fila;
        _escopos = escopos;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var email in _fila.LerTodosAsync(stoppingToken))
        {
            try
            {
                await using var escopo = _escopos.CreateAsyncScope();
                var servico = escopo.ServiceProvider.GetRequiredService<RedefinicaoSenhaService>();
                await servico.SolicitarAsync(email, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falha ao processar um pedido de redefinicao de senha.");
            }
        }
    }
}
