namespace EduCore.API.Interfaces.Repositories;

using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Enums;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task AddAsync(Usuario usuario);
    Task<Usuario?> GetByIdAsync(int id);
    Task UpdateAsync(Usuario usuario);
    Task<List<UsuarioListItemDTO>> GetAtivosAsync(IReadOnlyCollection<TipoUsuarioEnum> tipos);
    Task<int> CountAtivosAsync(TipoUsuarioEnum tipo);

    /// <summary>
    /// Usuário rastreado com aluno/matrículas/turmas e funcionário/professor, para edição.
    /// </summary>
    Task<Usuario?> GetComPerfilAsync(int id);
    Task<bool> EmailEmUsoAsync(string email, int? ignorarUsuarioId = null);
    void RemoverMatriculas(IEnumerable<Matricula> matriculas);
    Task SalvarAlteracoesAsync();
}