using System.ComponentModel.DataAnnotations;
using EduCore.Web.Client.Shared;

namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Estado editável do formulário de usuário (cadastro e edição).
/// Atributos validam cada campo ao sair dele; <see cref="ValidarRegras"/> cobre as regras que dependem de outros campos.
/// </summary>
public sealed class UsuarioFormModel : IRegrasDeFormulario
{
    public const string CargoPadrao = "Professor";

    /// <summary>
    /// Na edição a senha é opcional (em branco mantém a atual) e o tipo não muda.
    /// </summary>
    public bool Edicao { get; private init; }

    public TipoUsuario Tipo { get; set; } = TipoUsuario.Aluno;

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido, como nome@escola.com.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    public string? Senha { get; set; }

    public string? ConfirmacaoSenha { get; set; }

    public List<int> TurmaIds { get; set; } = [];

    [StringLength(100, ErrorMessage = "A especialidade deve ter no máximo 100 caracteres.")]
    public string? Especialidade { get; set; }

    [StringLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres.")]
    public string? Cargo { get; set; } = CargoPadrao;

    public DateOnly? DataAdmissao { get; set; }

    public TipoContrato? TipoContrato { get; set; }

    public bool IsProfessor => Tipo == TipoUsuario.Professor;

    public static UsuarioFormModel ParaCadastro() => new();

    public static UsuarioFormModel ParaEdicao(UsuarioDetalhe usuario) => new()
    {
        Edicao = true,
        Tipo = usuario.IsAluno ? TipoUsuario.Aluno : TipoUsuario.Professor,
        Nome = usuario.Nome,
        Email = usuario.Email,
        TurmaIds = [.. usuario.TurmaIds],
        Especialidade = usuario.Professor?.Especialidade,
        Cargo = string.IsNullOrWhiteSpace(usuario.Professor?.Cargo) ? CargoPadrao : usuario.Professor.Cargo,
        DataAdmissao = usuario.Professor?.DataAdmissao,
        TipoContrato = usuario.Professor?.TipoContrato
    };

    public CriarUsuarioRequest ParaCriacao() => new(
        Tipo,
        Nome.Trim(),
        Email.Trim(),
        Senha!,
        IsProfessor ? [] : TurmaIds,
        DadosProfessor());

    public AtualizarUsuarioRequest ParaAtualizacao() => new(
        Nome.Trim(),
        Email.Trim(),
        string.IsNullOrEmpty(Senha) ? null : Senha,
        IsProfessor ? [] : TurmaIds,
        DadosProfessor());

    /// <summary>
    /// Converte a chave de erro da API ("Professor.Especialidade", "NovaSenha"...) no campo do formulário.
    /// Retorna nulo quando o erro não pertence a um campo visível.
    /// </summary>
    public static string? CampoDoErroDaApi(string chave)
    {
        var nome = chave[(chave.LastIndexOf('.') + 1)..];

        if (nome.Equals("NovaSenha", StringComparison.OrdinalIgnoreCase))
        {
            return nameof(Senha);
        }

        string[] campos =
        [
            nameof(Tipo), nameof(Nome), nameof(Email), nameof(Senha), nameof(TurmaIds),
            nameof(Especialidade), nameof(Cargo), nameof(DataAdmissao), nameof(TipoContrato)
        ];

        return campos.FirstOrDefault(c => c.Equals(nome, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<ValidationResult> ValidarRegras()
    {
        if (!Edicao && string.IsNullOrEmpty(Senha))
        {
            yield return new ValidationResult("Informe a senha.", [nameof(Senha)]);
        }

        if (!string.IsNullOrEmpty(Senha) && Senha != ConfirmacaoSenha)
        {
            yield return new ValidationResult("As senhas não conferem.", [nameof(ConfirmacaoSenha)]);
        }

        if (!IsProfessor)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(Especialidade))
        {
            yield return new ValidationResult("Informe a especialidade.", [nameof(Especialidade)]);
        }

        if (string.IsNullOrWhiteSpace(Cargo))
        {
            yield return new ValidationResult("Informe o cargo.", [nameof(Cargo)]);
        }

        if (DataAdmissao is null)
        {
            yield return new ValidationResult("Informe a data de admissão.", [nameof(DataAdmissao)]);
        }

        if (TipoContrato is null)
        {
            yield return new ValidationResult("Selecione o tipo de contrato.", [nameof(TipoContrato)]);
        }
    }

    private DadosProfessorRequest? DadosProfessor() => IsProfessor
        ? new DadosProfessorRequest(Especialidade!.Trim(), Cargo!.Trim(), DataAdmissao!.Value, TipoContrato!.Value)
        : null;
}
