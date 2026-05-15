namespace Galileo.Models.ProGrX.Fondos
{
    public class SeguridadMovimientoPlanDto
    {
        public int? cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public string? descripcion { get; set; }
        public string? registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public bool? seleccionado { get; set; }
    }

    public class SeguridadMovimientoUsuarioDto
    {
        public required string usuario { get; set; }
        public required string descripcion { get; set; }
        public required string registro_usuario { get; set; }
        public DateTime? registro_fecha { get; set; }
        public bool seleccionado { get; set; }
    }
    
    public class SeguridadMovimientoNivelDto
    {
        public string cod_grupo_aprtanul { get; set; } = "";
        public string descripcion { get; set; } = "";
        public required decimal aporte_autorizado { get; set; }
        public required decimal anulacion_autorizado { get; set; }
        public required bool activo { get; set; }
    }
}