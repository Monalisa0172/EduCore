using EduCore.API.Interfaces.Repositories;
using EduCore.API.Services;
using EduCore.API.Services.Academico;
using EduCore.API.Services.Acesso;
using FluentAssertions;
using Moq;

namespace EduCore.Tests.Services;

public class BoletimServiceTests
{
    private const int UsuarioId = 26;
    private const int AlunoId = 7;

    private readonly Mock<IBoletimRepository> _boletimRepository = new();
    private readonly Mock<IAcessoAcademico> _acesso = new();
    private readonly BoletimService _service;

    public BoletimServiceTests()
    {
        _service = new BoletimService(_boletimRepository.Object, _acesso.Object, CalendarioPadrao.Criar(), new RelogioFixo(new DateOnly(2026, 11, 6)));

        _boletimRepository.Setup(x => x.GetAlunoIdPorUsuarioAsync(UsuarioId)).ReturnsAsync(AlunoId);
        _boletimRepository.Setup(x => x.GetNomeDoAlunoAsync(AlunoId)).ReturnsAsync("Enzo Carvalho");
        _boletimRepository.Setup(x => x.GetTurmasDoAlunoAsync(AlunoId)).ReturnsAsync(
        [
            new TurmaDoAluno(12, "9º Ano A", 2026, true),
            new TurmaDoAluno(3, "8º Ano A", 2025, false)
        ]);
        _boletimRepository.Setup(x => x.CarregarAsync(AlunoId, It.IsAny<int>()))
            .ReturnsAsync(new DadosDoBoletim([new DisciplinaDoBoletim(1, "Matemática")], [], []));
    }

    [Fact]
    public async Task Ficha_Encontra_O_Aluno_Pelo_Usuario()
    {
        var boletim = (await _service.ObterPorUsuarioAsync(UsuarioId, null)).Valor!;

        boletim.AlunoId.Should().Be(AlunoId);
        boletim.AnoLetivo.Should().Be(2026, "sem ano, abre o mais recente");
        boletim.AnosLetivos.Should().Equal(2026, 2025);
        boletim.BimestreVigente.Should().Be(4);
    }

    [Fact]
    public async Task Ficha_Abre_Ano_Anterior_Encerrado()
    {
        var boletim = (await _service.ObterPorUsuarioAsync(UsuarioId, 2025)).Valor!;

        boletim.TurmaNome.Should().Be("8º Ano A");
        boletim.AnoEncerrado.Should().BeTrue();
        _boletimRepository.Verify(x => x.CarregarAsync(AlunoId, 3), Times.Once);
    }

    [Fact]
    public async Task Usuario_Que_Nao_E_Aluno_Recebe_Nao_Encontrado()
    {
        var resultado = await _service.ObterPorUsuarioAsync(999, null);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
    }

    [Fact]
    public async Task Aluno_Sem_Matricula_Recebe_Mensagem_Clara()
    {
        _boletimRepository.Setup(x => x.GetTurmasDoAlunoAsync(AlunoId)).ReturnsAsync([]);

        var resultado = await _service.ObterPorUsuarioAsync(UsuarioId, null);

        resultado.Falha.Should().Be(FalhaOperacao.NaoEncontrado);
        resultado.Mensagem.Should().Contain("não está matriculado");
    }
}
