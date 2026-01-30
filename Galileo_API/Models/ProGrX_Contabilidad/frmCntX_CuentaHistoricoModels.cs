namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXCuentaHistoricoData
    {
        public int anio { get; set; } = 0;
        public int mes { get; set; } = 0;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string cod_unidad { get; set; } = string.Empty;
        public string cod_centro_costo { get; set; } = string.Empty;

        public decimal saldo_inicial { get; set; } = 0;
        public decimal debitos { get; set; } = 0;
        public decimal creditos { get; set; } = 0;

        public decimal neto_mes { get; set; } = 0;
        public decimal saldofinal { get; set; } = 0;
    }

}
