namespace PgxAPI.Models.TES
{
    public class TES_TokenDTO
    {
        public required string idtoken { get; set; }
        public required string estado { get; set; }
        public DateTime registrofecha { get; set; }
        public required string registrousuario { get; set; }
        public int pendiente { get; set; }
        public decimal monto { get; set; }
    }

    public class TES_TokenSolicitudesData
    {
        public int nsolicitud { get; set; }
        public string codigo { get; set; }
        public string beneficiario { get; set; }
        public DateTime fecha_solicitud { get; set; }
        public string remesa { get; set; }
        public int remesa_id { get; set; }
    }

}