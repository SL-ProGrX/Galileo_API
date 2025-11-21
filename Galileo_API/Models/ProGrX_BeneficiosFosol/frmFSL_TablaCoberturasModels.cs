namespace Galileo.Models.FSL
{
    public class FslTablaAplicacionData
    {
        public int linea { get; set; }
        public int mes_inicio { get; set; }
        public int mes_corte { get; set; }
        public float cobertura { get; set; }
        public string tipo { get; set; } = string.Empty;
        public string registra_usuario { get; set; } = string.Empty;
    }

    public class FslTablaAplicacionFiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
        public string? tipo { get; set; }
    }

    public class FslTablaAplicacionDataLista
    {
        public int Total { get; set; }
        public List<FslTablaAplicacionData>? coberturas { get; set; }
    }
}