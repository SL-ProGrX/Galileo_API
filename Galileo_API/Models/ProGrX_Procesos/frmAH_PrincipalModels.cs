namespace Galileo.Models.ProGrX_Procesos
{
    public class FrmAhPrincipalConsultaResponse
    {
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public decimal obrero { get; set; }
        public decimal patronal { get; set; }
        public decimal custodia { get; set; }
        public decimal capitaliza { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public decimal total { get; set; }
        public decimal aporte_cobro { get; set; }
        public DateTime? fec_ahorro { get; set; }
        public DateTime? fec_aporte { get; set; }
        public DateTime? fec_custodia { get; set; }
        public DateTime? fec_capitaliza { get; set; }
    }

    public class FrmAhPrincipalDetallePatrimonioRequest
    {
        public string cedula { get; set; } = string.Empty;
        public bool incluir_obrero { get; set; } = true;
        public bool incluir_patronal { get; set; } = true;
        public bool incluir_capitalizacion { get; set; } = true;
        public bool incluir_custodia { get; set; } = true;
    }

    public class FrmAhPrincipalDetallePatrimonioResponse
    {
        public DateTime? fecha { get; set; }
        public string fecha_proceso { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public string tipo_desc { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string movimiento { get; set; } = string.Empty;
        public string mov_numero { get; set; } = string.Empty;
        public string mov_concepto { get; set; } = string.Empty;
        public string mov_usuario { get; set; } = string.Empty;
        public bool resaltar_rojo { get; set; }
        public string tcon { get; set; } = string.Empty;
    }

    public class FrmAhPrincipalExcedentesResponse
    {
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public decimal excedente_bruto { get; set; }
        public decimal capitalizado { get; set; }
        public decimal renta { get; set; }
        public decimal excedente_final { get; set; }
    }

    public class FrmAhPrincipalHistoricoResponse
    {
        public int anio { get; set; }
        public int mes { get; set; }
        public string cod_divisa { get; set; } = string.Empty;
        public decimal ahorro { get; set; }
        public decimal aporte { get; set; }
        public decimal custodia { get; set; }
        public decimal capitaliza { get; set; }
        public string estado_desc { get; set; } = string.Empty;
    }

    public class FrmAhPrincipalLiquidacionesResponse
    {
        public decimal consec { get; set; }
        public DateTime? fec_liq { get; set; }
        public decimal ahorro_liq { get; set; }
        public decimal aporte_liq { get; set; }
        public decimal capitalizado_liq { get; set; }
        public decimal extra_liq { get; set; }
        public string usuario { get; set; } = string.Empty;
    }
}
