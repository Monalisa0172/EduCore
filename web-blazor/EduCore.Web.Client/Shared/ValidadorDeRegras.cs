using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace EduCore.Web.Client.Shared;

/// <summary>
/// Regras que dependem de mais de um campo (ex.: "senha obrigatória só no cadastro").
/// </summary>
public interface IRegrasDeFormulario
{
    IEnumerable<ValidationResult> ValidarRegras();
}

/// <summary>
/// Validador para usar ao lado do DataAnnotationsValidator dentro de um EditForm.
/// </summary>
/// <remarks>
/// O IValidatableObject só roda quando todos os atributos passam, então o usuário veria os erros em duas levas.
/// Aqui as regras rodam junto com os atributos no envio e são reavaliadas a cada alteração,
/// mas só exibem erro novo no campo alterado (não "acendem" campos que o usuário ainda não tocou).
/// </remarks>
public sealed class ValidadorDeRegras : ComponentBase, IDisposable
{
    private ValidationMessageStore? _mensagens;
    private EditContext? _contextoAtual;
    private HashSet<FieldIdentifier> _camposComErro = [];

    [CascadingParameter]
    private EditContext EditContext { get; set; } = default!;

    protected override void OnParametersSet()
    {
        if (ReferenceEquals(EditContext, _contextoAtual))
        {
            return;
        }

        if (EditContext.Model is not IRegrasDeFormulario)
        {
            throw new InvalidOperationException(
                $"{nameof(ValidadorDeRegras)} exige um modelo que implemente {nameof(IRegrasDeFormulario)}.");
        }

        Desinscrever();

        _contextoAtual = EditContext;
        _mensagens = new ValidationMessageStore(EditContext);
        _camposComErro = [];
        EditContext.OnValidationRequested += AoValidarFormulario;
        EditContext.OnFieldChanged += AoAlterarCampo;
    }

    private void AoValidarFormulario(object? sender, ValidationRequestedEventArgs e)
    {
        var erros = Avaliar();

        _mensagens!.Clear();
        foreach (var (campo, mensagens) in erros)
        {
            _mensagens.Add(campo, mensagens);
        }

        _camposComErro = [.. erros.Keys];
    }

    private void AoAlterarCampo(object? sender, FieldChangedEventArgs e)
    {
        var erros = Avaliar();

        // Atualiza o campo alterado e os que ja mostravam erro (ex.: corrigir a senha limpa "senhas nao conferem").
        var afetados = new HashSet<FieldIdentifier>(_camposComErro) { e.FieldIdentifier };

        foreach (var campo in afetados)
        {
            _mensagens!.Clear(campo);
            if (erros.TryGetValue(campo, out var mensagens))
            {
                _mensagens.Add(campo, mensagens);
            }
        }

        _camposComErro = [.. afetados.Where(erros.ContainsKey)];
        EditContext.NotifyValidationStateChanged();
    }

    private Dictionary<FieldIdentifier, List<string>> Avaliar()
    {
        var modelo = EditContext.Model;
        var erros = new Dictionary<FieldIdentifier, List<string>>();

        foreach (var resultado in ((IRegrasDeFormulario)modelo).ValidarRegras())
        {
            foreach (var membro in resultado.MemberNames)
            {
                var campo = new FieldIdentifier(modelo, membro);
                if (!erros.TryGetValue(campo, out var lista))
                {
                    erros[campo] = lista = [];
                }

                lista.Add(resultado.ErrorMessage ?? string.Empty);
            }
        }

        return erros;
    }

    private void Desinscrever()
    {
        if (_contextoAtual is null)
        {
            return;
        }

        _contextoAtual.OnValidationRequested -= AoValidarFormulario;
        _contextoAtual.OnFieldChanged -= AoAlterarCampo;
        _mensagens?.Clear();
    }

    public void Dispose() => Desinscrever();
}
