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

    public class CrPolProcPrevistaDetalleLineaModel
    {
        public int linea { get; set; } = 0;               // pLinea
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string n_poliza { get; set; } = string.Empty;
        public decimal monto_asegurado { get; set; } = 0;  // pMonto
        public decimal prima { get; set; } = 0;           // pPrima
        public long operacion { get; set; } = 0;          // pOperacion
    }

    public class CrPolProcPrevistaDetalleAddRequest
    {
        public string cod_poliza { get; set; } = string.Empty;   // pPoliza (VB)
        public DateTime? corte { get; set; }                      // pCorte
        public string factura { get; set; } = string.Empty;      // txtR_Factura
        public List<CrPolProcPrevistaDetalleLineaModel> lineas { get; set; } = new();
    }

    public class CrPolProcPrevistaDetalleEliminarRequest
    {
        public List<long> id_registros { get; set; } = new();
        public string motivo { get; set; } = "Poliza Descartada por Cancelacion";
    }

    public class CrPolProcPrevistaConciliaConsultaRequest
    {
        public DateTime? corte { get; set; }
        public string cod_poliza { get; set; } = string.Empty;
    }

}
