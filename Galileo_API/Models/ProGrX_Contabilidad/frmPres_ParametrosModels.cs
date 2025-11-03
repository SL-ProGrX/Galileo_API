namespace PgxAPI.Models.PRE
{
    public class PresParametrosDTO
    {
        public string cod_parametro { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string valor { get; set; } = string.Empty;
        public string? notas { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime registro_fecha { get; set; }
        public Nullable<DateTime> modifica_fecha { get; set; }
        public string? modifica_usuario { get; set; }
    }
}
