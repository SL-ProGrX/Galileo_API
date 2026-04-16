namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class VivTiposDesembolsosData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool niveldesembolso { get; set; } = false;
        public bool nivelformalizacion { get; set; } = false;
        public bool aplicaingeniero { get; set; } = false;
        public bool aplicaabogado { get; set; } = false;
        public bool aplicainteres { get; set; } = false;
        public decimal porcentaje { get; set; } = 0;
        public string cuenta { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string registrousuario { get; set; } = string.Empty;
        public DateTime? registrofecha { get; set; }
    }
}
