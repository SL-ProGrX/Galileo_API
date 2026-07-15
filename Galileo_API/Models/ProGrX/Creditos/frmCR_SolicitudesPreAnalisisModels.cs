namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSolicitudesPreAnalisisPantallaData
    {
        public List<CrSolicitudesPreAnalisisComiteDto> comites { get; set; } = new();
    }

    public class CrSolicitudesPreAnalisisComiteDto
    {
        public int id_comite { get; set; } = 0;
        public string descripcion { get; set; } = string.Empty;
        public int acta { get; set; } = 0;
    }

    public class CrSolicitudesPreAnalisisConsultaRequest
    {
        public int? solicitud_desde { get; set; }
        public int? solicitud_hasta { get; set; }
    }

    public class CrSolicitudesPreAnalisisConsultaData
    {
        public List<CrSolicitudesPreAnalisisOperacionDto> lista { get; set; } = new();
    }

    public class CrSolicitudesPreAnalisisOperacionDto
    {
        public int id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public string membresia { get; set; } = string.Empty;
    }

    internal sealed class CrSolicitudesPreAnalisisOperacionQueryDto
    {
        public int id_solicitud { get; set; } = 0;
        public string cedula { get; set; } = string.Empty;
        public string categoria { get; set; } = string.Empty;
        public DateTime? fecha_ingreso { get; set; }
        public string estadoactual { get; set; } = string.Empty;
    }
}