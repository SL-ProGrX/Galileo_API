namespace Galileo.Models.SIF
{
    public class BitacoraDto
    {
        public int cliente { get; set; }
        public DateTime? fechainicio { get; set; }
        public DateTime? fechacorte { get; set; }
        public string? usuario { get; set; }
        public int? modulo { get; set; }
        public string? movimiento { get; set; }
        public string? detalle { get; set; }
        public string? appname { get; set; }
        public string? appversion { get; set; }
        public string? logequipo { get; set; }
        public string? logip { get; set; }
        public string? equipomac { get; set; }
        public bool todasFechas { get; set; }
        public bool todasHoras { get; set; }
    }

    public class BitacoraResultadoDto
    {
        public string modulodesc { get; set; } = string.Empty;
        public string usuarionombre { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string movimiento { get; set; } = string.Empty;
        public DateTime fecha_format { get; set; }
        public string detalle { get; set; } = string.Empty;
        public string app_nombre { get; set; } = string.Empty;
        public string app_version { get; set; } = string.Empty;
        public string app_equipo { get; set; } = string.Empty;
        public string equipo_mac { get; set; } = string.Empty;
        public string app_ip { get; set; } = string.Empty;


    }

    public class SifBitacoraLista
    {
        public int total { get; set; }
        public List<BitacoraResultadoDto>? lista { get; set; }
    }

    public class BitacoraModuloDto
    {
        public int Modulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Keyent { get; set; } = string.Empty;
    }
}