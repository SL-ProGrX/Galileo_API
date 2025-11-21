namespace Galileo.Models.ProGrX.Bancos
{
    public class TesUbicacionesLista
    {
        public int total { get; set; }
        public List<TesUbicacionesData> lista { get; set; } = new List<TesUbicacionesData>();
    }

    public class TesUbicacionesData
    {
        public string cod_ubicacion { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public string usuario { get; set; } = string.Empty;
        public bool isNew { get; set; } = false;
    }
}