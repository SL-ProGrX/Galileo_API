namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxCCuentasAbonosData
    {
        public int operacion { get; set; }
        public decimal? saldo { get; set; }
        public string proceso { get; set; } = string.Empty;
        public decimal? tasa_corriente { get; set; }
        public decimal? interesc { get; set; }
        public decimal? amortiza { get; set; }
        public int? fecha_ultmov { get; set; }
        public decimal? cuota { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public int? meses { get; set; }
        public string nombre { get; set; } = string.Empty;
        public DateTime? activa_fecha { get; set; }
        public string autoriza_usuario { get; set; } = string.Empty;
        public string conceptodesc { get; set; } = string.Empty;
        public string oficinadesc { get; set; } = string.Empty;
        public DateTime? fechaserver { get; set; }
        public int? caja_valida_concepto { get; set; }
        public int? facturas { get; set; }
    }

    public class CxCCuotasActivasData
    {
        public int linea { get; set; }
        public DateTime fecha_corte { get; set; }
        public decimal monto { get; set; }
        public string estado_desc { get; set; } = string.Empty;
        public decimal int_cor { get; set; }
        public decimal int_mor { get; set; }
        public decimal principal { get; set; }
        public decimal cargos { get; set; }
        public int dias { get; set; }
        public int dias_mora { get; set; }
    }

    public class CxCOperacionesActivasData
    {
        public int operacion { get; set; }
        public string cod_concepto { get; set; } = string.Empty;
        public string cedula { get; set; } = string.Empty;
        public string nombre { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    public class CxCCuentaCuotasInfoData
    {
        public long seqX { get; set; } = 0;
        public decimal intCor { get; set; } = 0;
        public decimal principal { get; set; } = 0;
        public decimal saldo { get; set; } = 0;
        public DateTime? fecha_Proceso { get; set; }
        public decimal cuota { get; set; } = 0;
    }

    public class CxCCuentasRegistrarAbonoRequest
    {
        public string mcaja { get; set; } = "";
        public int mapertura { get; set; } = 0;
        public int msesionid { get; set; } = 0;
        public string mtiquete { get; set; } = "";
        public int operacionid { get; set; } = 0;

        public string tipodoc { get; set; } = "";

        public DateTime? fechacancelacion { get; set; }

        public bool fechacancelacion_enabled { get; set; } = false;

        public decimal totalcajas { get; set; } = 0;
        public decimal totalcancela { get; set; } = 0;
        public decimal diferencia { get; set; } = 0;
        public decimal datosamortiza { get; set; } = 0;
        public decimal totalpagar { get; set; } = 0;

        public int diasactivo { get; set; } = 0;
        public bool recalculacuota { get; set; } = false;

        public AbonoTipo tipoabono { get; set; } = AbonoTipo.Ordinario;
        public bool diferenciaaplenabled { get; set; } = false;
        public string diferenciaapltexto { get; set; } = "";

        public string usuario { get; set; } = "";
        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public string codigo { get; set; } = "";
        public string descripcion { get; set; } = "";
        public string notas { get; set; } = "";
        public decimal saldo_anterior { get; set; } = 0;
        public decimal saldo_nuevo { get; set; } = 0;
        
        public bool recibo_digital { get; set; } = false;
    }

    public enum AbonoTipo
    {
        Ordinario = 0,
        Extraordinario = 1,
        Cancelacion = 2,
        AdelantoCuotas = 3
    }
}
