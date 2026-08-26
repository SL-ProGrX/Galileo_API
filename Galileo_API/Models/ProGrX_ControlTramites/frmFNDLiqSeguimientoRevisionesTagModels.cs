namespace Galileo_API.BusinessLogic.ProGrX_ControlTramites
{
    public class FndLiqSeguimientoRevisionesTagLiquidacionesListaResult
    {
        public int total { get; set; }

        public List<FndLiqSeguimientoRevisionesTagLiquidacionData> lista
        {
            get;
            set;
        } = new();
    }

    public class FndLiqSeguimientoRevisionesTagLiquidacionData
    {
        public long consecutivo { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
        public string cod_plan { get; set; } = string.Empty;
        public long cod_contrato { get; set; }
        public string retiene { get; set; } = string.Empty;
        public string banco_descripcion { get; set; } = string.Empty;
    }

    public class FndLiqSeguimientoRevisionesTagSeguimientoData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string tag_descripcion { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
    }

    public class FndLiqSeguimientoRevisionesTagEtiquetaData
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string tag_descripcion { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
    }

    public class FndLiqSeguimientoRevisionesTagRevisionData
    {
        public int id_error { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool seleccionado { get; set; }
        public bool aplicado { get; set; }
        public string mensaje { get; set; } = string.Empty;
        public long? linea_err { get; set; }
    }

    public class FndLiqSeguimientoRevisionesTagAplicarRevisionRequest
    {
        public int? id_error { get; set; }
        public bool? seleccionado { get; set; }
    }

    public class FndLiqSeguimientoRevisionesTagAplicarRequest
    {
        public string? cedula { get; set; }
        public long? consecutivo { get; set; }
        public string? tag_codigo { get; set; }
        public string? observacion { get; set; }

        public List<FndLiqSeguimientoRevisionesTagAplicarRevisionRequest>
            revisiones
        {
            get;
            set;
        } = new();
    }

    public class FndLiqSeguimientoRevisionesTagSeleccionRequest
    {
        public string? cedula { get; set; }
        public long? consecutivo { get; set; }
        public int? id_error { get; set; }
        public bool? seleccionado { get; set; }
        public long? linea_err { get; set; }
    }
}