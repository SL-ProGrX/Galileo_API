namespace Galileo_API.Models.ProGrX.Cajas
{
    public sealed class CajasCrdAbonosCtPData
    {
        public long id_solicitud { get; set; }

        public decimal saldo { get; set; }

        public decimal Saldo_mes { get; set; }

        public string proceso { get; set; } = string.Empty;

        public string divisa { get; set; } = "COL";

        public decimal? interesv { get; set; }

        // Use @ to escape the reserved keyword 'int'
        public int @int { get; set; }

        public int plazo { get; set; }

        public decimal interesc { get; set; }

        public decimal amortiza { get; set; }

        public decimal? fecult { get; set; }  

        public long prideduc { get; set; }

        public int? iva_aplica { get; set; }

        public int? opex { get; set; }

        public decimal cuota { get; set; }

        public string codigo { get; set; } = string.Empty;

        public string cedula { get; set; } = string.Empty;

        public int? cuotas_planilla { get; set; }

        public int? cuotas_directas { get; set; }

        public int? meses { get; set; }

        public DateTime? fechaforp { get; set; }

        public string base_Calculo { get; set; } = string.Empty;

        // socios
        public string nombre { get; set; } = string.Empty;

        // catalogo
        public string descripcion { get; set; } = string.Empty;

        public string retencion { get; set; } = "N"; // 'S' / 'N'

        public string poliza { get; set; } = "N";    // 'S' / 'N'

        public decimal? porc_cargo_cancelacion { get; set; }

        public int? anticipo_meses { get; set; }

        public int? diasActivo { get; set; }

        public int? autPagoAnt { get; set; } 

        public string lineaDesc { get; set; } = string.Empty;

        public string oficinaDesc { get; set; } = string.Empty;

        public string recursoDesc { get; set; } = string.Empty;

        public DateTime? fechaServer { get; set; } 

        public int? caja_Valida_Concepto { get; set; }

        public int? control { get; set; }

        public int? iva_porc { get; set; }
    }

    public class CajasCrdOperacionTransacData
    {
        public decimal id_seq { get; set; }
        public int num_cuota { get; set; }
        public int fecha_proceso { get; set; }
        public DateTime fecha_pago { get; set; }
        public decimal cuota { get; set; }
        public int mora_dias { get; set; }
        public decimal intcor { get; set; }
        public decimal intmor { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public decimal poliza { get; set; }
        public decimal iva { get; set; }
        public int dias_calculo { get; set; }
        public DateTime fecha_corte { get; set; }
    }
}
