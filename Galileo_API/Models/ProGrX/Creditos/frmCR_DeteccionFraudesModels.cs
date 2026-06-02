using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrDeteccionFraudesReporteRequest
    {
        public string tipo_reporte { get; set; } = string.Empty;
        public bool todas_fechas { get; set; } = false;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string estado_operacion { get; set; } = string.Empty;
        public string estado_persona { get; set; } = string.Empty;
        public string garantia { get; set; } = string.Empty;
        public string grupo { get; set; } = string.Empty;
        public bool todas_lineas { get; set; } = true;
        public string codigo { get; set; } = string.Empty;
        public string descripcion_linea { get; set; } = string.Empty;
        public string recurso { get; set; } = string.Empty;
        public string destino { get; set; } = string.Empty;
        public string comite { get; set; } = string.Empty;
        public int? id_comite { get; set; }
        public short? dias { get; set; }
        public short? meses { get; set; }
        public bool por_persona { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
        public string nombre_empresa { get; set; } = string.Empty;
    }

    public class CrDeteccionFraudesReporteResult
    {
        public string nombreReporte { get; set; } = string.Empty;
        public string folder { get; set; } = "Credito";
        public string parametros { get; set; } = string.Empty;
    }

    public class CrDeteccionFraudesLineaDescripcionDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrDeteccionFraudesCatalogosDto
    {
        public List<DropDownListaGenericaModel> operaciones { get; set; } = new();
        public List<DropDownListaGenericaModel> personas { get; set; } = new();
        public List<DropDownListaGenericaModel> garantias { get; set; } = new();
        public List<DropDownListaGenericaModel> grupos { get; set; } = new();
        public List<DropDownListaGenericaModel> comites { get; set; } = new();
        public List<DropDownListaGenericaModel> recursos { get; set; } = new();
        public List<DropDownListaGenericaModel> destinos { get; set; } = new();
    }
}