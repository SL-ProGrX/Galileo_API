namespace Galileo_API.Models.ProGrX_Nucleo
{
    /// <summary>
    /// Filtros de las listas del integrador APL. linea_id nulo equivale a "TODAS".
    /// El dominio APL lo resuelve el API desde SIF_EMPRESA.APL_Dominio.
    /// </summary>
    public class AplListaRequest
    {
        public string estado { get; set; } = string.Empty;
        public string? linea_id { get; set; }
        public bool atendidas { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class AplLineaDto
    {
        public string? item_id { get; set; }
        public string? item_desc { get; set; }
    }

    /// <summary>
    /// Fila cruda de spProGrX_APL_Lineas: Item_Id puede venir numérico o texto.
    /// </summary>
    public class AplLineaRaw
    {
        public object? item_id { get; set; }
        public string? item_desc { get; set; }
    }

    public class AplTramiteDto
    {
        public long? apl_operacion { get; set; }
        public string? linea_desc { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal? factura_monto { get; set; }
        public int? plazo { get; set; }
        public decimal? tasa { get; set; }
        public decimal? cuota { get; set; }
        public string? plan_desc { get; set; }
        public string? institucion_desc { get; set; }
        public string? factura_numero { get; set; }
        public long? operacion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? atiende_fecha { get; set; }
        public string? atiende_usuario { get; set; }
        public int? minutos { get; set; }
    }

    public class AplSolicitudDto
    {
        public long? num_solicitud { get; set; }
        public string? linea_desc { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal? monto { get; set; }
        public int? plazo { get; set; }
        public string? institucion_desc { get; set; }
        public string? notas { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? atiende_fecha { get; set; }
        public string? atiende_usuario { get; set; }
        public int? minutos { get; set; }
    }

    /// <summary>
    /// Resultado de spProGrX_APL_Monitor: casos pendientes de resolver.
    /// </summary>
    public class AplMonitorDto
    {
        public int solicitud { get; set; }
        public int tramite { get; set; }
    }
}
