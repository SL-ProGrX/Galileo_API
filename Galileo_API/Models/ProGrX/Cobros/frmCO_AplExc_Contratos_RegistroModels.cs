namespace Galileo_API.Models.ProGrX.Cobros
{
    namespace Galileo_API.Models.ProGrX.Cobros
    {
        public class CoAplExcContratosRegistroListaRequest
        {
            public string filtro { get; set; } = string.Empty;
            public int estado { get; set; } = 1;
            public int lineas { get; set; } = 1000;
        }

        public class CoAplExcContratosRegistroListaRow: CoAplContratosRegistroListaRow
        {
            public bool? present { get; set; } = false;
        }

        public class CoAplExcContratosRegistroConsultaRequest
        {
            public long id_contrato { get; set; }
            public long id_solicitud { get; set; }
        }

        public class CoAplExcContratosRegistroData: CoAplContratosRegistroData
        {
            public bool? activo { get; set; }
        }

        public class CoAplExcContratosRegistroCreditosRequest
        {
            public string cedula { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroCreditoDbRow
        {
            public string estado_desc { get; set; } = string.Empty;
            public long id_contrato { get; set; }
            public string codigo { get; set; } = string.Empty;
            public long id_solicitud { get; set; }
            public decimal saldo { get; set; }
            public string fecha_vencimiento { get; set; } = string.Empty;
            public string fecha_firma { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroCreditoRow
        {
            public string estado { get; set; } = string.Empty;
            public string estado_desc { get; set; } = string.Empty;
            public long id_contrato { get; set; } = 0;
            public string codigo { get; set; } = string.Empty;
            public long id_solicitud { get; set; } = 0;
            public decimal saldo { get; set; } = 0;
            public DateTime? fecha_vencimiento { get; set; }

            public string proceso { get; set; } = string.Empty;

            public DateTime? fecha_firma { get; set; }
        }

        public class CoAplExcContratosRegistroGuardarRequest
        {
            public long id_contrato { get; set; } = 0;
            public string cedula { get; set; } = string.Empty;

            public DateTime?  firma_contrato { get; set; }
            public DateTime? fecha_vencimiento { get; set; }
            public int estado { get; set; } = 0;
            public long id_solicitud { get; set; } = 0;
            public string observaciones { get; set; } = string.Empty;
            public string usuario { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroGuardarResponse
        {
            public bool pass { get; set; } = false;
            public long contrato_id { get; set; } = 0;
            public string movimiento { get; set; } = string.Empty;
            public string mensaje { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroGuardarDbResponse
        {
            public int Pass { get; set; } = 0;
            public long ContratoId { get; set; } = 0;
            public string? Movimiento { get; set; } = string.Empty;
            public string? Mensaje { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroPersonaF4Request
        {
            public string texto { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroPersonaF4Row
        {
            public string cedula { get; set; } = string.Empty;
            public string cedular { get; set; } = string.Empty;
            public string nombre { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroCargaLoteRow
        {
            public string cedula { get; set; } = string.Empty;
            public long operacion { get; set; } = 0;
            public string notas { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroCargaLoteRequest
        {
            public List<CoAplExcContratosRegistroCargaLoteRow> detalle { get; set; } = new();
            public string usuario { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroCargaLoteResponse
        {
            public bool pass { get; set; } = false;
            public string mensaje { get; set; } = string.Empty;
            public int procesados { get; set; } = 0;
            public string ultima_cedula { get; set; } = string.Empty;
            public long ultima_operacion { get; set; } = 0;
        }

    }

}
