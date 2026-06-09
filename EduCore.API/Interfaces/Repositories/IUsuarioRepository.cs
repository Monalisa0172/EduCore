namespace EduCore.API.Interfaces.Repositories;

using EduCore.API.Entities;

public interface IUsuarioRepository
{
    Task<Usuario?> GetByEmailAsync(string email);
    Task AddAsync(Usuario usuario);
    Task<Usuario?> GetByIdAsync(int id);
    Task UpdateAsync(Usuario usuario);
}