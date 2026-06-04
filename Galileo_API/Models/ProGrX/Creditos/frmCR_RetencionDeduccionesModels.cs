using Galileo.Models;

namespace Galileo_API.Models.ProGrX.Credito
{
    public class CrRetencionDeduccionesData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
    }

    public class CrRetencionDeduccionesPantallaData
    {
        public List<DropDownListaGenericaModel> clientes { get; set; } = new();
        public List<DropDownListaGenericaModel> instituciones { get; set; } = new();
        public List<DropDownListaGenericaModel> formatos { get; set; } = new();
        public List<DropDownListaGenericaModel> tipos { get; set; } = new();
        public string formato_default { get; set; } = "01";
        public string tipo_default { get; set; } = "P";
        public DateTime fecha_servidor { get; set; }
        public string proceso_default { get; set; } = string.Empty;
    }

    public class CrRetencionDeduccionesRequestBase
    {
        public string codigo { get; set; } = string.Empty;
        public int? cod_institucion { get; set; }
        public string formato { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string proceso { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
    }

    public class CrRetencionDeduccionesObtenerRequest
        : CrRetencionDeduccionesRequestBase
    {
    }

    public class CrRetencionDeduccionesResultadoData
    {
        public List<CrRetencionDeduccionesData> deducciones { get; set; } = new();
        public decimal total_monto { get; set; } = 0;
        public int total_casos { get; set; } = 0;
    }

    public class CrRetencionDeduccionesArchivoRequest
        : CrRetencionDeduccionesRequestBase
    {
    }

    public class CrRetencionDeduccionesArchivoData
    {
        public string nombre_archivo { get; set; } = string.Empty;
        public string contenido { get; set; } = string.Empty;
        public string content_type { get; set; } = "text/plain";
    }
}