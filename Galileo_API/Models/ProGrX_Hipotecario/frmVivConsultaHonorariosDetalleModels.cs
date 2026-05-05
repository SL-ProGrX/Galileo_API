namespace Galileo_API.Models.ProGrX_Hipotecario
{
    public class FrmVivConsultaHonorariosDetalleRequest
    {
        public long id_garantia { get; set; } = 0;
    }

    public class FrmVivConsultaHonorariosDetalleItem
    {
        public long linea { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string contacto { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_registro { get; set; }
    }

    public class FrmVivConsultaHonorariosDetalleResponse
    {
        public long numero_operacion { get; set; } = 0;
        public string cedula_socio { get; set; } = string.Empty;
        public string nombre_socio { get; set; } = string.Empty;
        public decimal total_monto { get; set; } = 0;
        public List<FrmVivConsultaHonorariosDetalleItem> detalle { get; set; } = [];
    }

    public class FrmVivConsultaHonorariosDetalleRawItem
    {
        public long numero_operacion { get; set; } = 0;
        public string cedula_socio { get; set; } = string.Empty;
        public string nombre_socio { get; set; } = string.Empty;
        public long linea { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public string contacto { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_registro { get; set; }
    }
}
