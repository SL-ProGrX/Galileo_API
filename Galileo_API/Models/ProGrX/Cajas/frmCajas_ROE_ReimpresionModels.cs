namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasRoeConsultaParams
    {
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public int? IdSesion { get; set; }
        public string? CedulaAso { get; set; }
        public string? IdentificacionDepo { get; set; }
        public string? NombreDepo { get; set; }
        public string? EstadoFiltro { get; set; } = string.Empty; // "Activo", "Inactivo", "PendienteActu", "PendienteImprimir"
    }

    public class CajasRoeConsultaResult
    {
        public int ID_ROE { get; set; }
        public string? TIPOROE { get; set; }
        public string? CEDULA_ASO { get; set; }
        public string? IDENTIFICACION_DEPO { get; set; }
        public string? NOMBRE_DEPO { get; set; }
        public DateTime? FECHA { get; set; }
        public string? USUARIO { get; set; }
        public decimal? MONTO_LOCAL { get; set; }
        public decimal? MONTO_DOL { get; set; }
        public decimal? TIPO_CAMBIO { get; set; }
        public DateTime? REGISTRO_FECHA { get; set; }
        public string? REGISTRO_USUARIO { get; set; }
        public DateTime? ACTUALIZA_FECHA { get; set; }
        public string? ACTUALIZA_USUARIO { get; set; }
        public string? USUARIO_ANULACION { get; set; }
        public DateTime? FECHA_ANULACION { get; set; }
        public string? OBSERV_ANULACION { get; set; }
        public DateTime? IMPRIME_FECHA { get; set; }
        public string? IMPRIME_USUARIO { get; set; }
        public string? ID_SESION { get; set; }
        public string? ESTADO { get; set; }
    }

    public class CajasRoeImprimeValidaResult
    {
        public int? Imprime { get; set; }
    }

    public class CajasRoeImprimeParams
    {
        public int Roe { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CajasRoeImprimeResult
    {
        public int? Pass { get; set; }
        public string? Mensaje { get; set; }
    }
}
