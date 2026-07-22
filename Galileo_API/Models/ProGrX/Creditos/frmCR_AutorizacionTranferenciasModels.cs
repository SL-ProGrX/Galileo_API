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
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public decimal? MontoSol { get; set; }
        public decimal? Cuota { get; set; }
        public int? Plazo { get; set; }
        public decimal? Interes { get; set; }
        public string EstadoSolDescripcion { get; set; } = string.Empty;
        public DateTime? FechaSol { get; set; }
    }

    public class CrAutorizacionTranferenciasOperacionTagRegistrarRequest
    {
        public int? Operacion { get; set; }
        public string Linea { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Asignado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
    }
}
