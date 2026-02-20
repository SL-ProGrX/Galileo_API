namespace Galileo_API.Models.ProGrX_Polizas
{
    public class CrdPolizasRegionDto
    {
        public int cod_region { get; set; }
        public decimal monto_comercial { get; set; }
        public decimal monto_personal { get; set; }

        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }

        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
    }

    public class CrdPolizasRegionGuardarDto
    {
        public string cod_poliza { get; set; } = string.Empty;
        public int? cod_region { get; set; }            // null => insertar
        public decimal? monto_comercial { get; set; }
        public decimal? monto_personal { get; set; }
    }

    public class  CrdPolizasRegionCantonDto
    {
        public bool asignado { get; set; } = false;               // 1 asignado, 0 no
        public string canton { get; set; } = string.Empty;
        public string ncanton { get; set; } = string.Empty;

        public int provincia { get; set; } = 0;
        public string nprovincia { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }

    public enum CrdCantonesModo
    {
        todos = 0,
        solo_asignados = 1,
        no_asignados = 2
    }

    public class CrdPolizasRegionAsignarCantonDto
    {
        public bool asigna { get; set; } = false;
        public string? cod_poliza { get; set; }
        public int cod_region { get; set; } = 0;
        public int provincia { get; set; } = 0;
        public string? canton { get; set; } = string.Empty;
    }
}
