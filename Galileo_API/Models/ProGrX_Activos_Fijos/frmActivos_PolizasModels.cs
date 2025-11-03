namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class frmActivos_PolizasModels
    {
        public class ActivosPolizasLista
        {
            public int total { get; set; }
            public List<ActivosPolizasData> lista { get; set; } = new();
        }

        public class ActivosPolizasData
        {
            public string cod_poliza { get; set; } = string.Empty;
            public string tipo_poliza { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public string? observacion { get; set; }
            public string fecha_inicio { get; set; } = string.Empty;
            public string fecha_vence { get; set; } = string.Empty;
            public decimal monto { get; set; }
            public string? num_poliza { get; set; }
            public string? documento { get; set; }
            public string estado { get; set; } = string.Empty;
            public string? tipo_poliza_desc { get; set; }
            public string? registro_usuario { get; set; }
            public string? registro_fecha { get; set; }
            public string? modifica_usuario { get; set; }
            public string? modifica_fecha { get; set; }

            public bool isNew { get; set; } = false; 
        }

        public class ActivosPolizasFiltros
        {
            public int? pagina { get; set; }
            public int? paginacion { get; set; }
            public string? filtro { get; set; } 
        }

        public class ActivosPolizasTipo
        {
            public string tipo_poliza { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
        }
        public class ActivosPolizasAsignacionItem
        {
            public string num_placa { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
            public string estado { get; set; } = string.Empty; 
            public bool asignado { get; set; } 
        }

        public class ActivosPolizasAsignacionFiltros
        {
            public string cod_poliza { get; set; } = string.Empty;
            public string? tipo_activo { get; set; }
            public string? placa { get; set; }
            public string? nombre { get; set; }
            public int? pagina { get; set; }
            public int? paginacion { get; set; }
        }
        public class ActivosPolizasAsignacionBulk
        {
            public string cod_poliza { get; set; } = string.Empty;
            public List<string> placas { get; set; } = new();
        }
    }
}
