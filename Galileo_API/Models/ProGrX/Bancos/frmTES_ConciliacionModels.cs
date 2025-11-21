namespace Galileo.Models.ProGrX.Bancos
{
    public class TesConciliacionCuentaData
    {
        public int id_banco { get; set; }
        public string? cta { get; set; }
        public string? desc_corta { get; set; }
        public string? descripcion { get; set; }
        public int idX { get; set; }
        public string? itmX { get; set; }
    }

    public class TesConciliacionHistorico
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public string? periodo_desc { get; set; }
        public string? estado { get; set; }
        public decimal libros_saldo_concilia { get; set; }
        public decimal cta_saldo_concilia { get; set; }
        public decimal diferencia { get; set; }
    }

    public class TesConciliaPeriodo
    {
        public int id_banco { get; set; }
        public int anio { get; set; }
        public int mes { get; set; }
        public string? estado { get; set; }
        public decimal libros_saldo_inicial { get; set; }
        public decimal libros_saldo { get; set; }
        public decimal libros_nc { get; set; }
        public decimal libros_nd { get; set; }
        public decimal libros_saldo_concilia { get; set; }
        public decimal cta_saldo_inicial { get; set; }
        public decimal cta_saldo { get; set; }
        public decimal cta_saldo_concilia { get; set; }
        public decimal depositos_transito { get; set; }
        public decimal cheques_no_cobrados { get; set; }
        public string? notas { get; set; }
        public Nullable<DateTime> registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public Nullable<DateTime> cierra_fecha { get; set; }
        public string? cierra_usuario { get; set; }
        public Nullable<DateTime> actualiza_fecha { get; set; }
        public string? actualiza_usuario { get; set; }
        public int cta_saldo_upd_ind { get; set; }
        public Nullable<DateTime> cta_saldo_upd_fecha { get; set; }
        public string? cta_saldo_upd_usuario { get; set; }
        public Nullable<DateTime> periodo_inicio { get; set; }
        public Nullable<DateTime> periodo_corte { get; set; }
    }

    public class TesConciliaResultados
    {
        public int id { get; set; }
        public Nullable<DateTime> fecha { get; set; }
        public string? tipo { get; set; }
        public string? documento { get; set; }
        public decimal importe { get; set; }
        public string? descripcion { get; set; }
        public string? estado { get; set; }
        public int concilia_id_ref { get; set; }
        public string? tipo_desc { get; set; }
    }

    public class TesConciliaResultadoFiltros
    {
        public int id_banco { get; set; }
        public int ahno { get; set; }
        public int mes { get; set; }
        public string? ubicacion { get; set; }
        public string? tipoDoc { get; set; }
        public string? estadoCasos { get; set; }
    }

    public class TesConciliaAsigna
    {
        public int id { get; set; }
        public Nullable<DateTime> fecha { get; set; }
        public string? tipo { get; set; }
        public string? documento { get; set; }
        public decimal importe { get; set; }
        public string? descripcion { get; set; }
        public string? estado { get; set; }
        public int concilia_id_ref { get; set; }
        public string? tipo_desc { get; set; }
    }

    public class TesConciliaAsignaFiltros
    {
        public int banco { get; set; }
        public int ahno { get; set; }
        public int mes { get; set; }
        public string? ubicacion { get; set; }
        public string? tipoMov { get; set; }
        public decimal movImporte { get; set; }
        public string? movFiltro { get; set; }
        public bool chkConciliaPendientes { get; set; }
        public bool chkConciliaFiltroMontos { get; set; }
        public bool chkConciliaFiltroFechas { get; set; }
        public Nullable<DateTime> dtpConciliaInicio { get; set; }
        public Nullable<DateTime> dtpConciliaCorte { get; set; }
    }

    public class TesConciliaFiltros
    {
        public string periodoEstado { get; set; } = string.Empty;
        public int banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public decimal saldo { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string? notas { get; set; } = null;
        public decimal? saldoActual { get; set; }
    }

    public class TesConciliacioExcelDto
    {
        public Nullable<DateTime> fecha { get; set; }
        public string? tipo { get; set; }
        public string? documento { get; set; }
        public decimal? importe { get; set; }
        public string? descripcion { get; set; }
        public decimal? saldo { get; set; }
    }

    public class TesConciliacionResultosFiltro
    {
        public int banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? periodoEstado { get; set; }
        public decimal? ar_monto { get; set; }
        public string? ar_cuenta { get; set; }
        public string? usuario { get; set; }
        public bool chkAutoReg { get; set; } = false;
        public string? ubicacion { get; set; } = null;
    }

    public class TesConciliacionFiltro
    {
        public int banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? periodoEstado { get; set; }
        public string? mov_id { get; set; } = null;
        public string? movFiltro { get; set; } = null;
        public string? usuario { get; set; } = null;
        public string? ubicacion { get; set; } = null;
        public int caso { get; set; } = 0;
    }

    public class TesConciliacionDetallesData
    {
        public int? id { get; set; }
        public Nullable<DateTime> fecha { get; set; }
        public string? tipo_desc { get; set; }
        public string? documento { get; set; }
        public decimal? importe { get; set; }
        public string? descripcion { get; set; }
        public int? concilia_id_ref { get; set; }
        public string? concilia_desc { get; set; }
    }

    public class TesConciliacionDetallesLoteData
    {
        public int? id { get; set; }
        public Nullable<DateTime> fecha { get; set; }
        public string? tipo_desc { get; set; }
        public string? documento { get; set; }
        public decimal? importe { get; set; }
        public string? descripcion { get; set; }
        public int? concilia_id_ref { get; set; }
        public string? concilia_desc { get; set; }
    }
}