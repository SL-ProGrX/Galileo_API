namespace Galileo_API.Models.ProGrX.Cobros
{
    public class CoAplFndContratosRegistroListaRequest
    {
        public string filtro { get; set; } = string.Empty;
        public int estado { get; set; } = 1;
        public int lineas { get; set; } = 1000;
    }

    public class CoAplFndContratosRegistroListaRow: CoAplContratosRegistroListaRow
    {
        public bool? present { get; set; } = false;
    }

    public class CoAplFndContratosRegistroConsultaRequest
    {
        public long id_contrato { get; set; }
        public long id_solicitud { get; set; }
    }

    public class CoAplFndContratosRegistroData : CoAplContratosRegistroData
    {
        public bool? activo { get; set; }
    }

    public class CoAplFndContratosRegistroCreditosRequest
    {
        public string cedula { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroCreditoDbRow
    {
        public string estado_desc { get; set; } = string.Empty;
        public long id_contrato { get; set; }
        public string codigo { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public decimal saldo { get; set; }
        public string fecha_vencimiento { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroCreditoRow
    {
        public string estado_desc { get; set; } = string.Empty;
        public long id_contrato { get; set; }
        public string codigo { get; set; } = string.Empty;
        public long id_solicitud { get; set; }
        public decimal saldo { get; set; }
        public DateTime? fecha_vencimiento { get; set; }
    }

    public class CoAplFndContratosRegistroGuardarRequest: CoAplContratosRegistroGuardarRequest
    {
        public bool? guarda { get; set; } = false;
    }

    public class CoAplFndContratosRegistroGuardarResponse: CoAplContratosRegistroGuardarResponse
    {
        public bool? resp { get; set; } = false;
    }

    public class CoAplFndContratosRegistroGuardarDbResponse
    {
        public int Pass { get; set; }
        public long ContratoId { get; set; }
        public string? Movimiento { get; set; } = string.Empty;
        public string? Mensaje { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroPersonaF4Request
    {
        public string texto { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroPersonaF4Row
    {
        public string cedula { get; set; } = string.Empty;
        public string cedular { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroCargaLoteRow
    {
        public string cedula { get; set; } = string.Empty;
        public long operacion { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroCargaLoteRequest
    {
        public List<CoAplFndContratosRegistroCargaLoteRow> detalle { get; set; } = new();
        public string usuario { get; set; } = string.Empty;
    }

    public class CoAplFndContratosRegistroCargaLoteResponse
    {
        public bool pass { get; set; } = false;
        public string mensaje { get; set; } = string.Empty;
        public int procesados { get; set; } = 0;
        public string ultima_cedula { get; set; } = string.Empty;
        public long ultima_operacion { get; set; } = 0;
    }

}
