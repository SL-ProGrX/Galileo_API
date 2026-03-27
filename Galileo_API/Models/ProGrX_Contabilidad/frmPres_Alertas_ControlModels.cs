namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class PresAlertasControlExclusionData
    {
        public int id_exclusion { get; set; } = 0;
        public int cod_empresa { get; set; } = 0;
        public int cod_contabilidad { get; set; } = 0;
        public string cod_modelo { get; set; } = string.Empty;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string tipo_alerta { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal? real_mes { get; set; }
        public decimal? mensual { get; set; }
        public decimal? diferencia_mes { get; set; }
        public decimal? real_acumulado { get; set; }
        public decimal? acumulado { get; set; }
        public decimal? diferencia_acumulada { get; set; }
        public decimal? pres_total { get; set; }
        public decimal? diferencia_total { get; set; }
        public decimal? ejecutado_mes { get; set; }
        public decimal? ejecutado_acumulado { get; set; }
        public decimal? ejecutado_total { get; set; }
        public bool? acepta_movimientos { get; set; }
        public string alerta_descripcion { get; set; } = string.Empty;
        public bool? justificada { get; set; }
        public string justificacion_actual { get; set; } = string.Empty;
        public DateTime? justificacion_fecha { get; set; }
        public string justificacion_usuario { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
    }

    public class PresAlertasControlExclusionGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public List<PresAlertasControlExclusionData> lineas { get; set; } = new();
    }

    public class PresAlertasControlExclusionFiltroRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string cod_modelo { get; set; } = string.Empty;
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string tipo_alerta { get; set; } = string.Empty;
    }

    public class PresAlertasControlExclusionEliminarRequest
    {
        public int id_exclusion { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }

    public class PresAlertasJustificaPeriodoRequest
    {
        public string cod_modelo { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string usuario { get; set; } = string.Empty;
        public bool bloqueo_visualizacion { get; set; }
    }

    public class PresAlertasJustificaPeriodoData
    {
        public int id_periodo { get; set; } = 0;
        public string cod_modelo { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; } = 0;
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool bloqueo_visualizacion { get; set; } = false;
        public bool permitido_justificar { get; set; } = false;
        public string mensaje { get; set; } = string.Empty;
    }

    public class PresAlertasControlPeriodoConfigRequest
    {
        public string cod_modelo { get; set; } = string.Empty;
        public int cod_contabilidad { get; set; }
        public int anio { get; set; } = 1900;
        public int mes { get; set; } = 1;
        public string usuario { get; set; } = string.Empty;
        public bool bloqueo_visualizacion { get; set; }
    }

    public class PresAlertasControlPeriodoEstadoData
    {
        public bool periodo_cerrado { get; set; } = false;
        public bool periodo_registrado { get; set; } = false;
        public bool puede_guardar_seleccion { get; set; } = false;
        public bool requiere_configuracion { get; set; } = false;
        public bool fuera_de_plazo { get; set; } = false;
        public string mensaje { get; set; } = string.Empty;
        public DateTime? cierre_fecha { get; set; }
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public bool bloqueo_visualizacion { get; set; } = false;
    }
}
