namespace Galileo.Models.ProGrX.Bancos
{
    public class TransferenciasData
    {
        public decimal monto { get; set; } = 0;
        public int nSolicitud { get; set; } = 0;
        public string? modulo { get; set; }
        public string? subModulo { get; set; }
        public string? detalle1 { get; set; }
        public string? referencia { get; set; }
        public int id_Banco { get; set; } = 0;
        public string? tipo { get; set; }
        public string? codigo { get; set; }
    }

    public class TesTransferenciasInfo
    {
        public int id_Banco { get; set; } = 0;
        public string? tipoDoc { get; set; }
        public string? plan { get; set; }
        public string? usuario { get; set; }
        public string? bancoConsec { get; set; }
        public string? gstrQuery { get; set; }
        public TesTransferenciasParametros? parametros { get; set; }
    }

    public class TesTransferenciasParametros
    {
        public int banco { get; set; } = 0;
        public string tipoDoc { get; set; } = string.Empty;
        public int minimo { get; set; } = 0;
        public int maximo { get; set; } = 0;
        public DateTime? fechaInicio { get; set; } = null;
        public DateTime? fechaCorte { get; set; } = null;
    }
}