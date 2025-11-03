namespace PgxAPI.Models.TES
{
    public class TES_Cuenta_BancariaDTO
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;
    }

    public class TES_Depositos_TramiteDTO
    {
        public int id_banco { get; set; }
        public string documento { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool? existe { get; set; }
        public string? inconsistencia { get; set; }
        public bool? requiere_identificacion { get; set; }
    }

    public class TES_Depositos_Tramite_InconsistenciasDTO
    {
        public string documento { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime fecha { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string inconsistencia { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public string banco { get; set; } = string.Empty;
    }

    public class Filtros_Registro
    {
        public int banco { get; set; }
        public string numDoc { get; set; } = string.Empty;
        public string cboFechas { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public int cboFiltro { get; set; }
        public string filtro { get; set; } = string.Empty;
        public int? pagina { get; set; } = 1;
        public int? paginacion { get; set; } = 30;
    }

    public class TES_Depositos_Tramite_BancoDTO
    {
        public int id_banco { get; set; }
        public int dp_tramite_id { get; set; }
        public string documento { get; set; } = string.Empty;
        public DateTime fecha { get; set; } 
        public decimal monto { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? cliente_id { get; set; } 
        public string? cliente_nombre { get; set; } = string.Empty;
        public DateTime? identifica_fecha { get; set; }
        public string? identifica_usuario { get; set; } = string.Empty;
        public DateTime? tes_aplicado_fecha { get; set; }
        public string? tes_aplicado_usuario { get; set; } = string.Empty;
        public int? nsolicitud { get; set; } 
    }

    public class Filtros_Inconsistencias
    {
        public int banco { get; set; }
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string filtro { get; set; } = string.Empty;
        public int? pagina { get; set; } = 1;
        public int? paginacion { get; set; } = 30;
    }
}
