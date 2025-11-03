namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TES_TrasladoDocumentoDTO
    {
        public int nsolicitud { get; set; }
        public int id_banco { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public int id_estado { get; set; }
        public string estado { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public string observa_rec { get; set; } = string.Empty;
        public string bancox { get; set; } = string.Empty;
        public string tipox { get; set; } = string.Empty;
        public DateTime? fecha_rec { get; set; }
        public string? usuario_rec { get; set; } = string.Empty;

    }

}
