using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.Cajas
{

    public class CajasCreditoDto
    {
        [JsonPropertyName("id_solicitud")]
        public int? Id_Solicitud { get; set; }

        [JsonPropertyName("codigo")]
        public string? Codigo { get; set; }

        [JsonPropertyName("garantia")]
        public string? GarantiaDesc { get; set; }

        public decimal? saldo { get; set; }
        public decimal? mora { get; set; }
        public decimal? cuota { get; set; }

        [JsonPropertyName("linea_desc")]
        public string? LineaDesc { get; set; }
    }

    public class CajasFondosDto
    {
        [JsonPropertyName("contrato")]
        public string? Cod_Contrato { get; set; }

        [JsonPropertyName("plan")]
        public string? Cod_Plan { get; set; }
        public decimal? aportes { get; set; }
        public decimal? rendimiento { get; set; }
        public decimal? acumulado { get; set; }
        public decimal? monto { get; set; }
        [JsonPropertyName("descripcion")]
        public string? PlanDesc { get; set; }
    }

    public class CajasCxcDto
    {
        public string? operacion { get; set; }
        public string? documento { get; set; }
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public decimal? saldo { get; set; }
        public decimal? cuota { get; set; }
        public string? estado { get; set; }
    }

    public class CajasServiciosDto
    {
        public string? servicio { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha { get; set; }
        public string? referencia { get; set; }
        public string? caja { get; set; }
        public string? usuario { get; set; }
    }

    public class CajasSaldoFavorDto
    {
        public int? linea { get; set; }
        public string? documento { get; set; }
        public DateTime? fecha { get; set; }
        public decimal? monto { get; set; }
        public decimal? saldo { get; set; }
        public string? referencia { get; set; }
    }

    public class CajasReciboMultipleDto
    {
        public int? recibo { get; set; }
        public decimal? monto { get; set; }
        public DateTime? fecha { get; set; }
        public string? caja { get; set; }
        public int? apertura { get; set; }
        public string? usuario { get; set; }
    }

    public class CajasDatosPersonaDto
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public bool cobroJudicial { get; set; }
        public bool incobrable { get; set; }
        public bool expedienteRestringido { get; set; }
        public string? mensaje { get; set; }
    }

}
