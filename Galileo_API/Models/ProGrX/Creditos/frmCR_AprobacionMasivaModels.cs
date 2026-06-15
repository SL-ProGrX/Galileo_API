namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrAprobacionMasivaConsultaRequest
    {
        public string usuario { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public string codigo { get; set; } = string.Empty;
    }

    public class CrAprobacionMasivaFormalizarRequest
    {
        public string usuario { get; set; } = string.Empty;
        public List<int> operaciones { get; set; } = [];
    }

    public class CrAprobacionMasivaOperacionData
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public DateTime? fecha_solicita { get; set; }
        public decimal monto { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public string garantia_desc { get; set; } = string.Empty;
        public string linea_desc { get; set; } = string.Empty;
    }
}