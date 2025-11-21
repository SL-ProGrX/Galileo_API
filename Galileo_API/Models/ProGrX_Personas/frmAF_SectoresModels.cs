namespace Galileo.Models.ProGrX_Personas
{
    public class SectoresData
    {
        public int Cod_Sector { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }

    public class SectoresLista
    {
        public int Total { get; set; }
        public List<SectoresData> Lista { get; set; } = new List<SectoresData>();
    }
}