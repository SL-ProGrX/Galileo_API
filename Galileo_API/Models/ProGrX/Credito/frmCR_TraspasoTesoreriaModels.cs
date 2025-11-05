namespace PgxAPI.Models.ProGrX.Credito
{
    public class TraspasoModel
    {
        public int id_solicitud { get; set; }
        public string? codigo { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal montoapr { get; set; }
        public decimal monto_girado { get; set; }
        public decimal desembolsos_numero { get; set; }
        public decimal desembolsos { get; set; }
    }
}