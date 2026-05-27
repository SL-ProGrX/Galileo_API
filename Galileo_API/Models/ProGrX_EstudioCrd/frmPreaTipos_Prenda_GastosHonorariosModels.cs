namespace Galileo_API.Models.ProGrX_EstudioCrd
{
    public class CrPreaTiposPrendaGastosHonorariosListaResult
    {
        public int total { get; set; }
        public List<CrPreaTiposPrendaGastosHonorariosListaData> lista { get; set; } = new();
    }

    public class CrPreaTiposPrendaGastosHonorariosListaData
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

    public class CrPreaTiposPrendaGastosHonorariosGuardarRequest
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

    public class CrPreaTiposPrendaGastosHonorariosSpResultDto
    {
        public short Pass { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string Movimiento { get; set; } = string.Empty;
        public int IdLlave { get; set; }
    }
}
