namespace Galileo.Models.ProGrX_Activos_Fijos
{
    public class ActivosPersonasLista
    {
        public int total { get; set; }
        public List<ActivosPersonasData> lista { get; set; } = new();
    }

    public class ActivosPersonasData
    {
        public string identificacion { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_departamento { get; set; } = string.Empty;
        public string cod_seccion { get; set; } = string.Empty;
        public string cod_alterno { get; set; } = string.Empty;
        public bool activo { get; set; } = true;

        public string? departamento { get; set; }
        public string? seccion { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
        public string? cod_traslado { get; set; }
    }

    public class CambioDeptoRequest
    {
        public string identificacion { get; set; } = string.Empty;
        public string cod_departamento { get; set; } = string.Empty;
        public string cod_seccion { get; set; } = string.Empty;
        public DateTime? fecha { get; set; }
    }

    public class CambioDeptoResponse
    {
        public string boleta { get; set; } = string.Empty;
    }
    
    public class ActivosPersonasReporteLoteRequest
    {
        public List<string> Identificaciones { get; set; } = new();
        public string? Usuario { get; set; }
    }
}