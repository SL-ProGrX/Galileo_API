namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public class CntXInvPeriodicoDto
    {
        public int Anio { get; set; }
        public int Mes { get; set; }
        public string Cod_Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public decimal Saldo_Final { get; set; }
    }

    public class CntXInvPeriodicoSaveParams
    {
        public required int CodContabilidad { get; set; }
        public required int Anio { get; set; }
        public required int Mes { get; set; }
        public string CodCuenta { get; set; } = string.Empty;
        public required decimal SaldoFinal { get; set; }
        public string RegistroUsuario { get; set; } = string.Empty;
    }

    public class CntXInvPeriodicoDeleteParams
    {
        public required int CodContabilidad { get; set; }
        public required int Anio { get; set; }
        public required int Mes { get; set; }
        public string CodCuenta { get; set; } = string.Empty;
        public string RegistroUsuario { get; set; } = string.Empty;
    }
}
