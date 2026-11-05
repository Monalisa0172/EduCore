using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface INotaRepository
{
    /// <summary>Notas da avaliação, rastreadas (para atualizar no lote).</summary>
    Task<List<Nota>> GetDaAvaliacaoAsync(int avaliacaoId);

    void Adicionar(Nota nota);

    void Remover(Nota nota);

    /// <summary>Grava o lote inteiro num único SaveChanges (transação).</summary>
    Task SalvarAsync();
}
