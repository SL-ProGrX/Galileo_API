namespace PgxAPI.Models.AH
{
    public class AutorizadorePatrimonioDto
    {
        public int id_usuario { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
        public string estado { get; set; } = string.Empty;
    }
}
