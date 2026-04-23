namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCoAplFndAcuerdosModels
    { 
        public class CoAplFndAcuerdosCargaMasivaRequest
        {
            public string Usuario { get; set; } = string.Empty;
            public List<CoAplFndAcuerdosCargaItemRequest> Items { get; set; } = [];
        }


        public class CoAplFndAcuerdosCargaItemRequest
        {
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public DateTime Fecha_Firma { get; set; }
            public bool Apl_Creditos { get; set; }
            public bool Apl_Obrero { get; set; }
            public bool Apl_Sobres { get; set; }
            public bool Apl_Abonos_ord { get; set; }
            public bool Activo { get; set; }
            public string Notas { get; set; } = string.Empty;
        }

        public class CoAplFndAcuerdosGridResponse
        {
            public int Id_Acuerdo { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Estado { get; set; } = string.Empty;
            public string Ind_Creditos { get; set; } = string.Empty;
            public string Ind_Ahorro_Obrero { get; set; } = string.Empty;
            public string Ind_Sobres { get; set; } = string.Empty;
            public string Ind_Abono_Ordinario { get; set; } = string.Empty;
            public string Usuario_Registra { get; set; } = string.Empty;
            public DateTime Fecha_Registra { get; set; }
            public string Usuario_Modifica { get; set; } = string.Empty;
            public DateTime Fecha_Modifica { get; set; }
            public string Observaciones  { get; set; } = string.Empty;
            public string Cedula_Vence { get; set; } = string.Empty;
            public string Firma_Boleta { get; set; } = string.Empty;
        }
        public class CoAplFndAcuerdosDetalleResponse
        {
            public int Id_Acuerdo { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string Observaciones { get; set; } = string.Empty;
            public DateTime Firma_Boleta { get; set; }  
            public bool Estado { get; set; }
            public bool Ind_Obrero { get; set; }
            public bool Ind_Creditos { get; set; }
            public bool Ind_Sobres { get; set; }
            public bool Ind_Abono { get; set; }
            public string Usuario { get; set; } = string.Empty;
        }
        public class CoAplFndAcuerdosGuardarResponse
        {
            public int Pass { get; set; } = 0;
            public int AcuerdoId { get; set; } = 0;
            public string Movimiento { get; set; } = string.Empty;
            public string Mensaje { get; set; } = string.Empty;
            public bool Procesado { get; set; }
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty; 
        }
        public class CoAplFndAcuerdosCargaMasivaResponse
        {
            public int Procesados { get; set; }
            public int Correctos { get; set; }
            public int ConError { get; set; }
            public List<CoAplFndAcuerdosGuardarResponse> Detalle { get; set; } = [];
        }
       
        public class CoAplFndAcuerdosFiltroRequest
        {
            public string Filtro { get; set; } = string.Empty;
            public int? Estado { get; set; } // null = Todos, 1 = Activos, 0 = Inactivos
        }
        public class CoAplFndAcuerdosSocioResult
        {
            public string? Cedula { get; set; } = string.Empty;
            public string? CedulaR { get; set; } = string.Empty;
            public string? Nombre { get; set; } = string.Empty;
        }

    }
}
