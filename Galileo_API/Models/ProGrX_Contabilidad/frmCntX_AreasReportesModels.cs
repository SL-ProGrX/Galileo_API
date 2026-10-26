using System.Collections.Generic;

namespace Galileo_API.Models.ProGrX_Contabilidad
{
    public sealed class CntXAreasReportesAreaData
    {
        public int cod_area { get; set; } = 0;

        public string descripcion { get; set; } =
            string.Empty;
    }

    public sealed class CntXAreasReportesGenerarRequest
    {
        public int cod_area { get; set; } = 0;

        public int periodo_anio { get; set; } = 0;

        public int periodo_mes { get; set; } = 0;

        public bool comparar { get; set; } = false;

        public int periodo_comparativo_anio { get; set; } = 0;

        public int periodo_comparativo_mes { get; set; } = 0;

        public string tipo_reporte { get; set; } =
            string.Empty;

        public string usuario { get; set; } =
            string.Empty;
    }

    public sealed class CntXAreasReportesGenerarData
    {
        public string nombre_reporte { get; set; } =
            string.Empty;

        public string folder { get; set; } =
            string.Empty;

        public Dictionary<string, object?> parametros
        {
            get;
            set;
        } = new();
    }
}