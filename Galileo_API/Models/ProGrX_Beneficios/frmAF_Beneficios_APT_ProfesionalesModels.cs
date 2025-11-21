namespace Galileo.Models.AF
{
    public class BeneAptProfesionalesDataLista
    {
        public int total { get; set; }
        public List<BeneAptProfesionalesData> lista { get; set; } = new List<BeneAptProfesionalesData>();
    }

    public class BeneAptProfesionalesData
    {
        public long id_profesional { get; set; }
        public string identificacion { get; set; } = string.Empty;
        public string? nombre { get; set; }
        public string? usuario { get; set; }
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }
    
    public class AfiAptProFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }
}