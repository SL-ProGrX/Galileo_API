namespace Galileo.Models.CPR
{
    public class TiposOrdenDto
    {
        public string tipo_orden { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
    }

    public class TipoOrdenFiltro
    {
        public int pagina { get; set; }
        public int paginacion { get; set; }
        public string filtro { get; set; } = string.Empty;
    }

    public class TiposOrdenLista
    {
        public int total { get; set; }
        public List<TiposOrdenDto> lista { get; set; } = new List<TiposOrdenDto>();
    }

    public class RangosMontos
    {
        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int monto_minimo { get; set; }
        public int monto_maximo { get; set; }
    }
}