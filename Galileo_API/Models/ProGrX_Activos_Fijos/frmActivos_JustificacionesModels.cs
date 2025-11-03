namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class frmActivos_JustificacionesModels
    {
        public class ActivosJustificacionesLista
        {
            public int total { get; set; }
            public List<ActivosJustificacionesData> lista { get; set; } = new List<ActivosJustificacionesData>();
        }

        public class ActivosJustificacionesData
        {
            public string cod_justificacion { get; set; } = string.Empty;
            public string tipo { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public string tipo_asiento { get; set; } = string.Empty;
            public string cod_cuenta_01 { get; set; } = string.Empty;
            public string cod_cuenta_02 { get; set; } = string.Empty;
            public string cod_cuenta_03 { get; set; } = string.Empty;
            public string cod_cuenta_04 { get; set; } = string.Empty;
            public string estado { get; set; } = string.Empty;

            public string registro_usuario { get; set; } = string.Empty;
            public string registro_fecha { get; set; } = string.Empty;
            public string modifica_usuario { get; set; } = string.Empty;
            public string modifica_fecha { get; set; } = string.Empty;

            public bool isNew { get; set; } = false;

            public string tipo_asiento_desc { get; set; } = string.Empty;

            public string cod_cuenta_01_mask { get; set; } = string.Empty;
            public string cod_cuenta_01_desc { get; set; } = string.Empty;

            public string cod_cuenta_02_mask { get; set; } = string.Empty;
            public string cod_cuenta_02_desc { get; set; } = string.Empty;

            public string cod_cuenta_03_mask { get; set; } = string.Empty;
            public string cod_cuenta_03_desc { get; set; } = string.Empty;

            public string cod_cuenta_04_mask { get; set; } = string.Empty;
            public string cod_cuenta_04_desc { get; set; } = string.Empty;
        }

        public class ActivosJustificacionesFiltros
        {
            public int? pagina { get; set; }
            public int? paginacion { get; set; }
            public string? filtro { get; set; }
        }
    }
}
