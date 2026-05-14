namespace Galileo.Models.ProGrX.Fondos
{
    public class FndSeguridadTasaPreferencialDto
    {
        public required string tp_rol { get; set; }
        public string? descripcion { get; set; }
        public required bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public required string registro_usuario { get; set; }
        public DateTime? modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }

    public class FndSeguridadTasaPreferenciaPlanData
    {
        public required string cod_operadora { get; set; }
        public required string cod_plan { get; set; }
        public string? descripcion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }

    public class FndSeguridadTasaPreferenciaUsuarioData
    {
        public required string nombre { get; set; }
        public string? descripcion { get; set; }
        public DateTime? registro_fecha { get; set; }
        public required string registro_usuario { get; set; }
    }

    public class FndSeguridadTasaPreferencialRolPlanDto
    {
        public required int cod_operadora { get; set; }
        public required string cod_plan { get; set; }
        public required string tp_rol { get; set; }
        public required string usuario { get; set; }
        public required bool asignar { get; set; }
    }

    public class FndSeguridadTasaPreferencialRolAutorizadorDto
    {
        public required string usuario { get; set; }
        public required string tp_rol { get; set; }
        public required string registro_usuario { get; set; }
        public required bool asignar { get; set; }
    }
}