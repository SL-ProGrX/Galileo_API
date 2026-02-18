namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasAjustesOperacionData
    {
        public int operacion { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string cod_concepto { get; set; } = string.Empty;
        public string num_documento { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string oficinax { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
        public string contrato { get; set; } = string.Empty;
        public string pagador { get; set; } = string.Empty;
    }

    public class CxCCuentasAjustesCuotasData
    {
        public int linea { get; set; } = 0;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal? int_cor { get; set; }
        public decimal? int_mor { get; set; }
        public decimal? principal { get; set; }
        public decimal? cargos { get; set; }
        public int? dias_mora { get; set; }
    }

    public class CxCCuentasAjustesCargosData
    {
        public int id_cargo { get; set; } = 0;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public decimal? saldo { get; set; }
        public string notas { get; set; } = string.Empty;
    }

    public class CxCCuentasAjustesFechaRequest
    {
        public int operacion { get; set; } = 0;
        public DateTime fecha_documento { get; set; } = DateTime.Now;
    }
}
