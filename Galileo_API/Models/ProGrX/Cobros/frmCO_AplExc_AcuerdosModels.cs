namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplExcAcuerdosData
    {
        public int id_acuerdo { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public DateTime? firma_boleta { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public string observaciones { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string giro_excedentes { get; set; } = string.Empty;
        public string usuario_registra { get; set; } = string.Empty;
        public DateTime? fecha_registra { get; set; }
        public string nombre { get; set; } = string.Empty;
        public DateTime? cedula_vence { get; set; }
    }

    public class CoAplExcAcuerdosGuardarResponse
    {
        public int pass { get; set; } = 0;
        public int acuerdoId { get; set; } = 0;
        public string movimiento { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }
}
