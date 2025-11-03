namespace PgxAPI.Models.CxP
{
    public class BancosAutorizadosDTO
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public bool cheques { get; set; }
        public bool transferencias { get; set; }
    }
}


