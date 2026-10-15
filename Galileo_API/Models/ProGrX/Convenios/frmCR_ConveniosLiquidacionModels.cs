namespace Galileo_API.Models.ProGrX.Convenios
{
    /// <summary>
    /// Datos del convenio consultado (sbConsulta) más los valores generales de la liquidación.
    /// </summary>
    public class CrLiqConvenioData
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string contrato_numero { get; set; } = string.Empty;
        public DateTime? fecha_vence { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public decimal? porc_comision_creditos { get; set; }
        public decimal? porc_comision_recaudacion { get; set; }
        public int? comision_informativa { get; set; }
        public int? cobra_creditos_anulados { get; set; }
        public decimal? flotante { get; set; }
        public DateTime? fecha_corte_general { get; set; }
    }

    /// <summary>
    /// Campos de la orden que comparten la consulta (sbConsultaOrden) y el guardado (sbGuardaOrden).
    /// </summary>
    public abstract class CrLiqOrdenCamposComunes
    {
        public string notas { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal? recaudacion_cargos { get; set; }
        public decimal? recaudacion_cuotas { get; set; }
        public decimal? recaudacion_devolucion { get; set; }
        public decimal? nuevos_creditos { get; set; }
        public decimal? comisiones_recaudacion { get; set; }
        public decimal? comisiones_nuevos_creditos { get; set; }
        public decimal? total_reserva { get; set; }
        public decimal? total_rebajos_cxp { get; set; }
        public decimal? retencion_ahorros { get; set; }
        public decimal? total_pagar { get; set; }
    }

    /// <summary>
    /// Fila del tab Órdenes (sbConsultaOrdenes).
    /// </summary>
    public class CrLiqOrdenListaData
    {
        public int cod_orden { get; set; }
        public string cod_convenio { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal? recaudacion_cuotas { get; set; }
        public decimal? recaudacion_cargos { get; set; }
        public decimal? nuevos_creditos { get; set; }
        public decimal? retencion_ahorros { get; set; }
        public decimal? comisiones_recaudacion { get; set; }
        public decimal? comisiones_nuevos_creditos { get; set; }
        public decimal? total_reserva { get; set; }
        public decimal? total_rebajos_cxp { get; set; }
        public decimal? total_pagar { get; set; }
        public string documento { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    /// <summary>
    /// Orden completa para el tab Liquidación (sbConsultaOrden).
    /// </summary>
    public class CrLiqOrdenData : CrLiqOrdenCamposComunes
    {
        public int cod_orden { get; set; }
        public string cod_convenio { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public decimal? iva_total { get; set; }
        public decimal? iva_referencia_crd { get; set; }
        public decimal? flotante_alcobro { get; set; }
        public int? apl_com_informativa { get; set; }
        public int? cobra_creditos_anulados { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    /// <summary>
    /// Resultado de fxValidaUltimaOrdenCerrada para el botón Nuevo.
    /// </summary>
    public class CrLiqNuevaOrdenData
    {
        public bool puede_crear { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_inicio_min { get; set; }
        public bool inicio_habilitado { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? fecha_corte_max { get; set; }
    }

    /// <summary>
    /// Entrada para Guardar y Cerrar (sbGuardaOrden). Los valores nulos se toman como 0.
    /// </summary>
    public class CrLiqOrdenGuardarRequest : CrLiqOrdenCamposComunes
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int? cod_orden { get; set; }
        public decimal? iva_referencia_crd { get; set; }
        public decimal? flotante_alcobro { get; set; }
        public int? apl_com_informativa { get; set; }
        public int? cobra_creditos_anulados { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Fila del tab Flotante al Cobro (spConvenio_CxP_CargosFlotantes).
    /// </summary>
    public class CrLiqFlotanteData
    {
        public string descripcion { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public DateTime? fecha { get; set; }
        public DateTime? vence { get; set; }
        public string documento { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }
}
