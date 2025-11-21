namespace Galileo.Models.GEN
{
    public class AutorizaSolicitudesCreditoData
    {
        public int id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto_girado { get; set; }
    }

    public class AutorizaSolicitudesFondosData
    {
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public decimal total_girar { get; set; }
    }

    public class AutorizaSolicitudesLiquidacionData
    {
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal tneto { get; set; }
        public string tipo { get; set; } = string.Empty;
    }

    public class AutorizaSolicitudesBeneficiosData
    {
        public int consec { get; set; }
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string cod_beneficio { get; set; } = string.Empty;
    }

    public class AutorizaSolicitudesHipotecarioData
    {
        public int codigodesembolso { get; set; }
        public int numerooperacion { get; set; }
        public string beneficiario { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public DateTime registrofecha { get; set; }
        public string registrousuario { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string codigo { get; set; } = string.Empty;
        public string tes_supervision_fecha { get; set; } = string.Empty;

    }
}