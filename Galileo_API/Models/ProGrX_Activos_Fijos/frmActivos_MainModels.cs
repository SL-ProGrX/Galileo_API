namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class MainGeneralData
    {
        public string num_placa { get; set; } = string.Empty;
        public string placa_alterna { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tipo_activo { get; set; } = string.Empty;
        public string met_depreciacion { get; set; } = string.Empty;
        public int vida_util { get; set; }
        public decimal ud_produccion { get; set; }
        public decimal ud_anio { get; set; }
        public string estado { get; set; } = string.Empty;
        public string vida_util_en { get; set; } = string.Empty;
        public decimal valor_historico { get; set; }
        public decimal valor_desecho { get; set; }
        public DateTime fecha_adquisicion { get; set; }
        public DateTime fecha_instalacion { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cod_departamento { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string cod_seccion { get; set; } = string.Empty;
        public string identificacion { get; set; } = string.Empty;
        public string compra_documento { get; set; } = string.Empty;
        public string cod_proveedor { get; set; } = string.Empty;
        public string num_serie { get; set; } = string.Empty;
        public string modelo { get; set; } = string.Empty;
        public string marca { get; set; } = string.Empty;
        public string otras_senas { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime depreciacion_periodo { get; set; }
        public decimal depreciacion_acum { get; set; }
        public decimal depreciacion_mes { get; set; }
        public string departamento_desc { get; set; } = string.Empty;
        public string seccion_desc { get; set; } = string.Empty;
        public string responsable_desc { get; set; } = string.Empty;
        public string proveedor { get; set; } = string.Empty;
        public string tipo_activo_desc { get; set; } = string.Empty;
        public string localiza_id { get; set; } = string.Empty;
        public string localiza_desc { get; set; } = string.Empty;
    }

    public class MainHistoricoData
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public string estado_periodo { get; set; } = string.Empty;
        public decimal valor_libros_consolidado { get; set; }
        public decimal depreciacion_ac_consolidado { get; set; }
        public decimal depreciacion_mes_consolidado { get; set; }
        public string responsable_nombre { get; set; } = string.Empty;
        public string responsable_departamento { get; set; } = string.Empty;
        public string responsable_seccion { get; set; } = string.Empty;
        public string tipo_activo_desc { get; set; } = string.Empty;
        public string met_depreciacion { get; set; } = string.Empty;
        public int vida_util { get; set; }
    }

    public class MainDetalleResponsablesData
    {
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string registro_fecha { get; set; } = string.Empty;
    }

    public class MainModificacionesData
    {
        public int id_addret { get; set; }
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string justifica { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string tipomov { get; set; } = string.Empty;
    }

    public class MainComposicionData
    {
        public string num_placa { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public DateTime depreciacion_periodo { get; set; }
        public decimal depreciacion_acum { get; set; }
        public decimal depreciacion_mes { get; set; }
        public DateTime fecha { get; set; }
        public decimal libros { get; set; }
    }

    public class MainPolizasData
    {
        public string desctipo { get; set; } = string.Empty;
        public string num_poliza { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public DateTime fecha_Inicio { get; set; }
        public DateTime fecha_vence { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cod_poliza { get; set; } = string.Empty;
    }
    
    public class MainActivosTiposData
    {
        public string met_depreciacion { get; set; } = string.Empty;
        public decimal vida_util { get; set; }
        public string tipo_vida_util { get; set; } = string.Empty;
    }
}