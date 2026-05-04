namespace Galileo.Models.ProGrX.Cobros
{
    public class CoAdvertenciasTiposLista
    {
        public List<CoAdvertenciasTiposData> lista { get; set; } = new();
        public int total { get; set; } = 0;
    }

    public class CoAdvertenciasTiposData
    {
        public string cod_advertencia { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public required bool afecta_clasificacion { get; set; }
        public required bool activo { get; set; }
        public bool isNew { get; set; } = false;
    }
}