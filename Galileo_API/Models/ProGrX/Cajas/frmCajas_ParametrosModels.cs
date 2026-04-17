namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasParametrosData
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string? tipo { get; set; }
        public string? visible { get; set; }
        public string? notas { get; set; }
        public DateTime? inicio_fecha { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }
}
