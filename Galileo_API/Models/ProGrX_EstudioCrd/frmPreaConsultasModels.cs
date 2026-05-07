using Galileo.Models;

namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class FrmPreaConsultasModels
    {
        public class PreaConsultasFiltroRequest
        {
            public string? Usuario { get; set; }
            public string? Estado { get; set; }
            public string? TipoFecha { get; set; }
            public DateTime? FechaInicio { get; set; }
            public DateTime? FechaCorte { get; set; }
            public string? CodLinea { get; set; }
            public string? CodDestino { get; set; }
            public int? CodInstitucion { get; set; }
            public int? IdComite { get; set; }
            public string? ClasificaGarantia { get; set; }
            public string? ClasificaMorosidad { get; set; }
            public string? ClasificaCapacidad { get; set; }
            public string? ClasificaEndeudamiento { get; set; }
            public string? ClasificaHistorial { get; set; }
            public string? TramiteEstado { get; set; }
            public string TipoResumen { get; set; } = "LINEA";
            public string? Filtro { get; set; } //filtro del buscar en tablas o buscador
            public int? Pagina { get; set; } = 1;//pagina de la tabla
            public int? Paginacion { get; set; } = 30; //paginacion de la tabla
            public int? SortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
            public string? SortField { get; set; } //campo por el cual se ordena
        }

        public class ConsultaLista
        {
            public int total { get; set; } = 0; 
            public List<PreaConsultasGridModel> lista { get; set; } = [];

        }
        public class PreaConsultasGridModel
        {
            public string Btn { get; set; } = string.Empty;
            public string Expediente { get; set; } = string.Empty;
            public string EstadoDesc { get; set; } = string.Empty;
            public string Cedula { get; set; } = string.Empty;
            public string Nombre { get; set; } = string.Empty;
            public string LineaDesc { get; set; } = string.Empty;
            public string DestinoDesc { get; set; } = string.Empty;
            public decimal Monto { get; set; }
            public int Plazo { get; set; }
            public decimal Tasa { get; set; }
            public decimal Cuota { get; set; }
            public decimal Refundiciones { get; set; }
            public decimal Desembolsos { get; set; }
            public decimal MontoColocado { get; set; }
            public string InstitucionDesc { get; set; } = string.Empty;
            public string DepartamentoDesc { get; set; } = string.Empty;
            public string OficinaDesc { get; set; } = string.Empty;
            public string Usuario { get; set; } = string.Empty;
            public string ClasificaCapacidad { get; set; } = string.Empty;
            public string ClasificaEndeudamiento { get; set; } = string.Empty;
            public string ClasificaHistorial { get; set; } = string.Empty;
            public string ClasificaGarantia { get; set; } = string.Empty;
            public string ClasificaMorosidad { get; set; } = string.Empty;
            public DateTime? RegistroFecha { get; set; }
            public DateTime? GestionFecha { get; set; }
            public string Operacion { get; set; } = string.Empty;
            public string TramiteDesc { get; set; } = string.Empty;
        }
        public class PreaConsultasResumenModel
        {
            public string Codigo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public int Casos { get; set; }
            public decimal Monto { get; set; }
            public decimal Refundiciones { get; set; }
            public decimal Desembolsos { get; set; }
            public decimal MontoColocado { get; set; }
        }
        public class PreaConsultasCatalogosResponse
        {
            public List<DropDownListaGenericaModel> Capacidad { get; set; } = [];
            public List<DropDownListaGenericaModel> Endeudamiento { get; set; } = [];
            public List<DropDownListaGenericaModel> Garantia { get; set; } = [];
            public List<DropDownListaGenericaModel> Historial { get; set; } = [];
            public List<DropDownListaGenericaModel> Morosidad { get; set; } = [];
            public List<DropDownListaGenericaModel> Estados { get; set; } = [];
            public List<DropDownListaGenericaModel> TiposFecha { get; set; } = [];
            public List<DropDownListaGenericaModel> Tramites { get; set; } = [];
            public DateTime FechaInicio { get; set; }
            public DateTime FechaCorte { get; set; }
        }
   


    }
}
