namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSeguimientoRegTagOperacionDto
    {
        public long id_solicitud { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public decimal montosol { get; set; }
        public decimal cuota { get; set; }
        public int plazo { get; set; }
        public decimal tasa { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime? fechasol { get; set; }
        public string oficina { get; set; } = string.Empty;
    }

    public class CrSeguimientoRegTagConsultaRequest
    {
        public string tag_codigo { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_fin { get; set; }
        public string estado { get; set; } = "Todos";
    }

    public class CrSeguimientoRegTagAplicarRequest
    {
        public string tag_codigo { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string observacion { get; set; } = string.Empty;
        public List<CrSeguimientoRegTagOperacionAplicarDto> operaciones { get; set; } = new();
    }

    public class CrSeguimientoRegTagOperacionAplicarDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
    }
}
