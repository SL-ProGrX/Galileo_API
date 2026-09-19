namespace Galileo.Models.INV
{
    public sealed class PaquetesFiltrosDto
    {
        public int pagina { get; set; } = 0;
        public int paginacion { get; set; } = 30;
        public string filtro { get; set; } = string.Empty;
    }

    public sealed class PaqueteDto
    {
        public int cod_paquete { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public DateTime? fecha_crea { get; set; }
        public string user_crea { get; set; } = string.Empty;
        public string user_modifica { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha_modifica { get; set; }
        public DateTime? fecha_corte { get; set; }
        public DateTime? frecuencia_horai { get; set; }
        public DateTime? frecuencia_horac { get; set; }
        public bool frecuencia_lunes { get; set; } = false;
        public bool frecuencia_martes { get; set; } = false;
        public bool frecuencia_miercoles { get; set; } = false;
        public bool frecuencia_jueves { get; set; } = false;
        public bool frecuencia_viernes { get; set; } = false;
        public bool frecuencia_sabado { get; set; } = false;
        public bool frecuencia_domingo { get; set; } = false;
    }

    public sealed class PaqueteDataLista
    {
        public int total { get; set; } = 0;
        public List<PaqueteDto> lista { get; set; } = [];
    }

    public sealed class PaqueteDetalleDto
    {
        public int linea { get; set; } = 0;
        public string cod_producto { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_paquete { get; set; } = 0;
        public decimal cantidad { get; set; } = 0;
        public decimal porc_utilidad { get; set; } = 0;
        public decimal precio { get; set; } = 0;
        public decimal imp_ventas { get; set; } = 0;
        public decimal imp_consumo { get; set; } = 0;
        public decimal total { get; set; } = 0;
        public string unidad { get; set; } = string.Empty;
    }
}