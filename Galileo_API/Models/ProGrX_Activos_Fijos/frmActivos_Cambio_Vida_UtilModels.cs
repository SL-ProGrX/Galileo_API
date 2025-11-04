namespace PgxAPI.Models.ProGrX_Activos_Fijos
{
    public class FrmActivosCambioVidaUtilModels
    {
        public class ActivoLite
        {
            public string numPlaca { get; set; } = string.Empty;
            public string? placaAlterna { get; set; }
            public string nombre { get; set; } = string.Empty;
        }

        public class ActivoLiteLista
        {
            public int total { get; set; }
            public List<ActivoLite>? lista { get; set; }
        }

        public class ActivoData
        {
            public string? numPlaca { get; set; }
            public string? placaAlterna { get; set; }
            public string? nombre { get; set; }
            public string? tipoActivo { get; set; }
            public string? tipoActivoDesc { get; set; }
            public int vidaUtil { get; set; }
            public string? vidaUtilEn { get; set; }
            public string? metDepreciacion { get; set; }
            public string? resumenActual { get; set; }
        }
        
        public class ActivosCambioVUFiltros
        {
            public int? pagina { get; set; }
            public int? paginacion { get; set; }   
            public string? filtro { get; set; } 
            public string? placa { get; set; }   
            public string? alterna { get; set; }
            public string? nombre { get; set; }   
            public string? sortField { get; set; }
            public int? sortOrder { get; set; }
        }

        public class ActivoBuscarResponse
        {
            public ActivoData? activo { get; set; }
        }

        public class MetodoDepreciacionData
        {
            public string? codigo { get; set; }            
            public string? descripcion { get; set; }
            public string? activo { get; set; }            
        }

        public class MetodoDepreciacionLista
        {
            public int total { get; set; }
            public List<MetodoDepreciacionData>? lista { get; set; }
        }

        public class CambioVidaUtilAplicarRequest
        {
            public string? numPlaca { get; set; }
            public int nuevaVidaUtil { get; set; }          
            public string unidad { get; set; } = "A";
            public string? notas { get; set; }
            public string? usuario { get; set; }
        }

        public class CambioVidaUtilAplicarResponse : ActivoData
        {
            public string? mensaje { get; set; }
        }
    }
}