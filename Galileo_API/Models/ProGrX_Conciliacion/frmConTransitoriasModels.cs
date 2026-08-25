namespace Galileo_API.Models.ProGrX_Conciliacion
{
    public sealed class ConTransitoriasInicializaData
    {
        public DateTime? fecha_servidor { get; set; }
        public List<ConTransitoriasOrigenData> origenes { get; set; } = [];
    }

    public sealed class ConTransitoriasOrigenData
    {
        public string codigo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public sealed class ConTransitoriasConsultaRequest
    {
        public string origen { get; set; } = string.Empty;
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
    }

    public sealed class ConTransitoriasData
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string concepto { get; set; } = string.Empty;
        public string num_liq { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_contrato { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cuenta { get; set; } = string.Empty;
        public string cuenta_desc { get; set; } = string.Empty;
        public DateTime? fecha_registro { get; set; }
        public string tesoreria_id { get; set; } = string.Empty;
        public DateTime? fecha_liquida { get; set; }
    }
}
