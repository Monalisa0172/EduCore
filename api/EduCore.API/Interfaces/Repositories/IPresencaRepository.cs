using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IPresencaRepository
{
    /// <summary>Registros da aula (turma + disciplina + dia), rastreados para atualizar no lote.</summary>
    Task<List<Presenca>> GetDaAulaAsync(int turmaId, int disciplinaId, DateOnly data);

    /// <summary>Aulas com chamada: data, presentes e total, da mais recente para a mais antiga.</summary>
    Task<List<(DateOnly Data, int Presentes, int Total)>> GetAulasAsync(int turmaId, int disciplinaId);

    void Adicionar(Presenca presenca);

    Task SalvarAsync();
}
