namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasRoeAnularLista
    {
        public int total { get; set; }
        public List<CajasRoeAnularData> lista { get; set; } = new List<CajasRoeAnularData>();
    }

    public class CajasRoeAnularData
    {
        public int ID_ROE { get; set; }
        public string estado { get; set; } = string.Empty;
        public string TIPOROE { get; set; } = string.Empty;
        public string CEDULA_ASO { get; set; } = string.Empty;
        public string IDENTIFICACION_DEPO { get; set; } = string.Empty;
        public string NOMBRE_DEPO { get; set; } = string.Empty;
        public DateTime FECHA { get; set; }
        public string USUARIO { get; set; } = string.Empty;
        public decimal MONTO_LOCAL { get; set; }
        public decimal Monto_Dol { get; set; }
        public decimal TIPO_CAMBIO { get; set; }
        public DateTime REGISTRO_FECHA { get; set; }
        public string REGISTRO_USUARIO { get; set; } = string.Empty;
        public DateTime ACTUALIZA_FECHA { get; set; }
        public string ACTUALIZA_USUARIO { get; set; } = string.Empty;
        public DateTime FECHA_ANULACION { get; set; }
        public string USUARIO_ANULACION { get; set; } = string.Empty;
        public string OBSERV_ANULACION { get; set; } = string.Empty;
        public DateTime IMPRIME_FECHA { get; set; }
        public string IMPRIME_USUARIO { get; set; } = string.Empty;
        public int ID_SESION { get; set; }
    }

    public class FiltrosCajasRoeAnularData
    {
        public string? IDENTIFICACION_DEPO { get; set; } = string.Empty;
        public string? NOMBRE_DEPO { get; set; } = string.Empty;
        public required bool rango_fechas { get; set; }
        public required DateTime fecha_desde { get; set; }
        public required DateTime fecha_hasta { get; set; }
        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int? pagina { get; set; } = 1;//pagina de la tabla
        public int? paginacion { get; set; } = 30; //paginacion de la tabla
        public object? parametros { get; set; } //adicional para enviar JSON con filtros adicionales
        public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena
        public object? filters { get; set; } //filtros de encabezados
    }
}
