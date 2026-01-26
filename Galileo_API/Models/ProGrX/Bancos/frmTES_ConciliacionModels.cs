namespace Galileo.Models.ProGrX.Bancos
{
    // =========================
    // Datos básicos
    // =========================

    public class TesConciliacionCuentaData
    {
        public int id_banco { get; set; } = 0;
        public string? cta { get; set; }
        public string? desc_corta { get; set; }
        public string? descripcion { get; set; }
        public int idX { get; set; } = 0;
        public string? itmX { get; set; }
    }

    public class TesConciliacionHistorico
    {
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? periodo_desc { get; set; }
        public string? estado { get; set; }
        public decimal libros_saldo_concilia { get; set; } = 0;
        public decimal cta_saldo_concilia { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
    }

    // =========================
    // Periodo conciliación
    // =========================

    public class TesConciliaPeriodo
    {
        public int id_banco { get; set; } = 0;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? estado { get; set; }
        public decimal libros_saldo_inicial { get; set; } = 0;
        public decimal libros_saldo { get; set; } = 0;
        public decimal libros_nc { get; set; } = 0;
        public decimal libros_nd { get; set; } = 0;
        public decimal libros_saldo_concilia { get; set; } = 0;
        public decimal cta_saldo_inicial { get; set; } = 0;
        public decimal cta_saldo { get; set; } = 0;
        public decimal cta_saldo_concilia { get; set; } = 0;
        public decimal depositos_transito { get; set; } = 0;
        public decimal cheques_no_cobrados { get; set; } = 0;
        public string? notas { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? cierra_fecha { get; set; }
        public string? cierra_usuario { get; set; }
        public DateTime? actualiza_fecha { get; set; }
        public string? actualiza_usuario { get; set; }
        public int cta_saldo_upd_ind { get; set; } = 0;
        public DateTime? cta_saldo_upd_fecha { get; set; }
        public string? cta_saldo_upd_usuario { get; set; }
        public DateTime? periodo_inicio { get; set; }
        public DateTime? periodo_corte { get; set; }
    }

    // =========================
    // Movimientos conciliación
    // =========================

    public class TesConciliaMovimientoBase
    {
        public int id { get; set; } = 0;
        public DateTime? fecha { get; set; }
        public string? tipo { get; set; }
        public string? documento { get; set; }
        public decimal importe { get; set; } = 0;
        public string? descripcion { get; set; }
        public string? estado { get; set; }
        public int concilia_id_ref { get; set; } = 0;
        public string? tipo_desc { get; set; }
    }

    public class TesConciliaResultados : TesConciliaMovimientoBase
    {
        public bool mov { get; set; } = false;
    }

    public class TesConciliaAsigna : TesConciliaMovimientoBase
    {
        public bool mov { get; set; } = false;
    }

    // =========================
    // Filtros
    // =========================

    public class TesConciliaResultadoFiltros
    {
        public int id_banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? ubicacion { get; set; }
        public string? tipoDoc { get; set; }
        public string? estadoCasos { get; set; }
    }

    public class TesConciliaAsignaFiltros
    {
        public int banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? ubicacion { get; set; }
        public string? tipoMov { get; set; }
        public decimal movImporte { get; set; } = 0;
        public string? movFiltro { get; set; }
        public bool chkConciliaPendientes { get; set; } = false;
        public bool chkConciliaFiltroMontos { get; set; } = false;
        public bool chkConciliaFiltroFechas { get; set; } = false;
        public DateTime? dtpConciliaInicio { get; set; }
        public DateTime? dtpConciliaCorte { get; set; }
    }

    public class TesConciliaFiltros
    {
        public string periodoEstado { get; set; } = string.Empty;
        public int banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string? notas { get; set; }
        public decimal? saldoActual { get; set; }
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
        public string? ubicacion { get; set; }
    }

    public class TesConciliacionFiltro
    {
        public int banco { get; set; } = 0;
        public int ahno { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string? periodoEstado { get; set; }
        public string? mov_id { get; set; }
        public string? movFiltro { get; set; }
        public string? usuario { get; set; }
        public string? ubicacion { get; set; }
        public int caso { get; set; } = 0;
    }

    // =========================
    // Excel / Detalles
    // =========================

    public class TesConciliacioExcelDto
    {
        public DateTime? fecha { get; set; }
        public string? tipo { get; set; }
        public string? documento { get; set; }
        public decimal? importe { get; set; }
        public string? descripcion { get; set; }
        public decimal? saldo { get; set; }
    }

    public class TesConciliacionDetallesBase
    {
        public int? id { get; set; }
        public DateTime? fecha { get; set; }
        public string? tipo_desc { get; set; }
        public string? documento { get; set; }
        public decimal? importe { get; set; }
        public string? descripcion { get; set; }
        public int? concilia_id_ref { get; set; }
        public string? concilia_desc { get; set; }
    }

    public class TesConciliacionDetallesData : TesConciliacionDetallesBase
    {
        public bool detalle { get; set; } = false;
    }

    public class TesConciliacionDetallesLoteData : TesConciliacionDetallesBase
    {
        public bool detalle { get; set; } = false;
    }
}
