using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrCambioTasasInicialResponse
    {
        public List<DropDownListaGenericaModel> garantias { get; set; } = [];
        public List<DropDownListaGenericaModel> divisas { get; set; } = [];
        public List<DropDownListaGenericaModel> estadosPersona { get; set; } = [];
        public List<DropDownListaGenericaModel> instituciones { get; set; } = [];
        public List<DropDownListaGenericaModel> deductoras { get; set; } = [];
        public List<DropDownListaGenericaModel> estadosLaboral { get; set; } = [];
        public List<DropDownListaGenericaModel> recursos { get; set; } = [];
        public List<DropDownListaGenericaModel> destinos { get; set; } = [];
        public DateTime fechaServidor { get; set; }
        public decimal glngFechaCR { get; set; }
        public decimal tbp { get; set; }
    }

    public class CrCambioTasasCatalogosLineaResponse
    {
        public List<DropDownListaGenericaModel> recursos { get; set; } = [];
        public List<DropDownListaGenericaModel> destinos { get; set; } = [];
    }

    public class CrCambioTasasConsultaRequest
    {
        public string? linea { get; set; }
        public string? garantia { get; set; }
        public string? destino { get; set; }
        public string? recurso { get; set; }
        public int? institucion { get; set; }
        public int? deductora { get; set; }
        public string? divisa { get; set; }
        public string? estadoPersona { get; set; }
        public string? estadoLaboral { get; set; }
        public DateTime? formalizaInicio { get; set; }
        public DateTime? formalizaCorte { get; set; }
        public bool aplicaPlazo { get; set; }
        public int? plazoInicio { get; set; }
        public int? plazoCorte { get; set; }
        public bool aplicaTasa { get; set; }
        public decimal? tasaInicio { get; set; }
        public decimal? tasaCorte { get; set; }
        public string? cobroTipo { get; set; }
        public string? operacionTipo { get; set; }
        public bool aplicaPriDeduc { get; set; }
        public string? priDeducFiltro { get; set; }
        public int? priDeduc { get; set; }
        public bool aplicaUltDeduc { get; set; }
        public string? ultDeducFiltro { get; set; }
        public int? ultDeduc { get; set; }
        public string tasaTipo { get; set; } = "R";
        public string tasaAplTipo { get; set; } = "N";
        public string tasaAplCtas { get; set; } = "R";
        public decimal tasaAplRef { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class CrCambioTasasConsultaResponse
    {
        public List<CrCambioTasasOperacionRow> operaciones { get; set; } = [];
        public CrCambioTasasResumen resumen { get; set; } = new();
    }

    public class CrCambioTasasOperacionRow
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal montoapr { get; set; }
        public decimal saldo { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public decimal cuota { get; set; }
        public decimal cuota_nueva { get; set; }
        public DateTime? fechaforp { get; set; }
        public decimal tasa_original { get; set; }
        public decimal tasa_nueva { get; set; }
        public int plazo_restante { get; set; }
        public DateTime? inicio_ajuste { get; set; }
        public string garantia_desc { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
        public decimal tasa_piso { get; set; }
        public decimal tbppuntosadd { get; set; }
        public decimal tasa_pts_bono { get; set; }
        public decimal liqtasa { get; set; }
    }

    public class CrCambioTasasResumen
    {
        public int casos { get; set; }
        public decimal cuotasActuales { get; set; }
        public decimal cuotasNuevas { get; set; }
        public decimal diferenciaInteres { get; set; }
    }

    public class CrCambioTasasAplicarRequest
    {
        public List<CrCambioTasasOperacionAplicar> operaciones { get; set; } = [];
        public string tasaTipo { get; set; } = "R";
        public string tasaAplTipo { get; set; } = "N";
        public string tasaAplCtas { get; set; } = "R";
        public decimal tasaAplRef { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
    }

    public class CrCambioTasasOperacionAplicar
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public decimal tasa { get; set; }
        public decimal tasa_nueva { get; set; }
        public decimal cuota_nueva { get; set; }
        public int plazo_restante { get; set; }
    }
}
