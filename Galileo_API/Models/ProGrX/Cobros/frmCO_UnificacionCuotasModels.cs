namespace Galileo.Models.ProGrX.Cobros
{
    public class CoUnificacionCuotasData
    {
        public int id_solicitud { get; set; } = 0;
        public string codigo { get; set; } = "";
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";

        public int cuota { get; set; } = 0;

        public decimal intc { get; set; } = 0m;
        public decimal intm { get; set; } = 0m;
        public decimal amortiza { get; set; } = 0m;
        public decimal cargos { get; set; } = 0m;
        public decimal iva { get; set; } = 0m;
        public decimal saldo { get; set; } = 0m;

        public string estado { get; set; } = "";
        public string estado_desc { get; set; } = "";

        public string fecap { get; set; } = "";
        public string fecult { get; set; } = "";
        public string fecha_corte { get; set; } = "";
    }

    public class CoUnificacionCuotasListaResult
    {
        public int total { get; set; } = 0;
        public List<CoUnificacionCuotasData> lista { get; set; } = new();
    }

    public class CoUnificacionCuotasUnificarRequest
    {
        public string codigo { get; set; } = "";
        public List<int> ids_solicitud { get; set; } = new();
        public string usuario_sesion { get; set; } = "";
    }

    public class CoUnificacionCuotasUnificarResponse
    {
        public int total_procesadas { get; set; } = 0;
    }
}