namespace Galileo.Models.AH
{
    public class MovimientosPatrimonioDto
    {
        public string id_seq { get; set; } = string.Empty;
        public string tipo_aporte { get; set; } = string.Empty;
        public string tipo_aporte_id { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public DateTime? fecha { get; set; } = null;
        public string usuario { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string tcon { get; set; } = string.Empty;
        public string ncon { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public decimal? fechaproc { get; set; } 
        public string cod_institucion { get; set; } = string.Empty;
        public string institucion { get; set; } = string.Empty;
        public string sectordesc { get; set; } = string.Empty;
    }

    public class DocumentosTransaccionSifDto
    {
        public string idx { get; set; } = string.Empty;
        public string itmx { get; set; } = string.Empty;
    }

    public class TipoAportePatrimonioDto
    {
        public string id { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class MovimientosPatrimonioFiltrosDto
    {
        public DateTime fecha_inicio { get; set; } = DateTime.MinValue;
        public DateTime fecha_corte { get; set; } = DateTime.Now;
        public List<TipoAportePatrimonioDto> tipos_aporte { get; set; } = [];
        public List<DocumentosTransaccionSifDto> tipos_documento { get; set; } = [];
    }

    public class MovimientosPatrimonioConsultaRequest
    {
        public DateTime fecha_inicio { get; set; } = DateTime.MinValue;
        public DateTime fecha_corte { get; set; } = DateTime.Now;
        public string tipo_aporte { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string? documento { get; set; } = string.Empty;
        public string? cedula { get; set; } = string.Empty;
    }
}