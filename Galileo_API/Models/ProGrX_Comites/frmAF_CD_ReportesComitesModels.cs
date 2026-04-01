using Galileo.Models;

namespace Galileo_API.Models.ProGrX_Comites
{
    public class AfCdReporteCatalogoDto
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string? codigo_padre { get; set; }
        public short nivel { get; set; }
        public short orden { get; set; }
        public bool es_hoja { get; set; }
    }

    public class AfCdReporteTipoDto
    {
        public string codigo_opcion { get; set; } = string.Empty;
        public string codigo_reporte { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfCdReportesComitesParametrosInicialesDto
    {
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public List<DropDownListaGenericaModel> estados { get; set; } = new();
        public List<AfCdReporteTipoDto> tipos_reporte { get; set; } = new();
    }

    public class AfCdReporteDefinicionDto
    {
        public string codigo_opcion { get; set; } = string.Empty;

        public bool requiere_fecha_inicio { get; set; }
        public bool requiere_fecha_corte { get; set; }
        public bool requiere_comite { get; set; }
        public bool requiere_promotor { get; set; }
        public bool requiere_estado { get; set; }
        public bool requiere_actividad { get; set; }
        public bool requiere_tipo_reporte { get; set; }

        public bool usa_stored_proc { get; set; }
    }

    public class AfCdComiteInfoDto
    {
        public string cod_comite { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfCdActividadInfoDto
    {
        public string cod_actividad { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class AfCdPromotorInfoDto
    {
        public int id_promotor { get; set; }
        public string nombre { get; set; } = string.Empty;
    }
}