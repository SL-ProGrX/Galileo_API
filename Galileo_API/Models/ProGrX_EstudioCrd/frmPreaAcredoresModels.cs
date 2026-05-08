namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class CrdPreaAcredoresData
    {
        public string cod_acredor { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string nombre_giro { get; set; } = string.Empty;
        public bool modifica_nombre_giro { get; set; } = false;
        public bool activo { get; set; } = false;
    }
}