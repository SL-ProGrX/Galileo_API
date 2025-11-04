namespace PgxAPI.Models.FSL
{
    public class FslTablaTipoLista
    {
        public int Total { get; set; }
        public List<FslTablaTipoData>? Lista { get; set; }
    }

    public class FslTablaTipoData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activa { get; set; }
    }
}