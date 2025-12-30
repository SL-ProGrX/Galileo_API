namespace Galileo.Models.ProGrX.Fondos
{
    public class FndContratosBuscarParams
    {
        public string cod_plan_origen { get; set; } = string.Empty;
        public string cod_plan_destino { get; set; } = string.Empty;
        public int cod_operadora { get; set; }
        public bool aplicar_todos { get; set; }
        public bool solo_renueva { get; set; }
        public string? estado_socio { get; set; }
    }

    public class FndRenuevaContratosDto
    {
        public bool aplicar { get; set; }     
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public bool existe { get; set; }       
        public int cod_contrato { get; set; }
    }

    public class FndRenuevaContratosRequest
    {
        public int? cod_operadora { get; set; } 
        public string cod_plan_origen { get; set; } = string.Empty;
        public string cod_plan_destino { get; set; } = string.Empty;
        public int? plazo { get; set; }
        public DateTime? fecha_vence { get; set; }
        public string usuario { get; set; } = string.Empty;
        public List<FndRenuevaContratosDto> contratos { get; set; } = new List<FndRenuevaContratosDto>();
    }
}