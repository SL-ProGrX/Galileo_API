namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class FrmActivosTiposActivosModels
    {
        public class ActivosTiposActivosLista
        {
            public int total { get; set; }
            public List<ActivosTiposActivosData> lista { get; set; } = new List<ActivosTiposActivosData>();
        }

        public class ActivosTiposActivosData
        {
            public string tipo_activo { get; set; } = string.Empty;
            public string asiento_genera { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public string met_depreciacion { get; set; } = string.Empty;
            public string cod_cuenta_actvo { get; set; } = string.Empty;
            public string cod_cuenta_gastos { get; set; } = string.Empty;
            public string cod_cuenta_depacum { get; set; } = string.Empty;
            public string cod_cuenta_transitoria { get; set; } = string.Empty;
            public string vida_util { get; set; } = string.Empty;
            public string tipo_vida_util { get; set; } = string.Empty;

            public string registro_usuario { get; set; } = string.Empty;
            public string registro_fecha { get; set; } = string.Empty;
            public string modifica_usuario { get; set; } = string.Empty;
            public string modifica_fecha { get; set; } = string.Empty;

            public bool isNew { get; set; } = false;

            public string tipo_asiento_desc { get; set; } = string.Empty;

            public string cod_cuenta_activo_mask { get; set; } = string.Empty;
            public string cod_cuenta_activo_desc { get; set; } = string.Empty;

            public string cod_cuenta_gastos_mask { get; set; } = string.Empty;
            public string cod_cuenta_gastos_desc { get; set; } = string.Empty;

            public string cod_cuenta_depacum_mask { get; set; } = string.Empty;
            public string cod_cuenta_depacum_desc { get; set; } = string.Empty;

            public string cod_cuenta_transitoria_mask { get; set; } = string.Empty;
            public string cod_cuenta_transitoria_desc { get; set; } = string.Empty;
        }
        
        public class ActivosTiposActivosFiltros
        {
            public int? pagina { get; set; }
            public int? paginacion { get; set; }
            public string? filtro { get; set; }
        }
    }
}