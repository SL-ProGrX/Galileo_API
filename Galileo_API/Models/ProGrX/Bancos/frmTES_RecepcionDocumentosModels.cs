namespace PgxAPI.Models.ProGrX.Bancos
{
    public class TesUbiRemesaDto
    {
        public int cod_remesa { get; set; }
        public string cod_ubicacion { get; set; } = string.Empty;
        public string cod_ubicacion_destino { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string oubicacion { get; set; } = string.Empty;
        public string dubicacion { get; set; } = string.Empty;
    }

    public class TesRecepcionDocumentoDto
    {
        public int nsolicitud { get; set; }
        public int id_banco { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string ndocumento { get; set; } = string.Empty;
        public bool estado { get; set; }
        public string observacion { get; set; } = string.Empty;
        public string observa_rec { get; set; } = string.Empty;
        public string bancox { get; set; } = string.Empty;
        public string tipox { get; set; } = string.Empty;
        public DateTime fecha_rec { get; set; }
        public string usuario_rec { get; set; } = string.Empty;
    }

    public class TesRecepcionDocumentoFiltros
    {
        public int cod_remesa { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<TesRecepcionDocumentoDto>? solicitudes { get; set; }
    }
}