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
}
