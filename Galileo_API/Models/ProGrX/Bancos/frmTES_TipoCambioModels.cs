namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesTipoCambioConsulta
    {
        public int CodEmpresa { get; set; } = 0;
        public int contabilidad { get; set; } = 1;
        public float tc_actual { get; set; } = 0;
        public float monto_actual { get; set; } = 0;
        public string divisa { get; set; } = "DOL";
        public float tcPermitido { get; set; } = 0;
        public float tcVariacion { get; set; } = 0;

    }

    public class TesTipoCambioDivisasTipoCambio
    {
        public float tc_venta { get; set; } = 0;
        public float tc_Compra { get; set; } = 0;
        public float variacion { get; set; } = 0;
        public float tc_actual { get; set; } = 0;
    }

}
