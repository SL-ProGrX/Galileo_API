namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaEstudiov2CausaDto
    {
        public int id_causa { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public DateTime fecha { get; set; }
        public string tipo { get; set; } = string.Empty;
    }

    public class FrmPreaEstudiov2CausasGuardarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public string cod_preanalisis { get; set; } = string.Empty;
        public int id_causa { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
    }
}
