namespace Galileo.Models.INV
{
    public sealed class TomaFisicaDto
    {
        public int consecutivo { get; set; } = 0;
        public string cod_bodega { get; set; } = string.Empty;
        public string bodega { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public DateTime? fecha_crea { get; set; }
        public string user_crea { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? fecha_aplica { get; set; }
        public string user_aplica { get; set; } = string.Empty;
        public string tipo_asiento { get; set; } = string.Empty;
        public string num_asiento { get; set; } = string.Empty;
        public DateTime? fecha_asiento { get; set; }
        public string causa_entrada { get; set; } = string.Empty;
        public string causa_salida { get; set; } = string.Empty;
        public int cod_proveedor_entrada { get; set; } = 0;
        public int cod_entradag { get; set; } = 0;
        public int cod_salidag { get; set; } = 0;
    }

    public sealed class TomaFisicaDetalleDto
    {
        public int consecutivo { get; set; } = 0;
        public string cod_bodega { get; set; } = string.Empty;
        public string bodega { get; set; } = string.Empty;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string ubicacion { get; set; } = string.Empty;
        public decimal existencia_logica { get; set; } = 0m;
        public decimal existencia_fisica { get; set; } = 0m;
        public decimal diferencia { get; set; } = 0m;
    }

    public sealed class TomaFisicaListaFiltros
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
        public string sortField { get; set; } = "consecutivo";
        public int sortOrder { get; set; } = -1;
    }

    public sealed class TomaFisicaDetalleFiltros
    {
        public int consecutivo { get; set; } = 0;
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
    }

    public sealed class TomaFisicaGuardarRequest
    {
        public TomaFisicaDto toma { get; set; } = new();
        public List<TomaFisicaDetalleDto> detalle { get; set; } = [];
        public string usuario { get; set; } = string.Empty;
    }

    public sealed class TomaFisicaInventarioRequest
    {
        public int consecutivo { get; set; } = 0;
        public string cod_bodega { get; set; } = string.Empty;
        public DateTime? fecha_corte { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<string>? cod_productos { get; set; }
    }

    public sealed class TomaFisicaProductoRequest
    {
        public string cod_bodega { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }
}