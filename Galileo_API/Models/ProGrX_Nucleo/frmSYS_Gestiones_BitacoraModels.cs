namespace Galileo.Models.ProGrX_Nucleo
{
    public class SysGestionesBitacorasData
    {
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_Usuario { get; set; }
        public string? descripcion { get; set; }
        public string? notas { get; set; }
    }

    public class SysGestionesBitacorasLista
    {
        public int total { get; set; }
        public List<SysGestionesBitacorasData>? lista { get; set; }
    }

    public class SociosLookupData
    {
        public string? CEDULA { get; set; }
        public string? CEDULAR { get; set; }
        public string? NOMBRE { get; set; }
    }

    public class SociosLookupLista
    {
        public int total { get; set; }
        public List<SociosLookupData>? lista { get; set; }
    }

    public class SysGestionesBitacoraFiltro
    {
        public string? ClienteBuscar { get; set; }
        public string? GestionCod { get; set; }
        public string? UsuarioBuscar { get; set; }
        public string? FechaInicio { get; set; }
        public string? FechaFin { get; set; }
        public required bool TodasFechas { get; set; }
        public FiltrosLazyLoadData? Filtros { get; set; }
    }
}