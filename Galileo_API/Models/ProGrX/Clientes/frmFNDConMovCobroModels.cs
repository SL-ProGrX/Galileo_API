namespace Galileo.Models.ProGrX.Clientes
{

    public class FndConMovCobroRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string CodPlan { get; set; } = "T"; // "T" para todos, o el código específico
    }

    public class FndConMovCobroResult
    {
        public string? Codigo { get; set; }
        public int Id_Solicitud { get; set; }
        public decimal Principal { get; set; }
        public DateTime Fecha { get; set; }
        public string? Proceso { get; set; }
        public string? InstitucionX { get; set; }
        public string? Nombre { get; set; }
        public string? CTANAMORT { get; set; }
        public string? CTAOAMORT { get; set; }
        public string? Cedula { get; set; }
        public string? Cod_Plan { get; set; }
        public int Cod_Operadora { get; set; }
        public int Cod_Contrato { get; set; }
        public string? Estado { get; set; }
        public string? Tcon { get; set; }
        public string? Ncon { get; set; }
    }

    public class FndAcreditaMovCbrPendienteRequest
    {
        public required int Operacion { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public required short Accion { get; set; } 
        public string TipoDoc { get; set; } = string.Empty;
        public string NumDoc { get; set; } = string.Empty;
        public required decimal Monto { get; set; }
    }

    public class FndConMovCobroResumenRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string CodPlan { get; set; } = string.Empty;
    }

    public class FndConMovCobroResumenResult
    {
        public decimal Monto { get; set; }
        public int Casos { get; set; }
    }
}