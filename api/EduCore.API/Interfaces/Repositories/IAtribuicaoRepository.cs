using EduCore.API.DTOs.Response;
using EduCore.API.Entities;

namespace EduCore.API.Interfaces.Repositories;

public interface IAtribuicaoRepository
{
    /// <summary>Lista com nomes resolvidos; filtros nulos são ignorados.</summary>
    Task<List<AtribuicaoResponseDTO>> ListarAsync(int? turmaId, int? professorId, int? disciplinaId, bool? ativo);

    Task<AtribuicaoResponseDTO?> GetByIdAsync(int id);

    Task<Atribuicao?> GetEntityByIdAsync(int id);

    /// <summary>Atribuições ativas da disciplina na turma (para checar conflitos), com o nome do professor.</summary>
    Task<List<AtribuicaoResponseDTO>> GetAtivasDaDisciplinaAsync(int turmaId, int disciplinaId);

    /// <summary>Atribuições ativas do professor, opcionalmente só de uma turma/disciplina (controle de acesso).</summary>
    Task<List<AtribuicaoResponseDTO>> GetAtivasDoProfessorAsync(int professorId, int? turmaId = null, int? disciplinaId = null);

    Task AddAsync(Atribuicao atribuicao);

    Task SalvarAsync();
}
