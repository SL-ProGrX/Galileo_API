namespace PgxAPI.Models.ProGrX_Nucleo
{
    public class SifTarjetasData
    {
        public string? cod_tarjeta { get; set; }
        public string? descripcion { get; set; }
        public bool activa { get; set; }
    }

    public class SifTarjetasLista
    {
        public int total { get; set; }
        public List<SifTarjetasData>? lista { get; set; }
    }

    public class SifEmisoresAsignadosData
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public string? Asignado { get; set; }
    }
}