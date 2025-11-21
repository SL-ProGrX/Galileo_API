namespace Galileo.Models.ProGrX_Nucleo
{
    public class SifEmisoresLista
    {
        public int total { get; set; }
        public List<SifEmisoresData> lista { get; set; } = new List<SifEmisoresData>();
    }

    public class SifEmisoresData
    {
        public string? cod_emisor { get; set; }
        public string? descripcion { get; set; }
        public bool activo { get; set; }
    }

    public class SifTarjetasAsignadasData
    {
        public string? Codigo { get; set; }
        public string? Descripcion { get; set; }
        public string? Asignado { get; set; }
    }

    public class SifEmisorTarjetaData
    {
        public string? cod_emisor { get; set; }
        public string? cod_tarjeta { get; set; }
    }
}