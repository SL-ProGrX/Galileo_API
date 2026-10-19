namespace Galileo_API.Models.ProGrX.Convenios
{
    /// <summary>
    /// Línea del asiento derivado de la orden (spConvenios_Orden_Asiento).
    /// </summary>
    public class CrOrdenAsientoLinea
    {
        public string? cod_convenio { get; set; }
        public int? cod_orden { get; set; }
        public int? cod_proveedor { get; set; }
        public string? cod_factura { get; set; }
        public int? cod_contabilidad { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }
        public string? cod_cuenta { get; set; }
        public string? cod_cuenta_mask { get; set; }
        public string? cod_divisa { get; set; }
        public decimal? tipo_cambio { get; set; }
        public decimal? importe { get; set; }
        public decimal? monto_debito { get; set; }
        public decimal? monto_credito { get; set; }
        public int? linea { get; set; }
        public string? descripcion { get; set; }
    }

    /// <summary>
    /// Resultado de la consulta del asiento: líneas, totales y estado (lblEstado del VB6).
    /// </summary>
    public class CrOrdenAsientoConsulta
    {
        public List<CrOrdenAsientoLinea> lineas { get; set; } = new();
        public decimal total_debitos { get; set; }
        public decimal total_creditos { get; set; }
        /// <summary>"Abierta", "Cerrada" o null si la orden no tiene líneas.</summary>
        public string? estado { get; set; }
    }
}
