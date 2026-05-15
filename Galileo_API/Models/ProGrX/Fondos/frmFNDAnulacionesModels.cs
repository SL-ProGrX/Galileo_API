namespace Galileo.Models.ProGrX.Fondos
{
    public class FndAnulacionesDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string planx { get; set; } = string.Empty;
        public string operadorax { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;
        public int cod_contrato { get; set; }
        public int cod_operadora { get; set; }
        public string estado { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public int cuentamaestra { get; set; }
        public int tipo_CDP { get; set; }
        public bool permite_mov_cajas { get; set; }
    }

    public class FndAnulacionesParams
    {
        public int operadora { get; set; }
        public string plan { get; set; } = string.Empty;
        public int contrato { get; set; }
        public string? cedula { get; set; }
        public decimal? autoriza_monto { get; set; }
        public decimal? aporte { get; set; }
        public string? usuario { get; set; }
        public bool? aporteLocked { get; set; }
    }

    public class FndAnulacionesSubCuentasDto
    {
        public int idx { get; set; } 
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal aportes { get; set; }
        public decimal rendimiento { get; set; } 
    }

    public class FndAutorizaDto
    {
        public int autorizado { get; set; }
        public decimal monto { get; set; }
    }

    public class FndAnulacionesEstadoGestionDto
    {
        public int gestion_id { get; set; }
        public string gestion_estado { get; set; } = string.Empty;
    }
}
