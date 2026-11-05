using EduCore.Web.Client.Api;
using EduCore.Web.Client.Shared;
using EduCore.Web.Client.Turmas.Models;
using EduCore.Web.Client.Turmas.Services;
using EduCore.Web.Client.Usuarios.Models;
using EduCore.Web.Client.Usuarios.Services;
using Microsoft.AspNetCore.Components;

namespace EduCore.Web.Client.Pages.Usuarios;

public partial class NovoUsuario : IDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly UsuarioFormModel _modelo = UsuarioFormModel.ParaCadastro();

    private IReadOnlyList<TurmaOpcao>? _turmas;
    private ApiError? _erroTurmas;

    [Inject]
    private IUsuarioApiClient UsuarioApi { get; set; } = default!;

    [Inject]
    private ITurmaApiClient TurmaApi { get; set; } = default!;

    [Inject]
    private AvisoNavegacao Avisos { get; set; } = default!;

    /// <summary>Turma já marcada no cadastro (vindo de "Cadastrar aluno" na turma).</summary>
    [SupplyParameterFromQuery(Name = "turma")]
    public int? Turma { get; set; }

    /// <summary>Página de origem: salvar ou cancelar volta para ela em vez do painel.</summary>
    [SupplyParameterFromQuery(Name = "voltar")]
    public string? Voltar { get; set; }

    [SupplyParameterFromQuery(Name = "origem")]
    public string? Origem { get; set; }

    private string UrlRetorno => RetornoNavegacao.CaminhoInterno(Voltar) ?? string.Empty;

    private IReadOnlyList<ItemTrilha>? Trilha => RetornoNavegacao.TrilhaDaTurma(Voltar, Origem, "Novo usuário");

    protected override Task OnInitializedAsync()
    {
        // A turma so fica marcada se estiver entre as ativas (o formulario so exibe essas).
        if (Turma is > 0)
        {
            _modelo.TurmaIds = [Turma.Value];
        }

        return CarregarTurmasAsync();
    }

    private async Task CarregarTurmasAsync()
    {
        _erroTurmas = null;
        _turmas = null;

        var resultado = await TurmaApi.GetAtivasAsync(_cts.Token);

        if (resultado.Succeeded)
        {
            _turmas = resultado.Value;
        }
        else
        {
            _erroTurmas = resultado.Error;
        }
    }

    private async Task<ApiError?> CriarAsync(UsuarioFormModel modelo, CancellationToken cancellationToken)
    {
        var resultado = await UsuarioApi.CriarAsync(modelo.ParaCriacao(), cancellationToken);
        if (!resultado.Succeeded)
        {
            return resultado.Error;
        }

        var usuario = resultado.Value!;
        Avisos.Publicar(
            "Usuário cadastrado",
            $"{usuario.Nome} já pode entrar no sistema com o e-mail {usuario.Email}.");

        return null;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
