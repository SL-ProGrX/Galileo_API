namespace PgxAPI.Models
{
    public class ResultadoConsultaDTO
    {
        public Dictionary<string, string> Columnas { get; set; } = new Dictionary<string, string>();

        public List<Dictionary<string, string>> Datos { get; set; } = new List<Dictionary<string, string>>();
    }

}
