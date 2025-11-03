namespace PgxAPI.Models.SYS
{

    public class AutorizadoresEXPDTO
    {
        public int autorizador_id { get; set; }
        public string aut_usuario { get; set; } = string.Empty;
        public string aut_clave { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;


    }


}
