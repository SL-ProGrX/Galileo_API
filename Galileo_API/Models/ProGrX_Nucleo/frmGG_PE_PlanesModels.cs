namespace PgxAPI.Models.GG_PE
{
    public class PePlanesFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class PePlanesDatosLista
    {
        public int total { get; set; } = 0;
        public List<PePlanesDto>? data { get; set; }
    }

    public class PePlanesDto
    {
        public int pe_id { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public DateTime inicio { get; set; }
        public DateTime finalizacion { get; set; }
        public string estado { get; set; } = string.Empty;
        public string mision { get; set; } = string.Empty;
        public string vision { get; set; } = string.Empty;
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
    }
}
