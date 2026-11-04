using System.Text.Json.Serialization;

namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Perfis que o Admin pode cadastrar. Trafega como texto ("Aluno"/"Professor").
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TipoUsuario>))]
public enum TipoUsuario
{
    Aluno,
    Professor
}
