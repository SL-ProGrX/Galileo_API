namespace Galileo.Models.ProGrX_Personas
{
    public class AfCrMotivosData
    {
        public string cod_motivo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
    }
}