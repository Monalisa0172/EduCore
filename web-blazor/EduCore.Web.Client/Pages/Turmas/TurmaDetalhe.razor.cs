using EduCore.Web.Client.Api;
using EduCore.Web.Client.Shared;
using EduCore.Web.Client.Turmas.Components;
using EduCore.Web.Client.Turmas.Models;
using EduCore.Web.Client.Turmas.Services;
using Microsoft.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.Turmas;

public partial class TurmaDetalhe : IDisposable
{
    private static readonly IReadOnlyList<ItemTrilha> TrilhaBase =
        [ItemTrilha.Inicio, new("Turmas", "turmas"), new("Turma")];

    private readonly CancellationTokenSource _cts = new();

    private TurmaItem? _turma;
    private ApiError? _erroTurma;
    private bool _carregandoTurma = true;
    private bool _reativando;

    // Alunos (nulo enquanto carrega)
    private List<TurmaAluno>? _alunos;
    private ApiError? _erroAlunos;
    private bool _mostrarInativos;

    private TurmaFormDialog _formulario = default!;
    private MatricularAlunosDialog _matricula = default!;
    private ConfirmDialog _confirmarInativacao = default!;
    private ConfirmDialog _confirmarRemocao = default!;
    private TurmaAluno? _alvoRemocao;
    private ApiError? _erroConfirmacao;
    private bool _processando;

    private AvisoPagina? _aviso;
    private int _avisoId;

    [Parameter]
    public int Id { get; set; }

    [Inject]
    private ITurmaApiClient TurmaApi { get; set; } = default!;

