namespace Galileo_API.Models.ProGrX.Cajas
{
    public class TesDepositoIdentificarDto
    {
        public long DepositoId { get; set; }
        public long BancoId { get; set; }
        public string Documento { get; set; } = "";
    }

    public class FrmCajasIdentificaSfDepositoDto
    {
        public int id_banco { get; set; } = 0;
        public string descripcion { get; set; } = "";
        public string cta { get; set; } = "";
        public string itmX { get; set; } = "";
    }
    public class FrmCajasIdentificaSfTramitsRsdto
    {
        public long dp_tramite_id { get; set; }
        public string nsolicitud { get; set; } = string.Empty;
        public int id_banco { get; set; }
        public string bancodesc { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public decimal monto { get; set; }
        public string? descripcion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }


}
