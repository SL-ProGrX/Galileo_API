namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrMonitorCancelacionRequest
    {
        public required DateTime Fecha_Inicio { get; set; }
        public required DateTime Fecha_Corte { get; set; }
        public decimal Porcentaje { get; set; } = 0;
    }

    public class CrMonitorCancelacionModel
    {
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public long Id_Solicitud { get; set; } = 0;
        public string Codigo { get; set; } = string.Empty;
        public decimal Saldo { get; set; } = 0;
        public int Plazo { get; set; } = 0;
        public decimal Tasa_Original { get; set; } = 0;
        public decimal Tasa_Actual { get; set; } = 0;
        public decimal Cuota_Inicial { get; set; } = 0;
        public decimal Cuota_Final { get; set; } = 0;
        public DateTime? Fecha_Termina { get; set; }
        public DateTime? Fecha_Termino_Inicial { get; set; }
        public int Plazo_Termina { get; set; } = 0;
        public int Plazo_Adicional { get; set; } = 0;
        public string Formaliza { get; set; } = string.Empty;
        public string NDocumento { get; set; } = string.Empty;
    }
}
