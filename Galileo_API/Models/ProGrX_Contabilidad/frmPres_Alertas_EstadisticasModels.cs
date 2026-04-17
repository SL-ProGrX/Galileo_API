namespace Galileo.Models.ProGrX_Contabilidad
{
    public class PresVistaPresupuestoAlertasBuscar
    {
        public int? cod_conta { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string centro_costo { get; set; } = string.Empty;
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string tipo_vista { get; set; } = string.Empty;
        public bool ctaMov { get; set; }= false;
        public bool traReal { get; set; } = false;
        public string tipo_alerta { get; set; } = string.Empty;
        public string justificacion { get; set; } = "T";
    }

    public class PresVistaPresupuestoAlertasData
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal real_mes { get; set; } = 0;
        public decimal mensual { get; set; } = 0;
        public decimal diferencia_mes { get; set; } = 0;
        public decimal real_acumulado { get; set; } = 0;
        public decimal acumulado { get; set; } = 0;
        public decimal diferencia_acumulada { get; set; } = 0;
        public decimal pres_total { get; set; } = 0;
        public decimal diferencia_total { get; set; } = 0;
        public decimal ejecutado_mes { get; set; } = 0;
        public decimal ejecutado_acumulado { get; set; } = 0;
        public decimal ejecutado_total { get; set; } = 0;
        public bool acepta_movimientos { get; set; } = false;
        public DateTime? periodo { get; set; }
        public decimal pre_mensual_inicial { get; set; } = 0;
        public decimal presupuesto { get; set; } = 0;
        public string alerta_tipo { get; set; } = string.Empty;
        public string alerta_descripcion { get; set; } = string.Empty;

        public bool justificada { get; set; } = false;
        public string justificacion_actual { get; set; } = string.Empty;
        public DateTime? justificacion_fecha { get; set; }
        public string justificacion_usuario { get; set; } = string.Empty;

        public int? id_exclusion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class PresAlertaTipoJustificacionData
    {
        public string cod_tp_justificacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class PresAlertasDashboardFiltroRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string cod_modelo { get; set; } = string.Empty;
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string tipo_alerta { get; set; } = "T";
    }

    public class PresAlertasDashboardResumenData
    {
        public int total_excluidas { get; set; } = 0;
        public int total_justificadas { get; set; } = 0;
        public int total_pendientes { get; set; } = 0;
        public decimal porcentaje_justificado { get; set; } = 0;
    }

    public class PresAlertasDashboardUnidadData
    {
        public string cod_unidad { get; set; } = string.Empty;
        public int excluidas { get; set; } = 0;
        public int justificadas { get; set; } = 0;
        public int pendientes { get; set; } = 0;
    }

    public class PresAlertasDashboardJustificacionData
    {
        public string tipo_alerta { get; set; } = string.Empty;
        public string alerta_descripcion { get; set; } = string.Empty;
        public string tipo_justificacion { get; set; } = string.Empty;
        public int cantidad { get; set; } = 0;
    }

}
