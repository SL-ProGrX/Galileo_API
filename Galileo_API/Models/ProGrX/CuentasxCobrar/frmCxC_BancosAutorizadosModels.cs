namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxcBancoAutorizadoInsertParams
    {
        public string Usuario { get; set; } = string.Empty;
    }

    public class CxcBancoAutorizadoResult
    {
        public int Id_Banco { get; set; }
        public string? Descripcion { get; set; }
        public int Cheques { get; set; }
        public int Transferencias { get; set; }
    }

    public class CxcBancoAutorizadoUpdateChequesParams
    {
        public int Id_Banco { get; set; }
        public int Cheques { get; set; }
    }

    public class CxcBancoAutorizadoUpdateTransferenciasParams
    {
        public int Id_Banco { get; set; }
        public int Transferencias { get; set; }
    }
}
