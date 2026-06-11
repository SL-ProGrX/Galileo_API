namespace Galileo.Models.AH
{
    public class ExcedentePeriodoDto
    {
        public int id_periodo { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public string estado { get; set; } = string.Empty;
        public decimal reserva_porc { get; set; }
        public decimal capitaliza_porc { get; set; }
        public bool capitaliza_renta_aplica { get; set; }
        public string nc_saldos { get; set; } = string.Empty;
        public string nc_mora { get; set; } = string.Empty;
        public string nc_opcf { get; set; } = string.Empty;
        public string nc_fnd_extra { get; set; } = string.Empty;
        public string nc_extraordinarios { get; set; } = string.Empty;
        public string doc_asiento { get; set; } = string.Empty;
        public bool visible_webapp { get; set; }
        public bool visible_sys { get; set; }
        public bool mostrar_en_historial { get; set; }
        public bool mostrar_tabla_renta { get; set; }
        public string estado_notas { get; set; } = string.Empty;
        public bool modo_automatico { get; set; }
        public string tipo_apl_mensual { get; set; } = string.Empty;
        public string tipo_apl_mensual_desc { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosListaDto
    {
        public int id_periodo { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public string estado { get; set; } = string.Empty;
        public decimal reserva_porc { get; set; }
        public decimal capitaliza_porc { get; set; }
        public string capitaliza_renta_aplica_desc { get; set; } = string.Empty;
        public string nc_saldos { get; set; } = string.Empty;
        public string nc_mora { get; set; } = string.Empty;
        public string nc_opcf { get; set; } = string.Empty;
        public string visible_webapp_desc { get; set; } = string.Empty;
        public string visible_sys_desc { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosDetalleDto
    {
        public ExcedentePeriodoDto periodo { get; set; } = new();
        public List<RentaExcedenteDto> renta_tabla { get; set; } = [];
    }

    public class FrmAhExcedentesPeriodosResumenDto
    {
        public string concepto { get; set; } = string.Empty;
        public decimal aplicado { get; set; }
        public decimal cargado { get; set; }
        public bool destacado { get; set; }
        public int orden { get; set; }
    }

    public class BitacoraExcedenteDto
    {
        public int linea { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string proceso_desc { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string cod_transaccion { get; set; } = string.Empty;
        public int casos { get; set; }
        public decimal monto { get; set; }
        public string time_inicio { get; set; } = string.Empty;
        public string time_corte { get; set; } = string.Empty;
        public string duracion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosGuardarRequest
    {
        public int id_periodo { get; set; } = 0;
        public DateTime inicio { get; set; } = DateTime.Now;
        public DateTime corte { get; set; } = DateTime.Now;
        public decimal reserva_porc { get; set; } = 0;
        public decimal capitaliza_porc { get; set; } = 0;
        public bool capitaliza_renta_aplica { get; set; } = false;
        public bool visible_webapp { get; set; } = false;
        public bool visible_sys { get; set; } = false;
        public bool mostrar_en_historial { get; set; } = false;
        public bool mostrar_tabla_renta { get; set; } = false;
        public string estado_notas { get; set; } = string.Empty;
        public bool modo_automatico { get; set; } = false;
        public string tipo_apl_mensual { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosGuardarResponse
    {
        public int id_periodo { get; set; }
        public string accion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosBaseAplicacionRequest
    {
        public int id_periodo { get; set; } = 0;
        public string tipo_apl_mensual { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosEstadoNotaRequest
    {
        public int id_periodo { get; set; } = 0;
        public string estado_notas { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosRecalcularBaseRequest
    {
        public int id_periodo { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosEstadoInternoDto
    {
        public int id_periodo { get; set; } = 0;
        public string estado { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosBitacoraRegistrarRequest
    {
        public int periodoId { get; set; } = 0;
        public string codProceso { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string tipoDocumento { get; set; } = string.Empty;
        public string codTransaccion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public int casos { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
    }

    public class FrmAhExcedentesPeriodosVisibilidadRequest
    {
        public int id_periodo { get; set; } = 0;
        public string campo { get; set; } = string.Empty;
        public bool valor { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
    }

}
