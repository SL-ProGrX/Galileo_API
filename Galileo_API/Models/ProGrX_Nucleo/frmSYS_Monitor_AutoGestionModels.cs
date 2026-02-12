namespace Galileo.Models.ProGrX_Nucleo
{
    public class MonitorAutoGestionListaData
    {
        public long Cod_Solicitud { get; set; }
        public string Estado_Desc { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Linea_Desc { get; set; } = "";
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public decimal Tasa { get; set; }
        public decimal Cuota { get; set; }
        public string Garantia_Desc { get; set; } = "";
        public DateTime? Registro_Fecha { get; set; }
        public DateTime? Res_Fecha { get; set; }
        public string? Res_Codigo { get; set; } = "";
        public string? Res_Tipo { get; set; } = "";
        public string Tramite_Estado_Desc { get; set; } = "";
    }

    public class MonitorAutoGestionLista
    {
        public int total { get; set; }
        public List<MonitorAutoGestionListaData>? lista { get; set; }
    }

    public class MonitorAutoGestionCasoDetalle
    {
        public long Cod_Solicitud { get; set; }
        public string Estado_Desc { get; set; } = "";
        public string Estado { get; set; } = "";
        public string Garantia_Desc { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Codigo { get; set; } = "";
        public string Linea_Desc { get; set; } = "";
        public decimal Monto { get; set; }
        public int Plazo { get; set; }
        public decimal Tasa { get; set; }
        public decimal Cuota { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; } = "";
        public DateTime? Res_Fecha { get; set; }
        public string? Res_Usuario { get; set; } = "";
        public string? Res_Codigo { get; set; } = "";
        public string? Notas { get; set; } = "";
        public bool Refunde_Ind { get; set; }
    }

    public class MonitorAutoGestionResumenData
    {
        public string Estado { get; set; } = "";
        public int Casos { get; set; }
        public decimal Monto { get; set; }
    }

    public class MonitorAutoGestionResumenLista
    {
        public int total { get; set; }
        public List<MonitorAutoGestionResumenData>? lista { get; set; }
    }

    public class MonitorAutoGestionAdjuntoData
    {
        public long Archivo_Id { get; set; }
        public string Tipo_Adjunto { get; set; } = "";
        public string Archivo_Nombre { get; set; } = "";
        public string Archivo_Tipo { get; set; } = "";
    }

    public class MonitorAutoGestionAdjuntosLista
    {
        public int total { get; set; }
        public List<MonitorAutoGestionAdjuntoData>? lista { get; set; }
    }

    public class MonitorAutoGestionBuscarRequest
    {
        public string? estado { get; set; } = null;
        public string? tramite_estado_id { get; set; } = null;
        public string fechaTipo { get; set; } = "Registro";
        public DateTime fechaInicio { get; set; }
        public DateTime fechaFin { get; set; }
        public string? codigoLinea { get; set; } = null;
        public string? cedula { get; set; } = null;
    }

    public class MonitorAutoGestionCasoRequest
    {
        public long cod_solicitud { get; set; }
    }

    public class MonitorAutoGestionAdjuntosRequest
    {
        public long cod_solicitud { get; set; }
    }

    public class MonitorAutoGestionDescargarAdjuntoRequest
    {
        public long archivo_id { get; set; }
    }

    public class MonitorAutoGestionResolucionRequest
    {
        public long cod_solicitud { get; set; }
        public string resolucion { get; set; } = "P";
        public string notas { get; set; } = "";
        public string usuario { get; set; } = "";
        public string gestion { get; set; } = "S";
    }

    public class MonitorAutoGestionResolucionResponse
    {
        public long cod_solicitud { get; set; }
        public string estado { get; set; } = "";
        public string estado_desc { get; set; } = "";
        public DateTime? res_fecha { get; set; }
        public string? res_usuario { get; set; } = "";
        public string? res_codigo { get; set; } = "";
        public string? notas { get; set; } = "";
    }

    public class MonitorAutoGestionExportRequest : MonitorAutoGestionBuscarRequest
    {
        public string formato { get; set; } = "Excel"; // "PDF" | "Excel"
    }


    public class MonitorAutoGestionListaFiltroDto
    {
        public string? estado { get; set; }                 // Se usa Trim().Substring(0,1)
        public string? tramite_estado_id { get; set; }       // Se usa Trim().Substring(0,1)
        public DateTime fechaInicio { get; set; }            // Obligatorio por uso directo
        public DateTime fechaFin { get; set; }               // Obligatorio por uso directo
        public string? fechaTipo { get; set; }               // "Todas" / "Resolución" / otro
        public string? codigoLinea { get; set; }
        public string? cedula { get; set; }
        public MonitorAutoGestionFiltrosDto? filtros { get; set; }
    }

    public class MonitorAutoGestionFiltrosDto
    {
       
    public string? filtro { get; set; }      // Búsqueda tipo Q

    // Orden
    public string? sortField { get; set; }   // nombre de columna
    public int? sortOrder { get; set; }      // 1 = ASC, 0 = DESC (según tu lógica)

    // Paginación
    public int? pagina { get; set; }         // offset (si tú lo tratas como offset)
    public int? paginacion { get; set; }     // tamaño página (fetch)                 // Búsqueda tipo Q
    }
}
