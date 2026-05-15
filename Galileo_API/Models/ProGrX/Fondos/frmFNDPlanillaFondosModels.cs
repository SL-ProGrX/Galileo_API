namespace Galileo.Models.ProGrX.Fondos
{
    public class CargarDeduccionesRequest
    {
        public int cod_institucion { get; set; }
        public int cod_operadora { get; set; }
        public string? plan { get; set; }
        public int proceso { get; set; }
        public string? comprobante { get; set; }
        public List<FndPlanillaFondosArchivoData> registros { get; set; } = new();
    }

    public class FndPlanillaFondosArchivoData
    {
        public required string cedula { get; set; }
        public required string nombre { get; set; }
        public decimal fondos { get; set; }
    }

    public class FndPlanillaFondosDetalleData
    {
        public required string cedula { get; set; }
        public required string nombre { get; set; }
        public bool existe_persona { get; set; }
        public bool existe_contrato { get; set; }
        public int? cod_contrato { get; set; }
        public decimal fondos { get; set; }
    }

    public class FndPlanillaFondosData
    {
        public List<FndPlanillaFondosDetalleData> detalles { get; set; } = new();
        public int total_socios { get; set; }
        public int total_contratos { get; set; }
        public int total_casos { get; set; }
        public decimal monto_total { get; set; }
    }

    public class FndPlanillaDirectaProcesaDto
    {
        public int cod_institucion { get; set; }
        public int cod_operadora { get; set; }
        public string plan { get; set; } = string.Empty;
        public int proceso { get; set; }
        public string comprobante { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
    }
}