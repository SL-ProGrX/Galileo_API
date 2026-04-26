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
        public string? EstadoFiltro { get; set; } = string.Empty; 
    }

    public class CajasRoeConsultaResult
    {
        public int Id_Roe { get; set; }
        public string? TipoRoe { get; set; }
        public string? Cedula_Aso { get; set; }
        public string? Identificacion_Depo { get; set; }
        public string? Nombre_Depo { get; set; }
        public DateTime? Fecha { get; set; }
        public string? Usuario { get; set; }
        public decimal? Monto_Local { get; set; }
        public decimal? Monto_Dol { get; set; }
        public decimal? Tipo_Cambio { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Actualiza_Fecha { get; set; }
        public string? Actualiza_Usuario { get; set; }
        public string? Usuario_Anulacion { get; set; }
        public DateTime? Fecha_Anulacion { get; set; }
        public string? Observ_Anulacion { get; set; }
        public DateTime? Imprime_Fecha { get; set; }
        public string? Imprime_Usuario { get; set; }
        public string? Id_Sesion { get; set; }
        public string? Estado { get; set; }
    }

    public class CajasRoeImprimeValidaResult
    {
        public int? Imprime { get; set; }
    }

    public class CajasRoeImprimeParams
    {
        public int? Roe { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class CajasRoeImprimeResult
    {
        public int? Pass { get; set; }
        public string? Mensaje { get; set; }
    }
}
