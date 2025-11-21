namespace Galileo.Models.Security
{
    public class ResultadoConsultaDto
    {
        public Dictionary<string, string> Columnas { get; set; } = new Dictionary<string, string>();

        public List<Dictionary<string, string>> Datos { get; set; } = new List<Dictionary<string, string>>();
    }
}
