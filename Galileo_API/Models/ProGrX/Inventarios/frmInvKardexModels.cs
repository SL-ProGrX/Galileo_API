namespace Galileo.Models.INV
{
    public class InvKardexBodegaDto
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class InvKardexMovimientosListaDto
    {
        public int total { get; set; } = 0;
        public List<InvKardexMovimientoDto> movimientos { get; set; } = new List<InvKardexMovimientoDto>();
    }

    public class InvKardexMovimientoDto
    {
        public DateTime fecha { get; set; }
        public string producto { get; set; } = string.Empty;
        public string tipox { get; set; } = string.Empty;
        public string origen { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public decimal existencia { get; set; } = 0m;
        public decimal cantidad { get; set; } = 0m;
        public decimal existenciax { get; set; } = 0m;
        public decimal precio { get; set; } = 0m;
        public decimal totalsinimp { get; set; } = 0m;
        public decimal impventas { get; set; } = 0m;
        public decimal impconsumo { get; set; } = 0m;
        public decimal totalconimp { get; set; } = 0m;
        public string bodega { get; set; } = string.Empty;
        public string bodegaenlace { get; set; } = string.Empty;
    }

    public class InvKardexMovimientosFiltro
    {
        public string fecha_inicio { get; set; } = string.Empty;
        public string fecha_corte { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string cod_bodega { get; set; } = string.Empty;
        public string vfiltro { get; set; } = string.Empty;
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 0;
    }
}