namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasSaldosFavorTiposLista
    {
        public int total { get; set; }
        public List<CajasSaldosFavorTiposData> lista { get; set; } = new List<CajasSaldosFavorTiposData>();
    }
    public class CajasSaldosFavorTiposData
    {
        public string doc_tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; } = false;
        public bool isNew { get; set; } = false;
    }
    public class CajasSaldosFavorUsuarioLiquidaLista
    {
        public int total { get; set; }
        public List<CajasSaldosFavorUsuarioLiquidData> lista { get; set; } = new List<CajasSaldosFavorUsuarioLiquidData>();
    }
    public class CajasSaldosFavorUsuarioLiquidData
    {
        public string usuario { get; set; } = string.Empty;
        public string doc_tipo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool envia_fondo { get; set; }
        public bool envia_tesoreria { get; set; }
        public bool ret_efectivo { get; set; }
        public bool excluye_saldo_favor { get; set; }
    }
    public class FiltroData
    {
        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int? pagina { get; set; } = 1;//pagina de la tabla
        public int? paginacion { get; set; } = 30; //paginacion de la tabla
        public object? parametros { get; set; } //adicional para enviar JSON con filtros adicionales
        public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena
        public object? filters { get; set; } //filtros de encabezados
    }
}