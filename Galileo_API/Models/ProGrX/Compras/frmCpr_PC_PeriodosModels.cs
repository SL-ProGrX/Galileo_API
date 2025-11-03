namespace PgxAPI.Models.CPR
{
    public class cprPeriodosPlanFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class cprPeriodosPlanLista
    {
        public int total { get; set; }
        public List<cprPlanPeriodosDTO> lista { get; set; } = new List<cprPlanPeriodosDTO>();
    }

    public class cprPlanPeriodosDTO
    {
        public int id_periodo { get; set; }
        public int cod_contabilidad { get; set; }
        public DateTime inicio { get; set; }
        public DateTime corte { get; set; }
        public string? estado { get; set; }
        public string? notas { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public Nullable<DateTime> actualiza_fecha { get; set; }
        public string? actualiza_usuario { get; set; }
    }

    public class cprModeloDatos
    {
        public int inicio_mes { get; set; }
        public int corte_mes { get; set; }
    }

    public class cprModeloDateDatos
    {
        public DateTime inicio_mes { get; set; }
        public DateTime corte_mes { get; set; }
    }

    public class cprModeloFiltro
    {
        public int codEmpresa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public int cod_Contabilidad { get; set; }
        public string cod_modelo { get; set; } = string.Empty;
    }
}
