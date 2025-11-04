namespace PgxAPI.Models.ProGrX_Personas
{
    public class BienesTipoData
    {
        public string Bien_Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; } = false;
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class BienesTipoLista
    {
        public int Total { get; set; }
        public List<BienesTipoData> Lista { get; set; } = new List<BienesTipoData>();
    }
}