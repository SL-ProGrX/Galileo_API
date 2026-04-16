namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class ViviendaContactosData
    {
        public int idcontacto { get; set; } = 0;
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string profesional { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime? suspensioninicio { get; set; }
        public DateTime? suspensioncorte { get; set; }
        public string observacion { get; set; } = string.Empty;
        public bool suspendeactual { get; set; } = false;
    }
}
