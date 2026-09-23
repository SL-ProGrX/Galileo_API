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
        /// <summary>Nota de la solicitud de autorizacion (@Nota de spFnd_Gestion_Registro).</summary>
        public string? nota { get; set; }
        /// <summary>Gestion de autorizacion vigente (VB6 txtGestionId), se envia a spFondos_Anula_Aporte.</summary>
        public int? gestion_id { get; set; }
        /// <summary>Monto a anular por subcuenta (VB6 vGrid col. 4) cuando el contrato es cuenta maestra.</summary>
        public List<FndAnulacionesSubCuentaMontoDto>? subcuentas { get; set; }
    }

    public class FndAnulacionesSubCuentaMontoDto
    {
        public int idx { get; set; }
        public decimal anulacion { get; set; }
    }

    /// <summary>Resultado de spFondos_Anula_Aporte.</summary>
    public class FndAnulacionesAplicaResultDto
    {
        public int Pass { get; set; }
        public string? NumDoc { get; set; }
        public string? Mensaje { get; set; }
        public string? Movimiento { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Proceso { get; set; }
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
