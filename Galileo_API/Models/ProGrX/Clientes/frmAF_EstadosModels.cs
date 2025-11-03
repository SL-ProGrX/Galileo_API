using PgxAPI.Models.ProGrX.Bancos;

namespace PgxAPI.Models.ProGrX.Clientes
{
    public class AF_EstadosDTO
    {
        public string cod_estado { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool deduce_creditos { get; set; }
        public bool deduce_patrimonio { get; set; }
        public bool deduce_ahorros { get; set; }
    }

    public class AF_EstadosLista
    {
        public int total { get; set; }
        public List<AF_EstadosDTO> lista { get; set; } = new List<AF_EstadosDTO>();
    }

    public class AF_Estados_MovimientosDTO
    {
        public string cod_movimiento { get; set; }
        public string cod_estado { get; set; }
        public string cod_estado_cambio { get; set; }
        public string usuario { get; set; }
        public DateTime fecha { get; set; }
        public string estadoInicial { get; set; }
        public string estadoFinal { get; set; }
    }

    public class AF_Estados_EntidadesDTO
    {
        public string cod_estado { get; set; }
        public string cod_institucion { get; set; }
        public string descripcion { get; set; }
        public string desc_corta { get; set; }
        public bool check { get; set; }
    }
}
