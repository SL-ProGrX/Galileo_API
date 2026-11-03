using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX_Contabilidad;

public class ArfGestionPagosFiltroDto
{
    [JsonRequired]
    public DateTime Corte { get; set; }
    public string Arrendador { get; set; } = string.Empty;
    public string Unidad { get; set; } = string.Empty;
}

public class ArfGestionPagoDto
{
    public long Operacion { get; set; }
    public decimal Id_Seq { get; set; }
    public DateTime Corte { get; set; }
    [JsonPropertyName("divisA_DESC")]
    public string DIVISA_DESC { get; set; } = string.Empty;
    public decimal Cuota { get; set; }
    [JsonPropertyName("unidaD_DESC")]
    public string UNIDAD_DESC { get; set; } = string.Empty;
    [JsonPropertyName("arrendatariO_DESC")]
    public string ARRENDATARIO_DESC { get; set; } = string.Empty;
}

public class ArfGenerarPagoDto
{
    [JsonRequired]
    public long Operacion { get; set; }
    [JsonRequired]
    public decimal Id_Seq { get; set; }
    [JsonRequired]
    public DateTime Corte { get; set; }
    [JsonRequired]
    public decimal Cuota { get; set; }
}
