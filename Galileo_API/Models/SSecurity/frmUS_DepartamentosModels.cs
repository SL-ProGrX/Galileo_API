using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Galileo.Models.Security;

public class DepartamentoRequest
{
    [Required, StringLength(10)]
    [JsonPropertyName("cod_departamento")]
    public string Cod_Departamento { get; set; } = string.Empty;

    [Required, StringLength(100)]
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [Range(0, 1)]
    [JsonPropertyName("activo")]
    public short Activo { get; set; } = 1;
}

public sealed class Departamento : DepartamentoRequest
{
    [JsonPropertyName("registro_fecha")]
    public DateTime? Registro_Fecha { get; set; }
    [JsonPropertyName("registro_usuario")]
    public string Registro_Usuario { get; set; } = string.Empty;
    [JsonPropertyName("modifica_fecha")]
    public DateTime? Modifica_Fecha { get; set; }
    [JsonPropertyName("modifica_usuario")]
    public string? Modifica_Usuario { get; set; }
}
