namespace Galileo.Models.AF
{
    public class AfTipoSancionesDto
    {
        public string tipo_sancion { get; set; } = string.Empty;
        public string? descripcion { get; set; }
        public string? codigo_cobro { get; set; }
        public string? plazo_maximo { get; set; }
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class AfTipoSancionesDtoLista
    {
        public int total { get; set; }
        public List<AfTipoSancionesDto> lista { get; set; } = new List<AfTipoSancionesDto>();
    }

    public class AfiTipoSancionfiltros
    {
        public int? pagina { get; set; }
        public int? paginacion { get; set; }
        public string? filtro { get; set; }
    }

    public class BeneListaRetencion
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public float plazo { get; set; }
    }
}