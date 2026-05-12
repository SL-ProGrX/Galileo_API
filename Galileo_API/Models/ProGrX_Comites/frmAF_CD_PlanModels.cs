namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdPlanMensajeData
    {
        public int num_mensaje { get; set; } = 0;
        public string cod_comite { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string usuario { get; set; } = string.Empty;
        public DateTime? vencimiento { get; set; }
        public string mensaje { get; set; } = string.Empty;
    }
}