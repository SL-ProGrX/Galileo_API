namespace PgxAPI.Models.AF
{
    public class BeneCategoria
    {
        public string cod_categoria { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool i_apremiante { get; set; }
        public bool i_reconocimientos { get; set; }
        public bool i_crece { get; set; }
        public bool i_fena { get; set; }
        public bool i_sepelio { get; set; }
        public bool i_desastres { get; set; }
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class BEeneCategoriaDataLista
    {
        public int Total { get; set; }
        public List<BeneCategoria> Lista { get; set; } = new List<BeneCategoria>();
    }

    public class BeneCategoriaPermisos
    {
        public string nombre { get; set; } = string.Empty;
        public bool i_cambiar_estado { get; set; } = false;
        public bool i_modifica_expediente { get; set; } = false;
        public bool i_traslado_tesoreria { get; set; } = false;
        public bool i_pago_programar { get; set; } = false;
        public bool i_pago_aprobar_m { get; set; } = false;
        public bool i_pago_realizar { get; set; } = false;
        public bool i_ingresar_solicitud { get; set; } = false;
        public bool i_periodo { get; set; } = false;
        public bool i_pago_consulta { get; set; } = false;
        public bool i_aprobar { get; set; } = false;
        public bool i_rechazar { get; set; } = false;
        public bool i_anular { get; set; } = false;
        public bool i_devolver_resolucion { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public int cod_rol { get; set; }
    }

    public class  BeneValidaLista
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class BeneCategoriaValidaLista
    {
        public string cod_categoria { get; set; } = string.Empty;
        public int cod_val { get; set; }
        public bool registro { get; set; } = false;
        public bool registro_justifica { get; set; } = false;
        public bool registro_info { get; set; } = false;
        public bool pago { get; set; } = false;
        public bool pago_justifica { get; set; } = false;
        public bool pago_info { get; set; } = false;
        public bool estado { get; set; } = false;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string? modifica_usuario { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
    }

    public class AfiBeneCalidaciones
    {
        public int cod_val { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string query_val { get; set; } = string.Empty;
        public string msj_val { get; set; } = string.Empty;
        public int resultado_val { get; set; } = 0;
        public bool registro_justifica { get; set; } = false;
        public bool pago_justifica { get; set; } = false;
    }
}