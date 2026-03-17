namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXCuentaForaneaData
    {
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
    }

    public class CntXCuentaMovSaldoData
    {
        public decimal saldo_inicial { get; set; } = 0;
        public decimal df_saldo_inicial { get; set; } = 0;
    }

    public class CntXCuentaForaneaInicializaRequest
    {
        public int cod_contabilidad { get; set; } = 0;
        public string cod_cuenta { get; set; } = string.Empty;
        public decimal saldo_inicial { get; set; } = 0;
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
    }
}
