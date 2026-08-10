using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.Creditos
{
    /// <summary>
    /// Contenedor de las secciones del lsw de formalización que en el VB6 cierran con un total.
    /// </summary>
    public class CrSeguimientoTramitesFormalizacionDetalleLista<T>
    {
        public List<T> lista { get; set; } = new();
        public decimal total { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionRefundicionItem
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string tipo_descripcion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionDesembolsoItem
    {
        public string concepto { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public int retener { get; set; }
        public string retiene_descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionRefundeRetencionItem
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionFirmaItem
    {
        public string cedula { get; set; } = string.Empty;
        public string calidad { get; set; } = string.Empty;
        public string tipo_descripcion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public bool firma { get; set; }
        public string firma_descripcion { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionRequisitoItem
    {
        public string cod_requisito { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int visible { get; set; }
        public int estado { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionCargoItem
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string @base { get; set; } = string.Empty;
        public string base_descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string tipo_descripcion { get; set; } = string.Empty;
        public decimal valor { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionFiadorItem
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CrSeguimientoTramitesFormalizacionImpactoLiquidezData
    {
        public decimal cuota_nueva { get; set; }
        public decimal cuota_libera { get; set; }
        public decimal impacto { get; set; }
    }

    public class CrSeguimientoTramitesFormalizacionFirmaActualizarRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        public string cedula { get; set; } = string.Empty;
        /// <summary>D deudor, F fiador o co-deudor, según el Tag del lsw del VB6.</summary>
        public string calidad { get; set; } = string.Empty;
        [JsonRequired]
        public bool firma { get; set; }
    }

    public class CrSeguimientoTramitesMontoNoGravableRequest
    {
        [JsonRequired]
        public int operacion { get; set; }
        [JsonRequired]
        public decimal monto_no_gravable { get; set; }
        [JsonRequired]
        public decimal monto { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string estado_solicitud { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    internal sealed class CrSeguimientoTramitesFormalizacionFiadorFirmaRaw
    {
        public string cedulaf { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string calidad { get; set; } = string.Empty;
        public string firma { get; set; } = string.Empty;
    }

    internal sealed class CrSeguimientoTramitesFormalizacionDeudorFirmaRaw
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public int firma_deudor { get; set; }
    }
}
