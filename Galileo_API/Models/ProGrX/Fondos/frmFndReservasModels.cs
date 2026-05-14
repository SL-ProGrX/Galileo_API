namespace Galileo.Models.ProGrX.Fondos
{
    public class FndReservasDto
    {
        public string cod_reserva { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_tra { get; set; } = string.Empty;
        public required bool activa { get; set; }
        public string registro_usuario { get; set; } = string.Empty;
        public DateTime? registro_fecha { get; set; }
        public string cta_reserva { get; set; } = string.Empty;
        public string cta_reserva_desc { get; set; } = string.Empty;
        public string cta_transitoria { get; set; } = string.Empty;
        public string cta_transitoria_desc { get; set; } = string.Empty;
    }

    public class FndReservaCuentaDto
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cod_cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FndReservaCorteFiltros
    {
        public string cod_reserva { get; set; } = string.Empty;
        public DateTime fecha_inicio { get; set; }
        public DateTime fecha_corte { get; set; }
        public string tipo { get; set; } = "R";
    }

    public class FndReservaContenidoDto
    {
        public required int linea_id { get; set; }
        public string? cod_operadora { get; set; }
        public string? cod_plan { get; set; }
        public string? descripcion { get; set; }
        public required decimal porcentaje { get; set; }
        public required bool patrimonio { get; set; } 
    }

    public class FndReservaCorteDto
    {
        public string corte { get; set; } = string.Empty;
        public string cod_reserva { get; set; } = string.Empty;
        public string cod_plan { get; set; } = string.Empty;    
        public string plan_desc { get; set; } = string.Empty;
        public string cod_operadora { get; set; } = string.Empty;
        public decimal base_ { get; set; }    
        public decimal porcentaje { get; set; }
        public decimal monto_reserva { get; set; }
        public decimal saldo_contable { get; set; }    
        public decimal pendiente { get; set; }  
    }
}