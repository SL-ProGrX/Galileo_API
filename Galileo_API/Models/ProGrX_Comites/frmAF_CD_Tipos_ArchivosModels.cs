namespace Galileo_API.Models.ProGrX_Comites
{
    public class TipoArchivoData
    {
        public int id_tipo_archivo { get; set; }
        public string nombre_tipo_archivo { get; set; } = string.Empty;
        public bool activo { get; set; }
        public string usuario { get; set; } = string.Empty;
        public bool isNew { get; set; }
    }

    public class TipoArchivoLista
    {
        public int total { get; set; }
        public List<TipoArchivoData> lista { get; set; } = new();
    }

}
