using EduCore.API.DTOs.Response;
using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services.Acesso;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class AcessoAcademicoTests
{
    private const int UsuarioId = 30;
    private const int ProfessorId = 5;
    private const int TurmaId = 1;
    private const int PortuguesId = 10;
    private const int GramaticaId = 100;
    private const int LiteraturaId = 101;

    private readonly Mock<IUsuarioAtual> _usuario = new();
    private readonly Mock<IProfessorRepository> _professorRepository = new();
    private readonly Mock<IAlunoRepository> _alunoRepository = new();
    private readonly Mock<IAtribuicaoRepository> _atribuicaoRepository = new();

    private readonly AcessoAcademico _acesso;

    public AcessoAcademicoTests()
    {
        _acesso = new AcessoAcademico(_usuario.Object, _professorRepository.Object, _alunoRepository.Object, _atribuicaoRepository.Object);

        _usuario.SetupGet(x => x.UsuarioId).Returns(UsuarioId);
        _usuario.SetupGet(x => x.EhProfessor).Returns(true);
        _professorRepository.Setup(x => x.GetIdAtivoPorUsuarioAsync(UsuarioId)).ReturnsAsync(ProfessorId);
        _atribuicaoRepository.Setup(x => x.GetAtivasDoProfessorAsync(ProfessorId, It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync([]);
    }

    private void Leciona(int? subDisciplinaId)
        => _atribuicaoRepository.Setup(x => x.GetAtivasDoProfessorAsync(ProfessorId, TurmaId, PortuguesId))
            .ReturnsAsync([new AtribuicaoResponseDTO { ProfessorId = ProfessorId, TurmaId = TurmaId, DisciplinaId = PortuguesId, SubDisciplinaId = subDisciplinaId }]);

    [Fact]
    public async Task Admin_Pode_Tudo_Sem_Consultar_Atribuicoes()
    {
        _usuario.SetupGet(x => x.EhAdmin).Returns(true);

        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, GramaticaId)).Should().BeTrue();
        (await _acesso.PodeFazerChamadaAsync(TurmaId, PortuguesId)).Should().BeTrue();
        _atribuicaoRepository.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Disciplina_Inteira_Da_Acesso_A_Qualquer_Subdisciplina()
    {
        Leciona(subDisciplinaId: null);

        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, null)).Should().BeTrue();
        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, LiteraturaId)).Should().BeTrue();
    }

    [Fact]
    public async Task Subdisciplina_So_Da_Acesso_A_Ela()
    {
        Leciona(subDisciplinaId: GramaticaId);

        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, GramaticaId)).Should().BeTrue();
        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, LiteraturaId)).Should().BeFalse();
        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, null)).Should().BeFalse();
    }

    [Fact]
    public async Task Quem_Leciona_Parte_Da_Disciplina_Faz_A_Chamada_Dela()
    {
        Leciona(subDisciplinaId: GramaticaId);

        (await _acesso.PodeFazerChamadaAsync(TurmaId, PortuguesId)).Should().BeTrue();
    }

    [Fact]
    public async Task Sem_Atribuicao_Nao_Tem_Acesso()
    {
        (await _acesso.PodeGerenciarAvaliacaoAsync(TurmaId, PortuguesId, null)).Should().BeFalse();
        (await _acesso.PodeFazerChamadaAsync(TurmaId, PortuguesId)).Should().BeFalse();
    }

    [Fact]
    public async Task Aluno_Nao_Tem_Acesso_De_Professor()
    {
        _usuario.SetupGet(x => x.EhProfessor).Returns(false);
        _usuario.SetupGet(x => x.EhAluno).Returns(true);

        (await _acesso.ProfessorIdAsync()).Should().BeNull();
        (await _acesso.PodeFazerChamadaAsync(TurmaId, PortuguesId)).Should().BeFalse();
        _professorRepository.Verify(x => x.GetIdAtivoPorUsuarioAsync(It.IsAny<int>()), Times.Never);
    }
}
