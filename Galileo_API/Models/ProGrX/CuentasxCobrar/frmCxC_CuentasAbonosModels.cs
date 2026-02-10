namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasAbonosData
    {
        public int operacion { get; set; }
        public decimal saldo { get; set; }
        public string proceso { get; set; } = string.Empty;
        public decimal tasa_corriente { get; set; }
        public decimal interesc { get; set; }
        public decimal amortiza { get; set; }
        public int fecha_ultmov { get; set; }
        public decimal cuota { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public int meses { get; set; }
        public string nombre { get; set; } = string.Empty;
        public DateTime activa_fecha { get; set; }
        public string autoriza_usuario { get; set; } = string.Empty;
        public string conceptodesc { get; set; } = string.Empty;
        public string oficinadesc { get; set; } = string.Empty;
        public DateTime fechaserver { get; set; }
        public bool caja_valida_concepto { get; set; }
        public int facturas { get; set; }
    }

    public class CxCCuotasActivasData
    {
        public int linea { get; set; }
        public DateTime fecha_corte { get; set; }
        public decimal monto { get; set; }
        public string estado_desc { get; set; } = string.Empty;
        public decimal int_cor { get; set; }
        public decimal int_mor { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public int dias { get; set; }
        public int dias_mora { get; set; }
    }

    public class CxCOperacionesActivasData
    {
        public int operacion { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }
}
