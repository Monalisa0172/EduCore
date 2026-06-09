using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IFuncionarioRepository
{
    Task AddAsync(Funcionario funcionario);
    Task<Funcionario?> GetByIdAsync(int id);
    Task UpdateAsync(Funcionario funcionario);

}