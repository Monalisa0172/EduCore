using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EduCore.API.Enums;

namespace EduCore.API.DTOs.Request;

/// <summary>
/// Campos comuns ao cadastro e à edição de alunos e professores.
/// </summary>
public abstract class SalvarUsuarioRequest
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(100, ErrorMessage = "O e-mail deve ter no máximo 100 caracteres.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Turmas ativas do aluno. Ignorado para professor.
    /// </summary>
    public List<int> TurmaIds { get; set; } = new();

    /// <summary>
    /// Obrigatório quando o usuário é professor.
    /// </summary>
    public DadosProfessorRequest? Professor { get; set; }
}

public class CreateUsuarioRequest : SalvarUsuarioRequest
{
    [Required(ErrorMessage = "Informe o tipo de usuário.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TipoUsuarioEnum? Tipo { get; set; }

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    public string Senha { get; set; } = string.Empty;
}

public class UpdateUsuarioRequest : SalvarUsuarioRequest
{
    /// <summary>
    /// Opcional: quando vazio, a senha atual é mantida.
    /// </summary>
    [StringLength(100, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 100 caracteres.")]
    public string? NovaSenha { get; set; }
}

public class DadosProfessorRequest
{
    [Required(ErrorMessage = "Informe a especialidade.")]
    [StringLength(100, ErrorMessage = "A especialidade deve ter no máximo 100 caracteres.")]
    public string Especialidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o cargo.")]
    [StringLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres.")]
    public string Cargo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data de admissão.")]
    public DateOnly? DataAdmissao { get; set; }

    [Required(ErrorMessage = "Informe o tipo de contrato.")]
    [EnumDataType(typeof(TipoContratoEnum), ErrorMessage = "Tipo de contrato inválido.")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TipoContratoEnum? TipoContrato { get; set; }
}
