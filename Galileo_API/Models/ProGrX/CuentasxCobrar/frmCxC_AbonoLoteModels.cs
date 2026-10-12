using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public sealed class CxCAbonoLoteFacturasRequest
    {
        public string cedula { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;

        public string? cod_factura { get; set; }
    }

    public sealed class CxCAbonoLoteDetalleRequest
    {
        [JsonRequired]
        public int operacion { get; set; } = 0;

        public string cod_factura { get; set; } = string.Empty;

        [JsonRequired]
        public decimal monto { get; set; } = 0;
    }

    public sealed class CxCAbonoLoteAplicarRequest
    {
        public string caja { get; set; } = string.Empty;

        [JsonRequired]
        public int apertura { get; set; } = 0;

        [JsonRequired]
        public int sesion_id { get; set; } = 0;

        public string tiquete { get; set; } = string.Empty;

        public string tipo_documento { get; set; } = string.Empty;

        public string usuario { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public string cod_divisa { get; set; } = string.Empty;

        [JsonRequired]
        public decimal total_detallado { get; set; } = 0;

        [JsonRequired]
        public List<CxCAbonoLoteDetalleRequest> facturas { get; set; } = [];
    }

    public sealed class CxCAbonoLoteAplicarData
    {
        public string tipo_documento { get; set; } = string.Empty;

        public string numero_documento { get; set; } = string.Empty;

        public decimal total_aplicado { get; set; } = 0;

        public int cantidad_facturas { get; set; } = 0;
    }
}