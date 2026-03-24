namespace Galileo_API.Models.ProGrX_Contabilidad
{
        public class CntxMayorizacionProcesarDto
        {
            public int? anio { get; set; }
            public int? mes { get; set; }
            public string? tipo_aplicacion { get; set; } = string.Empty; 
            public string? tipo_filtro { get; set; } = string.Empty;
            public string? tipo_asiento { get; set; }
            public string? fecha_inicio { get; set; }
            public string? fecha_fin { get; set; }
            public string? usuario { get; set; } = string.Empty;
        }


    }



