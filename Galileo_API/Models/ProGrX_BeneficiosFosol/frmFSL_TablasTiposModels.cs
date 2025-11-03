namespace PgxAPI.Models.FSL
{
    public class fslTablaTipoLista
    {
        public int Total { get; set; }
        public List<fslTablaTipoData> Lista { get; set; }
    }

    public class fslTablaTipoData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
    }
}
