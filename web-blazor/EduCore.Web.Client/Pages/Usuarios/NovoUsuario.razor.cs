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

    protected override Task OnInitializedAsync() => CarregarTurmasAsync();

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
