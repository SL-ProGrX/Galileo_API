namespace PgxAPI.Models.ProGrX.Bancos
{
    public class ChequesBancoPuenteData
    {
        public int control { get; set; }
        public int nsolicitud { get; set; }
        public string? codigo { get; set; }
        public string? beneficiario { get; set; }
        public float monto { get; set; }
        public Nullable<DateTime> fecha_solicitud { get; set; }
    }
}