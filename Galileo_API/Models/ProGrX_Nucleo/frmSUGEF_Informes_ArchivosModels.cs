namespace Galileo.Models.ProGrX_Nucleo
{
    public class SugefInformesArchivosDataLista
    {
        public List<SugefInformesArchivosData> lista { get; set; } = new List<SugefInformesArchivosData>();
        public int total { get; set; }
    }

    public class SugefInformesArchivosData
    {
        public DateTime corte { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public int genera_base { get; set; }
        public DateTime genera_fecha { get; set; }
        public string genera_usuario { get; set; } = string.Empty;
        public int archivo_genera { get; set; }
        public DateTime archivo_fecha { get; set; }
        public string archivo_usuario { get; set; } = string.Empty;
        public DateTime rango_inicio { get; set; }
        public DateTime rango_corte { get; set; }
    }

    public class SugefFacilidadesCrediticiasData
    {
        public int id { get; set; }
        public string accion { get; set; } = string.Empty;
        public string numerdoidentificacion { get; set; } = string.Empty;
        public int tipoidentificacion { get; set; }
        public string nombrecliente { get; set; } = string.Empty;
        public string primerapellidocliente { get; set; } = string.Empty;
        public string segundoapellidocliente { get; set; } = string.Empty;
        public string nombreempresa { get; set; } = string.Empty;
        public int tiporeporte { get; set; }
        public int tipooperacion { get; set; }
        public int tipomovimiento { get; set; }
        public int tipoingreso { get; set; }
        public int tiposalida { get; set; }
        public int tipomonedamovimiento { get; set; }
        public decimal montomovimiento { get; set; }
        public DateTime fechatransaccion { get; set; }
        public string motivotransaccion { get; set; } = string.Empty;
        public string origenrecursos { get; set; } = string.Empty;
        public string motivocredito { get; set; } = string.Empty;

    }

    public class Filtros
    {
        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int? pagina { get; set; } = 1;//pagina de la tabla
        public int? paginacion { get; set; } = 30; //paginacion de la tabla
        public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena
    }
}