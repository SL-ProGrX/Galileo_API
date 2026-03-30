namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdOperacionData
    {
        public int noperacion { get; set; } = 0;
        public DateTime? activa_fecha { get; set; }
        public int dias_pendientes { get; set; } = 0;
        public decimal monto { get; set; } = 0;
        public string actividad { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string desembolso { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? fecha_emision { get; set; }
    }
}
