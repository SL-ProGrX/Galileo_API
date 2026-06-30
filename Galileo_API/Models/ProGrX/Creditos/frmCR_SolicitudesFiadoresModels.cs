namespace Galileo_API.Models.ProGrX.Creditos
{
    public class CrSolicitudesFiadoresLista
    {
        public int total { get; set; }
        public List<CrSolicitudesFiadoresData> lista { get; set; } = new();
    }

    public class CrSolicitudesFiadoresData
    {
        public long fia_consec { get; set; }
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;

        public string cedulaf { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public string calidad { get; set; } = string.Empty;
        public string calidad_desc { get; set; } = string.Empty;

        public decimal salario { get; set; }
        public decimal devengado { get; set; }
        public decimal liquidez { get; set; }

        public int interno { get; set; }

        public int cod_institucion { get; set; }
        public string institucion_desc { get; set; } = string.Empty;

        public string estado { get; set; } = string.Empty;
        public string firma { get; set; } = string.Empty;
        public string estadoactual { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }
        public string registro_usuario { get; set; } = string.Empty;

        public DateTime? actualiza_fecha { get; set; }
        public string actualiza_usuario { get; set; } = string.Empty;
    }

    public class CrSolicitudesFiadoresDetalleDto
    {
        public long fia_consec { get; set; }
        public long id_solicitud { get; set; }
        public string cedulaf { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre1 { get; set; } = string.Empty;
        public string nombre2 { get; set; } = string.Empty;

        public string calidad { get; set; } = string.Empty;
        public string calidad_desc { get; set; } = string.Empty;

        public decimal salario { get; set; }
        public decimal devengado { get; set; }
        public decimal liquidez { get; set; }

        public int interno { get; set; }

        public int cod_institucion { get; set; }
        public string institucion_desc { get; set; } = string.Empty;

        public DateTime? registro_fecha { get; set; }
        public DateTime? actualiza_fecha { get; set; }
    }

    public class CrSolicitudesFiadoresSocioDto
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre1 { get; set; } = string.Empty;
        public string nombre2 { get; set; } = string.Empty;

        public int cod_institucion { get; set; }
        public string institucion_desc { get; set; } = string.Empty;
        public bool bloquea_institucion { get; set; }
    }

    public class CrSolicitudesFiadoresGuardarRequest
    {
        public long? id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula_deudor { get; set; } = string.Empty;

        public string cedulaf { get; set; } = string.Empty;

        public string apellido1 { get; set; } = string.Empty;
        public string apellido2 { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;

        public string calidad { get; set; } = string.Empty;
        public int? cod_institucion { get; set; }

        public int interno { get; set; } = 1;

        public decimal? salario { get; set; }
        public decimal? devengado { get; set; }
        public decimal? liquidez { get; set; }

        public string usuario { get; set; } = string.Empty;
        public string maquina { get; set; } = string.Empty;
        public string version { get; set; } = string.Empty;
    }

    public class CrSolicitudesFiadoresEliminarRequest
    {
        public long? fia_consec { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    public class CrSolicitudesFiadoresInstitucionDto
    {
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;

        public string item { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CrSolicitudesFiadoresOperacionContextDto
    {
        public long id_solicitud { get; set; }
        public string codigo { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
    }
}