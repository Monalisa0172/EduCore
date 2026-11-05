using EduCore.Web.Client.Api;
using EduCore.Web.Client.Disciplinas.Components;
using EduCore.Web.Client.Disciplinas.Models;
using EduCore.Web.Client.Disciplinas.Services;
using EduCore.Web.Client.Shared;
using Microsoft.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.Disciplinas;

public partial class DisciplinaDetalhe : IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    // Disciplina
    private DisciplinaItem? _disciplina;
    private ApiError? _erroDisciplina;
    private bool _carregandoDisciplina = true;
    private bool _reativandoDisciplina;

    // Subdisciplinas (nulo enquanto carrega)
    private List<SubDisciplinaItem>? _subDisciplinas;
    private ApiError? _erroSub;
    private bool _mostrarInativas;

    // Dialogos
    private DisciplinaFormDialog _formDisciplina = default!;
    private SubDisciplinaFormDialog _formSub = default!;
    private ConfirmDialog _confirmarDisciplina = default!;
    private ConfirmDialog _confirmarSub = default!;
    private SubDisciplinaItem? _alvoSub;
    private ApiError? _erroConfirmacao;
    private bool _processando;

    private AvisoPagina? _aviso;
    private int _avisoId;

    [Parameter]
    public int Id { get; set; }

    [Inject]
    private IDisciplinaApiClient DisciplinaApi { get; set; } = default!;

    [Inject]
    private ISubDisciplinaApiClient SubDisciplinaApi { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private IReadOnlyList<SubDisciplinaItem> SubDisciplinasVisiveis =>
        (_subDisciplinas ?? []).Where(s => _mostrarInativas || s.Ativo).ToList();

    private int QuantidadeAtivas => _subDisciplinas?.Count(s => s.Ativo) ?? 0;

    private int QuantidadeInativas => _subDisciplinas?.Count(s => !s.Ativo) ?? 0;

    private string TextoContagemSub
    {
        get
        {
            if (_subDisciplinas is null)
            {
                return "Carregando...";
            }

            var ativas = QuantidadeAtivas == 1 ? "1 ativa" : $"{QuantidadeAtivas} ativas";
            return QuantidadeInativas == 0
                ? ativas
                : $"{ativas} · {(QuantidadeInativas == 1 ? "1 inativa" : $"{QuantidadeInativas} inativas")}";
        }
    }

    private string TituloVazioSub => QuantidadeInativas > 0 && !_mostrarInativas
        ? "Nenhuma subdisciplina ativa"
        : "Nenhuma subdisciplina cadastrada";

    private string DescricaoVazioSub => QuantidadeInativas > 0 && !_mostrarInativas
        ? "Há subdisciplinas inativas. Ligue \"Mostrar inativas\" para vê-las e reativá-las."
        : "Sem subdisciplinas, as avaliações são lançadas direto na disciplina.";

    // A rota pode mudar de /disciplinas/1 para /disciplinas/2 sem recriar o componente.
    protected override Task OnParametersSetAsync() => _disciplina?.Id == Id ? Task.CompletedTask : CarregarAsync();

    private async Task CarregarAsync()
    {
        _carregandoDisciplina = true;
        _erroDisciplina = null;
        _disciplina = null;
        _subDisciplinas = null;
        _aviso = null;

        // As duas consultas sao independentes: rodam em paralelo.
        var disciplinaTask = DisciplinaApi.ObterAsync(Id, _cts.Token);
        var subTask = CarregarSubDisciplinasAsync();

        var resultado = await disciplinaTask;

        _carregandoDisciplina = false;

        if (resultado.Succeeded)
        {
            _disciplina = resultado.Value;
        }
        else
        {
            _erroDisciplina = resultado.Error;
        }

        await subTask;
    }

    private async Task CarregarSubDisciplinasAsync()
    {
        _erroSub = null;
        _subDisciplinas = null;

        var resultado = await SubDisciplinaApi.ListarPorDisciplinaAsync(Id, _cts.Token);

        if (resultado.Succeeded)
        {
            _subDisciplinas = [.. resultado.Value!];
        }
        else
        {
            _erroSub = resultado.Error;
        }

        StateHasChanged();
    }

    private void VoltarParaLista() => Navigation.NavigateTo("disciplinas");

    /// <summary>Até duas iniciais para o marcador da subdisciplina ("Matemática Financeira" -> "MF").</summary>
    private static string Iniciais(string nome)
    {
        var palavras = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => p.Length > 2)
            .ToList();

        return palavras.Count switch
        {
            0 => nome.Length > 0 ? nome[..1].ToUpperInvariant() : "?",
            1 => palavras[0][..1].ToUpperInvariant(),
            _ => string.Concat(palavras[0][0], palavras[1][0]).ToUpperInvariant()
        };
    }

    // ---------- Disciplina ----------

    private static readonly IReadOnlyList<ItemTrilha> TrilhaBase =
        [ItemTrilha.Inicio, new("Disciplinas", "disciplinas"), new("Disciplina")];

    private Task EditarDisciplinaAsync() => _formDisciplina.AbrirEdicaoAsync(_disciplina!);

    // Abre a edicao ja destacando os campos que faltam (codigo e carga horaria).
    private Task CompletarCadastroAsync() => _formDisciplina.AbrirEdicaoAsync(_disciplina!, destacarPendencias: true);

    private void AoSalvarDisciplina(DisciplinaSalva salva)
    {
        _disciplina = salva.Disciplina;
        MostrarAviso(new AvisoPagina("Disciplina atualizada", "As alterações foram salvas."));
    }

    private async Task SolicitarInativacaoDisciplinaAsync()
    {
        _erroConfirmacao = null;
        await _confirmarDisciplina.AbrirAsync();
    }

    private async Task ConfirmarInativacaoDisciplinaAsync()
    {
        if (_disciplina is null || _processando)
        {
            return;
        }

        _processando = true;
        _erroConfirmacao = null;

        var resultado = await DisciplinaApi.InativarAsync(_disciplina.Id, _cts.Token);

        _processando = false;

        if (!resultado.Succeeded)
        {
            _erroConfirmacao = resultado.Error;
            return;
        }

        await _confirmarDisciplina.FecharAsync();
        _disciplina = resultado.Value;
        MostrarAviso(new AvisoPagina("Disciplina inativada", $"{_disciplina!.Nome} foi inativada. Você pode reativá-la a qualquer momento."));
    }

    private async Task ReativarDisciplinaAsync()
    {
        if (_disciplina is null || _reativandoDisciplina)
        {
            return;
        }

        _reativandoDisciplina = true;
        var resultado = await DisciplinaApi.ReativarAsync(_disciplina.Id, _cts.Token);
        _reativandoDisciplina = false;

        if (!resultado.Succeeded)
        {
            MostrarAviso(new AvisoPagina(resultado.Error.Title, resultado.Error.Message, Erro: true));
            return;
        }

        _disciplina = resultado.Value;
        MostrarAviso(new AvisoPagina("Disciplina reativada", $"{_disciplina!.Nome} está ativa novamente."));
    }

    // ---------- Subdisciplinas ----------

    private Task NovaSubDisciplinaAsync() => _formSub.AbrirNovaAsync(Id);

    private Task EditarSubDisciplinaAsync(SubDisciplinaItem sub) => _formSub.AbrirEdicaoAsync(sub);

    private void AoSalvarSubDisciplina(SubDisciplinaSalva salva)
    {
        SubstituirSub(salva.SubDisciplina);

        MostrarAviso(salva.Nova
            ? new AvisoPagina("Subdisciplina adicionada", $"{salva.SubDisciplina.Nome} foi adicionada a {_disciplina?.Nome}.")
            : new AvisoPagina("Subdisciplina atualizada", $"As alterações em {salva.SubDisciplina.Nome} foram salvas."));
    }

    private async Task SolicitarInativacaoSubAsync(SubDisciplinaItem sub)
    {
        _alvoSub = sub;
        _erroConfirmacao = null;
        await _confirmarSub.AbrirAsync();
    }

    private async Task ConfirmarInativacaoSubAsync()
    {
        if (_alvoSub is not { } sub || _processando)
        {
            return;
        }

        _processando = true;
        _erroConfirmacao = null;

        var resultado = await SubDisciplinaApi.InativarAsync(sub.Id, _cts.Token);

        _processando = false;

        if (!resultado.Succeeded)
        {
            _erroConfirmacao = resultado.Error;
            return;
        }

        await _confirmarSub.FecharAsync();
        _alvoSub = null;

        SubstituirSub(resultado.Value!);
        MostrarAviso(new AvisoPagina("Subdisciplina inativada", $"{sub.Nome} foi inativada. Ligue \"Mostrar inativas\" para reativá-la."));
    }

    private async Task ReativarSubAsync(SubDisciplinaItem sub)
    {
        var resultado = await SubDisciplinaApi.ReativarAsync(sub.Id, _cts.Token);

        if (!resultado.Succeeded)
        {
            MostrarAviso(new AvisoPagina(resultado.Error.Title, resultado.Error.Message, Erro: true));
            return;
        }

        SubstituirSub(resultado.Value!);
        MostrarAviso(new AvisoPagina("Subdisciplina reativada", $"{sub.Nome} está ativa novamente."));
    }

    /// <summary>
    /// Atualiza a lista local e a contagem exibida na disciplina, sem recarregar da API.
    /// </summary>
    private void SubstituirSub(SubDisciplinaItem sub)
    {
        _subDisciplinas ??= [];

        var indice = _subDisciplinas.FindIndex(s => s.Id == sub.Id);
        if (indice >= 0)
        {
            _subDisciplinas[indice] = sub;
        }
        else
        {
            _subDisciplinas.Add(sub);
        }

        _subDisciplinas.Sort((a, b) => string.Compare(a.Nome, b.Nome, StringComparison.CurrentCultureIgnoreCase));

        if (_disciplina is not null)
        {
            _disciplina = _disciplina with { QuantidadeSubDisciplinas = QuantidadeAtivas };
        }
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
