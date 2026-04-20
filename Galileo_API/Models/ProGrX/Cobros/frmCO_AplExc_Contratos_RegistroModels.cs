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

        public class CoAplExcContratosRegistroListaRow
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

        public class CoAplExcContratosRegistroConsultaRequest
        {
            public long id_contrato { get; set; }
            public long id_solicitud { get; set; }
        }

        public class CoAplExcContratosRegistroData
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

        public class CoAplExcContratosRegistroCreditosRequest
        {
            public string cedula { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroCreditoRow
        {
            public string estado_desc { get; set; } = string.Empty;
            public long id_contrato { get; set; }
            public string codigo { get; set; } = string.Empty;
            public long id_solicitud { get; set; }
            public decimal saldo { get; set; }
            public DateTime? fecha_vencimiento { get; set; }
        }

        public class CoAplExcContratosRegistroGuardarRequest
        {
            public long id_contrato { get; set; }
            public string cedula { get; set; } = string.Empty;
            public DateTime? fecha_vencimiento { get; set; }
            public int estado { get; set; }
            public long id_solicitud { get; set; }
            public string observaciones { get; set; } = string.Empty;
            public string usuario { get; set; } = string.Empty;
        }

        public class CoAplExcContratosRegistroGuardarResponse
        {
            public bool pass { get; set; }
            public long contrato_id { get; set; }
            public string movimiento { get; set; } = string.Empty;
            public string mensaje { get; set; } = string.Empty;
        }
    }

}
