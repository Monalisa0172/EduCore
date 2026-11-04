using System.Text.Json.Serialization;
using EduCore.API.Enums;

namespace EduCore.API.DTOs.Response;

/// <summary>
/// Dados de um aluno ou professor para a tela de edição.
/// </summary>
public record UsuarioDetalheDTO(
    int UsuarioId,
    string Nome,
    string Email,
    string Tipo,
    bool Ativo,
    IReadOnlyList<int> TurmaIds,
    DadosProfessorDTO? Professor);

/// <summary>
/// Nulo em <see cref="UsuarioDetalheDTO.Professor"/> quando o professor ainda não tem registro de funcionário.
/// </summary>
public record DadosProfessorDTO(
    string Especialidade,
    string Cargo,
    DateOnly DataAdmissao,
    [property: JsonConverter(typeof(JsonStringEnumConverter))] TipoContratoEnum? TipoContrato);
