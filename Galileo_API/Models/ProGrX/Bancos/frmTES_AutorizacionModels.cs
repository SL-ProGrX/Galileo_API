namespace PgxAPI.Models.TES
{
    public class TesAutorizacionData
    {
        public string? nombre { get; set; }
        public string? notas { get; set; }
        public string? clave { get; set; }
        public string? estado { get; set; }
        public decimal? rango_gen_inicio { get; set; }
        public decimal? rango_gen_corte { get; set; }
        public decimal? firmas_gen_inicio { get; set; }
        public decimal? firmas_gen_corte { get; set; }
    }

    public class TesFirmasAutData
    {
        public decimal? firmas_autoriza_inicio { get; set; }
        public decimal? firmas_autoriza_corte { get; set; }
    }

    public class TesAutorizacionFiltros
    {
        public int id_banco { get; set; }
        public string tipo_doc { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public bool duplicados { get; set; }
        public bool todas_fechas { get; set; }
        public DateTime fecha_inicio{ get; set; }
        public DateTime fecha_corte { get; set; }
        public bool todas_solicitudes { get; set; }
        public int? solicitud_inicio { get; set; }
        public int? solicitud_corte { get; set; }
        public bool casos_bloqueados { get; set; }
        public string? tipo_cuenta { get; set; } = string.Empty;
        public bool mismo_banco { get; set; }
        public int tipo_autorizacion { get; set; }
        public decimal monto_inicio { get; set; }
        public decimal monto_fin { get; set; }
        public string token { get; set; } = string.Empty;
        public string detalle { get; set; } = string.Empty;
        public string appid { get; set; } = string.Empty;
    }

    public class TesSolicitudesLista
    {
        public int total { get; set; }
        public List<TesSolicitudesData>? solicitudes { get; set; } = new List<TesSolicitudesData>();
    }

    public class TesSolicitudesData
    {
        public int nsolicitud { get; set; }
        public string? codigo { get; set; } = string.Empty;
        public string? beneficiario { get; set; } = string.Empty;
        public decimal? monto { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public string? cta_ahorros { get; set; } = string.Empty;
        public bool? duplicado { get; set; }
        public string? cta_verifica { get; set; } = string.Empty;
        public string? detalle { get; set; } = string.Empty;
        public string? appid { get; set; } = string.Empty;
        public bool bloqueo { get; set; }
        public string? estadoactual { get; set; } = string.Empty;
    }

    public class TesAutorizaParametros
    {
        public int codEmpresa { get; set; }
        public string? clave { get; set; }
        public string? usuario { get; set; }
        public int tipo_autorizacion { get; set; }
        public bool? estadoSinpe { get; set; }
        public string? tipoGiroSinpe { get; set; }
        public string? tipoDocumento { get; set; }
        public string? solicitudesLista { get; set; }
    }
}