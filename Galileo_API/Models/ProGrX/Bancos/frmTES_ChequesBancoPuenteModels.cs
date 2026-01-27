namespace Galileo.Models.ProGrX.Bancos
{
    public class ChequesBancoPuenteData
    {
        public int control { get; set; } = 0;
        public int nsolicitud { get; set; } = 0;
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public float monto { get; set; } = 0;
        public Nullable<DateTime> fecha_solicitud { get; set; }
    }
}