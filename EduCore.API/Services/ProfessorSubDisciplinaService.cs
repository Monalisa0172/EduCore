using EduCore.API.DTOs.Request;
using EduCore.API.DTOs.Response;
using EduCore.API.Entities;
using EduCore.API.Interfaces.Repositories;

namespace EduCore.API.Services;

public class ProfessorSubDisciplinaService
{
    private readonly IProfessorSubDisciplinaRepository _professorSubDisciplinaRepository;
    private readonly IProfessorRepository _professorRepository;
    private readonly ISubDisciplinaRepository _subDisciplinaRepository;

    public ProfessorSubDisciplinaService(IProfessorSubDisciplinaRepository professorSubDisciplinaRepository, IProfessorRepository professorRepository, ISubDisciplinaRepository subDisciplinaRepository)
    {
        _professorSubDisciplinaRepository = professorSubDisciplinaRepository;
        _professorRepository = professorRepository;
        _subDisciplinaRepository = subDisciplinaRepository;
    }

    public async Task<List<ProfessorSubDisciplinaResponseDTO>> GetAllAsync()
    {
        return await _professorSubDisciplinaRepository.GetAllAsync();
    }

    public async Task<ProfessorSubDisciplinaResponseDTO?> GetByIdAsync(int id)
    {
        return await _professorSubDisciplinaRepository.GetByIdAsync(id);
    }

    public async Task<ProfessorSubDisciplinaResponseDTO?> CreateAsync(CreateProfessorSubDisciplinaRequest request)
    {
        var professor = await _professorRepository.GetEntityByIdAsync(request.ProfessorId);

        if (professor == null)
            throw new Exception("Professor não encontrado");

        var subDisciplina = await _subDisciplinaRepository.GetEntityByIdAsync(request.SubDisciplinaId);

        if (subDisciplina == null)
            throw new Exception("Subdisciplina não encontrada");

        var vinculoExiste = await _professorSubDisciplinaRepository.GetByProfessorAndSubDisciplinaAsync(request.ProfessorId, request.SubDisciplinaId);

        if (vinculoExiste != null)
            throw new Exception("Vínculo já existe");

        var professorSubDisciplina =
            new ProfessorSubDisciplina
            {
                ProfessorId = request.ProfessorId,
                SubDisciplinaId =
                    request.SubDisciplinaId,
                Ativo = true
            };

        await _professorSubDisciplinaRepository.AddAsync(professorSubDisciplina);

        return await _professorSubDisciplinaRepository.GetByIdAsync(professorSubDisciplina.Id);
    }

    public async Task<bool> InativarAsync(int id)
    {
        var vinculo = await _professorSubDisciplinaRepository.GetEntityByIdAsync(id);

        if (vinculo == null)
            return false;

        vinculo.Ativo = false;

        await _professorSubDisciplinaRepository.UpdateAsync(vinculo);

        return true;
    }
}