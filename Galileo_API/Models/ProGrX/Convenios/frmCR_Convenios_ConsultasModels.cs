namespace Galileo_API.Models.ProGrX.Convenios
{
    public class CrConvenioConsultaData
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrConvenioConsultaFiltros
    {
        public string cod_convenio { get; set; } = string.Empty;
        public bool? todos_tipos { get; set; }
        public List<string> tipos { get; set; } = new();
        public bool? todas_fechas { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string usuario { get; set; } = "TODOS";
        public string estado { get; set; } = "TODOS";
    }

    public class CrConvenioOrdenData
    {
        public int cod_orden { get; set; }
        public string cod_factura { get; set; } = string.Empty;
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo_convenio { get; set; } = string.Empty;
        public string tipo_convenio_desc { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public decimal? total_pagar { get; set; }
        public decimal? total_orden { get; set; }
        public decimal? total_deducciones { get; set; }
        public decimal? comisiones_nuevos_creditos { get; set; }
        public decimal? comisiones_recaudacion { get; set; }
        public decimal? total_reserva { get; set; }
        public decimal? total_rebajos_cxp { get; set; }
        public decimal? retencion_ahorros { get; set; }
        public decimal? iva_total { get; set; }
    }

    public class CrConvenioOrdenesConsulta
    {
        public int casos { get; set; }
        public decimal total_pagar { get; set; }
        public decimal total_orden { get; set; }
        public decimal total_deducciones { get; set; }
        public List<CrConvenioOrdenData> lista { get; set; } = new();
    }

    /// <summary>
    /// Resultado de spConvenios_Ordenes_DetallePago; los nombres siguen las columnas del SP.
    /// </summary>
    public class CrConvenioDetallePagoData
    {
        public int cod_orden { get; set; }
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal? ordenTotalNeto { get; set; }
        public decimal? totalDeducciones { get; set; }
        public decimal? total_pagar { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string ordenDocumento { get; set; } = string.Empty;
        public string cod_factura { get; set; } = string.Empty;
        public decimal? facturaTotal { get; set; }
        public decimal? cargos { get; set; }
        public decimal? montoDesembolso { get; set; }
        public int? nSolicitud { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public string beneficiario { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string nDocumento { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
        public DateTime? fecha_emision { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public List<CrConvenioOrdenCanceladaData> ordenes_canceladas { get; set; } = new();
    }

    /// <summary>
    /// Resultado de spConvenios_OrdenesCanceladas.
    /// </summary>
    public class CrConvenioOrdenCanceladaData
    {
        public int cod_orden { get; set; }
        public DateTime? fecha_emision { get; set; }
        public decimal? total_pagar { get; set; }
    }
}
