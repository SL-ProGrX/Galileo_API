namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplContratosRegistroListaRow
    {
        public long id_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public DateTime? firma_contrato { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public int estado { get; set; }
        public string estado_desc { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public string usuario_registra { get; set; } = string.Empty;
        public DateTime? fecha_registra { get; set; }
    }

    public class CoAplContratosRegistroData
    {
        public long id_contrato { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string observaciones { get; set; } = string.Empty;
        public DateTime? firma_contrato { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public int estado { get; set; }
        public string estado_desc { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
    }

    public class CoAplContratosRegistroGuardarRequest
    {
        public long id_contrato { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public DateTime? firma_contrato { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
        public int estado { get; set; } = 0;
        public long id_solicitud { get; set; } = 0;
        public string observaciones { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CoAplContratosRegistroGuardarResponse
    {
        public bool pass { get; set; } = false;
        public long contrato_id { get; set; } = 0;
        public string movimiento { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }
}
