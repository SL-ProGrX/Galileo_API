namespace Galileo.Models.ProGrX.Clientes
{
    public class AfEstadosDto
    {
        public string cod_estado { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public bool activo { get; set; }
        public bool deduce_creditos { get; set; }
        public bool deduce_patrimonio { get; set; }
        public bool deduce_ahorros { get; set; }
    }

    public class AfEstadosLista
    {
        public int total { get; set; }
        public List<AfEstadosDto> lista { get; set; } = new List<AfEstadosDto>();
    }

    public class AfEstadosMovimientosDto
    {
        public string? cod_movimiento { get; set; }
        public string? cod_estado { get; set; }
        public string? cod_estado_cambio { get; set; }
        public string? usuario { get; set; }
        public DateTime fecha { get; set; }
        public string? estadoInicial { get; set; }
        public string? estadoFinal { get; set; }
    }

    public class AfEstadosEntidadesDto
    {
        public string? cod_estado { get; set; }
        public string? cod_institucion { get; set; }
        public string? descripcion { get; set; }
        public string? desc_corta { get; set; }
        public bool check { get; set; }
    }
}