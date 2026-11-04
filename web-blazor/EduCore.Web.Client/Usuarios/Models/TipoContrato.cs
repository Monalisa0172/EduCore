using System.Text.Json.Serialization;

namespace EduCore.Web.Client.Usuarios.Models;

/// <summary>
/// Espelha EduCore.API.Enums.TipoContratoEnum. Trafega como texto ("Clt"/"Pj").
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<TipoContrato>))]
public enum TipoContrato
{
    Clt = 1,
    Pj = 2
}
