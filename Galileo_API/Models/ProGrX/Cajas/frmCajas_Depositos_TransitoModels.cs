using Org.BouncyCastle.Ocsp;

namespace Galileo.Models.ProGrX.Cajas
{
    public class CajasDepositosTransitoData
    {
        public int linea { get; set; }
        public decimal monto { get; set; }
        public string usuario { get; set; } = string.Empty;
        public string dp_Numero { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string cod_apertura { get; set; } = string.Empty;
        public string cuenta_id { get; set; } = string.Empty;
        public string cuenta_banco { get; set; } = string.Empty;
        public string cod_caja { get; set; } = string.Empty;
        public string caja_desc { get; set; } = string.Empty;
        public string registro_usuario { get; set; } = string.Empty;
        public int id_banco { get; set; }
    }

    public class FiltrosData
    {
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public required int banco { get; set; }
        public string? numero { get; set; } = string.Empty;
        public required decimal MntInicio { get; set; }
        public required decimal MntCorte { get; set; }
        public string? filtro { get; set; } //filtro del buscar en tablas o buscador
        public int? pagina { get; set; } = 1;//pagina de la tabla
        public int? paginacion { get; set; } = 30; //paginacion de la tabla
        public int? sortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
        public string? sortField { get; set; } //campo por el cual se ordena

    }
    
    public class DepositosCuentasBancarias
    {
        public int id_banco { get; set; }
        public string descripcion { get; set; } = string.Empty;
        public string cta { get; set; } = string.Empty;
        public int idx { get; set; }
        public string itmx { get; set; } = string.Empty;

    }
}
