namespace PgxAPI.Models.GG_PE
{
    public class PeObjetivosEstrategicosFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class PeObjetivosEstrategicosDatosLista
    {
        public int total { get; set; } = 0;
        public List<PeObjetivosEstrategicosDTO>? data { get; set; }
    }

    public class PeObjetivosEstrategicosDTO
    {
        public int objetivo_id { get; set; }
        public int? perspectiva_id { get; set; }
        public string? nombre_pespectiva { get; set; }
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public string? indicador_clave { get; set; }
        public float? meta { get; set; }
        public string? unidad_medida { get; set; }
        public bool activo { get; set; }
        public string registro_usuario { get; set; }
        public DateTime registro_fecha { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

    }

}
