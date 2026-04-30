namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class CrPreaConfigListaResult
    {
        public int total { get; set; }
        public List<CrPreaConfigListaData> lista { get; set; } = new();
    }
    public class CrPreaConfigListaData
    {
        public int id { get; set; }
        public decimal monto_min { get; set; }
        public decimal monto_max { get; set; }
        public decimal gastos { get; set; }
        public decimal honorarios { get; set; }
        public decimal impuesto { get; set; }
        public decimal total { get; set; }
        public string rango_edad { get; set; } = string.Empty;
        public short edad_min { get; set; }
        public short edad_max { get; set; }
        public string descripcion_examenes { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string modifica_usuario { get; set; } = string.Empty;
        public DateTime? modifica_fecha { get; set; }
        public bool isNew { get; set; }
    }
    public class CrPreaConfigGuardarRequest
    {
        public required int id { get; set; }
        public required decimal monto_min { get; set; }
        public required decimal monto_max { get; set; }
        public required decimal gastos { get; set; }
        public required decimal honorarios { get; set; }
        public required decimal impuesto { get; set; }
        public string rango_edad { get; set; } = string.Empty;
        public required short edad_min { get; set; }
        public required short edad_max { get; set; }
        public string descripcion_examenes { get; set; } = string.Empty;

        public string estado { get; set; } = string.Empty;
    }
    public class CrPreaAvaluoCfiaDto
    {
        public decimal valor_formula_crd_hip { get; set; }
        public decimal valor_formula_aseccss { get; set; }
        public decimal valor_porc_iva { get; set; }
        public decimal monto_honorarios_min_iva { get; set; }
        public DateTime? fec_registro { get; set; }
        public string usuario_registro { get; set; } = string.Empty;
        public DateTime? fecha_modifica { get; set; }
        public string usuario_modifica { get; set; } = string.Empty;
    }
    public class CrPreaAvaluoCfiaGuardarRequest
    {
        public required decimal valor_formula_crd_hip { get; set; }
        public required decimal valor_formula_aseccss { get; set; }
        public required decimal valor_porc_iva { get; set; }
        public required decimal monto_honorarios_min_iva { get; set; }
    }
    public class CrPreaSpResultDto
    {
        public short Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
        public int IdLlave { get; set; }
    }
}