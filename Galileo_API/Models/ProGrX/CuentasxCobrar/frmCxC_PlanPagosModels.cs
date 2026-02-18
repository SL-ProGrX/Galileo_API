namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCPlanPagosOperacionData
    {
        public int operacion { get; set; }
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public string cod_concepto { get; set; } = "";
        public string descripcion { get; set; } = "";
        public decimal monto { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public decimal cuota { get; set; } = 0;
        public decimal tasao { get; set; } = 0;
        public string tipo_plazo { get; set; } = "";
        public int plazo { get; set; } = 0;
        public string oficinax { get; set; } = "";
        public string? contrato { get; set; }
        public string? pagador { get; set; }
        public string? num_documento { get; set; }
        public DateTime? fecha_pago { get; set; }
    }

    public class CxCPlanPagosMovimientoData
    {
        public decimal linea { get; set; }
        public DateTime? fecha_inicio { get; set; }
        public DateTime? fecha_corte { get; set; }
        public decimal cargos { get; set; } = 0;
        public decimal int_cor { get; set; } = 0;
        public decimal int_mor { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal saldo_inicial { get; set; } = 0;
        public decimal saldo_final { get; set; } = 0;
        public int dias { get; set; } = 0;
        public string estado { get; set; } = "";
        public int dias_mora { get; set; } = 0;

        public DateTime? registro_fecha { get; set; }
        public decimal mov_monto { get; set; } = 0;
        public decimal mov_cargos { get; set; } = 0;
        public decimal mov_int_cor { get; set; } = 0;
        public decimal mov_int_mor { get; set; } = 0;
        public decimal mov_principal { get; set; } = 0;

        public string caja_usuario { get; set; } = ""; 
        public string tipo_documento { get; set; } = "";
        public string num_documento { get; set; } = "";
        public string concepto { get; set; } = "";
    }

    public class CxCPlanPagosOperacionResumenData
    {
        public decimal lineas { get; set; } = 0;
        public decimal intereses { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public int dias { get; set; } = 0;
        public DateTime? inicio { get; set; }
        public DateTime? corte { get; set; }
        public int moradias { get; set; } = 0;
    }

    public class CxCPlanPagosCargosMovData
    {
        public int id_cargo { get; set; }
        public int operacion { get; set; }
        public string? cod_cargo { get; set; }
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public string? notas { get; set; }
        public string? cod_cuenta { get; set; }
        public DateTime? registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? cod_unidad { get; set; }
        public string? cod_centro_costo { get; set; }
        public string? cod_divisa { get; set; }
        public decimal abono { get; set; }
        public decimal linea { get; set; }
    }
}
