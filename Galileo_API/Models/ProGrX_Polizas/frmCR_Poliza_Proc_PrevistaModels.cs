namespace Galileo_API.Models.ProGrX_Polizas
{
    public class CrPolProcprevistaDetalleDto
    {
        public string? n_poliza { get; set; }
        public long id_solicitud { get; set; } = 0;
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public decimal monto_asegurado { get; set; } = 0;
        public decimal prima { get; set; } = 0;
        public long id_registro { get; set; } = 0;
    }

    public class CrPolProcPrevistaConciliaDto
    {
        public long id_concilia { get; set; } = 0;
        public DateTime? corte { get; set; }
        public string? cod_poliza { get; set; }
        public string? n_poliza { get; set; }
        public long id_solicitud { get; set; } = 0;
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public long? envio_id { get; set; }
        public decimal envio_monto { get; set; } = 0;
        public long? recibo_id { get; set; }
        public decimal recibo_monto { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public decimal monto_asegurado { get; set; } = 0;
        public string? factura { get; set; }
        public string? concilia_tipo { get; set; }
    }
}
