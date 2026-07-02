namespace Galileo_API.Models.ProGrX_Pasivos
{
    public class FrmCrApaMovimientosAcreedorDto
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public decimal saldo { get; set; } = 0;
    }

    public class FrmCrApaMovimientosOperacionDto
    {
        public string estado_desc { get; set; } = string.Empty;
        public string cod_divisa { get; set; } = string.Empty;
        public decimal monto { get; set; } = 0;
        public int plazo { get; set; } = 0;
        public decimal tasa { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public DateTime? fecha_formaliza { get; set; }
        public DateTime? fecha_primer_pago { get; set; }
        public DateTime? fecha_prox_pago { get; set; }
        public string dia_de_pago { get; set; } = string.Empty;
        public decimal mov_amortiza { get; set; } = 0;
        public decimal mov_intereses { get; set; } = 0;
        public decimal mov_comision { get; set; } = 0;
        public decimal mov_cargos { get; set; } = 0;
    }

    public class FrmCrApaMovimientosDetalleDto
    {
        public string fecha { get; set; } = string.Empty;
        public string tipo_movimiento { get; set; } = string.Empty;
        public string documento { get; set; } = string.Empty;
        public decimal amortiza { get; set; } = 0;
        public decimal intereses { get; set; } = 0;
        public decimal comision { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal total { get; set; } = 0;
        public string usuario { get; set; } = string.Empty;
        public string notas { get; set; } = string.Empty;
    }

    public class FrmCrApaMovimientosCuentaDto
    {
        public string cod_cuenta { get; set; } = string.Empty;
        public string cuenta_mask { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class FrmCrApaMovimientosNavegarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string direccion { get; set; } = string.Empty;
        public bool solo_con_saldo { get; set; } = false;
    }

    public class FrmCrApaMovimientosNavegarDto
    {
        public string operacion { get; set; } = string.Empty;
    }

    public class FrmCrApaMovimientosAplicarRequest
    {
        public string cod_acreedor { get; set; } = string.Empty;
        public string operacion { get; set; } = string.Empty;
        public string usuario { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        public decimal amortiza { get; set; } = 0;
        public decimal intereses { get; set; } = 0;
        public decimal comision { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public string notas { get; set; } = string.Empty;
        public string cuenta { get; set; } = string.Empty;
        public string doc_ref { get; set; } = string.Empty;
    }

    public class FrmCrApaMovimientosAplicarResultadoDto
    {
        public string cod_transaccion { get; set; } = string.Empty;
        public string tipo_documento { get; set; } = string.Empty;
        public string mensaje { get; set; } = string.Empty;
        public string? reporte_resultado { get; set; }
    }

    public class FrmCrApaMovimientosOperacionBusquedaDto
    {
        public string operacion { get; set; } = string.Empty;
        public string cod_acreedor { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public decimal saldo { get; set; }
        public DateTime? fecha_formaliza { get; set; }
    }
}