    [Inject]
    private AvisoNavegacao Avisos { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<TurmaAluno> AlunosVisiveis =>
        (_alunos ?? []).Where(a => _mostrarInativos || a.Ativo).ToList();

    private int QuantidadeAtivos => _alunos?.Count(a => a.Ativo) ?? 0;

    private int QuantidadeInativos => _alunos?.Count(a => !a.Ativo) ?? 0;

    private string TextoContagemAlunos
    {
        get
        {
            if (_alunos is null)
            {
                return "Carregando...";
            }

            var ativos = QuantidadeAtivos == 1 ? "1 ativo" : $"{QuantidadeAtivos} ativos";
            return QuantidadeInativos == 0
                ? ativos
                : $"{ativos} · {(QuantidadeInativos == 1 ? "1 inativo" : $"{QuantidadeInativos} inativos")}";
        }
    }

    private string TituloVazio => QuantidadeInativos > 0 && !_mostrarInativos
        ? "Nenhum aluno ativo"
        : "Nenhum aluno matriculado";

    private string DescricaoVazio => QuantidadeInativos > 0 && !_mostrarInativos
        ? "Há alunos inativos nesta turma. Ligue \"Mostrar inativos\" para vê-los."
        : _turma?.Ativo == true
            ? "Inclua os alunos já cadastrados de uma vez, ou cadastre um aluno novo."
            : "Esta turma não teve alunos matriculados.";

    // A rota pode mudar de /turmas/1 para /turmas/2 sem recriar o componente.
    protected override Task OnParametersSetAsync() => _turma?.Id == Id ? Task.CompletedTask : CarregarAsync();

    private async Task CarregarAsync()
    {
        _carregandoTurma = true;
        _erroTurma = null;
        _turma = null;
        _alunos = null;

        // Aviso deixado pela edicao do aluno (voltar=/turmas/{id}).
        if (Avisos.Consumir() is { } aviso)
        {
            MostrarAviso(new AvisoPagina(aviso.Titulo, aviso.Mensagem));
        }

        // As duas consultas sao independentes: rodam em paralelo.
        var turmaTask = TurmaApi.ObterAsync(Id, _cts.Token);
        var alunosTask = CarregarAlunosAsync();

        var resultado = await turmaTask;

        _carregandoTurma = false;

        if (resultado.Succeeded)
        {
            _turma = resultado.Value;
        }
        else
        {
            _erroTurma = resultado.Error;
        }

        await alunosTask;
    }

    private async Task CarregarAlunosAsync()
    {
        _erroAlunos = null;
        _alunos = null;

        var resultado = await TurmaApi.ListarAlunosAsync(Id, _cts.Token);

        if (resultado.Succeeded)
        {
            _alunos = [.. resultado.Value!];
        }
        else
        {
            _erroAlunos = resultado.Error;
        }

        StateHasChanged();
    }

    /// <summary>
    /// Abre o cadastro do aluno e, ao salvar ou cancelar, volta para esta turma.
    /// </summary>
    private string LinkCadastro(TurmaAluno aluno)
        => $"usuarios/{aluno.UsuarioId}?voltar={Uri.EscapeDataString($"/turmas/{Id}")}&origem={Uri.EscapeDataString(_turma?.Nome ?? "Turma")}";

    /// <summary>
    /// Boletim do aluno (gráficos e notas), com a trilha de volta para esta turma.
    /// </summary>
    private string LinkBoletim(TurmaAluno aluno)
        => $"alunos/{aluno.AlunoId}/boletim?voltar={Uri.EscapeDataString($"/turmas/{Id}")}&origem={Uri.EscapeDataString(_turma?.Nome ?? "Turma")}";

    /// <summary>
    /// Cadastro de aluno novo já com esta turma marcada; ao salvar ou cancelar, volta para cá.
    /// </summary>
    private void CadastrarAluno()
        => Navigation.NavigateTo(
            $"usuarios/novo?turma={Id}&voltar={Uri.EscapeDataString($"/turmas/{Id}")}&origem={Uri.EscapeDataString(_turma?.Nome ?? "Turma")}");

    // ---------- Matriculas ----------

    private Task AbrirMatriculaAsync() => _matricula.AbrirAsync();

    private void AoMatricular(MatriculaConcluida concluida)
    {
        AtualizarAlunos(concluida.Alunos);

        var texto = concluida.Quantidade == 1 ? "1 aluno foi matriculado" : $"{concluida.Quantidade} alunos foram matriculados";
        MostrarAviso(new AvisoPagina("Matrícula concluída", $"{texto} em {_turma?.Nome}."));
    }

    private async Task SolicitarRemocaoAsync(TurmaAluno aluno)
    {
        _alvoRemocao = aluno;
        _erroConfirmacao = null;
        await _confirmarRemocao.AbrirAsync();
    }

    private async Task ConfirmarRemocaoAsync()
    {
        if (_alvoRemocao is not { } aluno || _processando)
        {
            return;
        }

        _processando = true;
        _erroConfirmacao = null;

        var resultado = await TurmaApi.RemoverAlunoAsync(Id, aluno.UsuarioId, _cts.Token);

        _processando = false;

        // 404: alguem ja tirou o aluno da turma; a lista local tambem deve refletir isso.
        if (!resultado.Succeeded && resultado.Error.Kind != ApiErrorKind.NotFound)
        {
            _erroConfirmacao = resultado.Error;
            return;
        }

        await _confirmarRemocao.FecharAsync();
        _alvoRemocao = null;

        if (resultado.Succeeded)
        {
            AtualizarAlunos(resultado.Value!);
        }
        else
        {
            await CarregarAlunosAsync();
        }

        MostrarAviso(new AvisoPagina("Aluno removido da turma", $"{aluno.Nome} não faz mais parte de {_turma?.Nome}."));
    }

    /// <summary>
    /// Troca a lista de alunos pela devolvida pela API e atualiza o total exibido no cabeçalho.
    /// </summary>
    private void AtualizarAlunos(IReadOnlyList<TurmaAluno> alunos)
    {
        _alunos = [.. alunos];

        if (_turma is not null)
        {
            _turma = _turma with { QuantidadeAlunos = QuantidadeAtivos };
        }
    }

    private void VoltarParaLista() => Navigation.NavigateTo("turmas");

    private Task EditarTurmaAsync() => _formulario.AbrirEdicaoAsync(_turma!);

    private void AoSalvarTurma(TurmaSalva salva)
    {
        _turma = salva.Turma;
        MostrarAviso(new AvisoPagina("Turma atualizada", "As alterações foram salvas."));
    }

    private async Task SolicitarInativacaoAsync()
    {
        _erroConfirmacao = null;
        await _confirmarInativacao.AbrirAsync();
    }

    private async Task ConfirmarInativacaoAsync()
    {
        if (_turma is null || _processando)
        {
            return;
        }

        _processando = true;
        _erroConfirmacao = null;

        var resultado = await TurmaApi.InativarAsync(_turma.Id, _cts.Token);

        _processando = false;

        if (!resultado.Succeeded)
        {
            _erroConfirmacao = resultado.Error;
            return;
        }

        await _confirmarInativacao.FecharAsync();
        _turma = resultado.Value;
        MostrarAviso(new AvisoPagina("Turma inativada", $"{_turma!.Nome} foi inativada. Você pode reativá-la a qualquer momento."));
    }

    private async Task ReativarAsync()
    {
        if (_turma is null || _reativando)
        {
            return;
        }

        _reativando = true;
        var resultado = await TurmaApi.ReativarAsync(_turma.Id, _cts.Token);
        _reativando = false;

        if (!resultado.Succeeded)
        {
            MostrarAviso(new AvisoPagina(resultado.Error.Title, resultado.Error.Message, Erro: true));
            return;
        }

        _turma = resultado.Value;
        MostrarAviso(new AvisoPagina("Turma reativada", $"{_turma!.Nome} voltou a aceitar matrículas."));
    }

    private void MostrarAviso(AvisoPagina aviso)
    {
        _aviso = aviso;
        _avisoId++;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
