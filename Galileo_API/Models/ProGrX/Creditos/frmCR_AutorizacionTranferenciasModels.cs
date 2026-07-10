namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrAutorizacionTranferenciasTag
    {
        public string Llave { get; set; } = string.Empty;
        public string Describe { get; set; } = string.Empty;
    }

    public class CrAutorizacionTranferenciasSolicitud
    {
        public int IdSolicitud { get; set; }
        public DateTime? FechaForp { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal? MontoSol { get; set; }
        public decimal? Cuota { get; set; }
        public int? Plazo { get; set; }
        public decimal? Interes { get; set; }
        public string EstadoSolDescripcion { get; set; } = string.Empty;
        public DateTime? FechaSol { get; set; }
    }
}
