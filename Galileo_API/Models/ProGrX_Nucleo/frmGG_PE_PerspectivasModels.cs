namespace PgxAPI.Models.GG_PE
{
    public class PePerspectivasFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class PePerspectivasDatosLista
    {
        public int total { get; set; } = 0;
        public List<PePerspectivasDto>? data { get; set; }
    }

    public class PePerspectivasDto
    {
        public int perspectiva_id { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int pe_id { get; set; }
        public string objetivo_a_1 { get; set; } = string.Empty;
        public string? objetivo_a_2 { get; set; } 
        public string? objetivo_a_3 { get; set; } 
        public string responsable { get; set; } = string.Empty;
        public bool activa { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

    public class PePlanesLista
    {
        public string? item { get; set; } 
        public string? descripcion { get; set; } 
    }
}