namespace Galileo_API.Models.ProGrX.CuentasxCobrar
{
    public class CxcOperacionAnulacionData
    {
        public int operacion { get; set; }
        public decimal saldo { get; set; }
        public string? num_documento { get; set; }
        public decimal? tasa_corriente { get; set; }
        public int? dias_plazo { get; set; }
        public decimal? interesc { get; set; }
        public decimal? amortiza { get; set; }
        public DateTime? fecha_ultmov { get; set; }
        public string? cod_concepto { get; set; }
        public string? cedula { get; set; }
        public string? nombre { get; set; }
        public string? descripcion { get; set; }
        public DateTime? activa_fecha { get; set; }
        public string? tipo_plazo { get; set; }
        public string? proceso { get; set; }
    }

    public class CxcOperacionMovimientoData
    {
        public int linea { get; set; }
        public string? estado { get; set; }
        public int? dias { get; set; }
        public int? dias_mora { get; set; }
        public decimal mov_intcor { get; set; }
        public decimal mov_intmor { get; set; }
        public decimal mov_principal { get; set; }
        public decimal mov_cargos { get; set; }
    }

    public class CxcAbonoAnularParams
    {
        public int operacion { get; set; } = 0;
        public string usuario { get; set; } = "";
        public bool generar_recibo { get; set; } = true;

        public string cedula { get; set; } = "";
        public string nombre { get; set; } = "";
        public string cod_concepto { get; set; } = "CRD008";
        public string cod_concepto_operacion { get; set; } = "";
        public string observacion { get; set; } = string.Empty;

        public decimal intcor { get; set; } = 0;
        public decimal intmor { get; set; } = 0;
        public decimal amortizacion { get; set; } = 0;
        public decimal cargos { get; set; } = 0;
        public decimal total { get; set; } = 0;

        public string deposito { get; set; } = "";
        public string detalle { get; set; } = "";
    }

    public class CxCOperacionCtasData
    {
        public int operacion { get; set; } = 0;

        public decimal saldo { get; set; } = 0;

        public string cod_divisa { get; set; } = "";

        public string cod_unidad { get; set; } = "";

        public string cod_centro_costo { get; set; } = "";

        public string ctaintc { get; set; } = "";

        public string ctaintm { get; set; } = "";

        public string ctacargos { get; set; } = "";

        public string ctaamortiza { get; set; } = "";

        public string cod_concepto { get; set; } = "";
    }

    public class SifTransaccionInsertParams
    {
        public int cod_transaccion { get; set; } = 0;

        public string tipo_documento { get; set; } = "";

        public string registro_usuario { get; set; } = "";

        public string cliente_identificacion { get; set; } = "";

        public string cliente_nombre { get; set; } = "";

        public string cod_concepto { get; set; } = "";

        public decimal monto { get; set; } = 0;

        public string estado { get; set; } = "P";

        public string referencia_01 { get; set; } = "";

        public string referencia_02 { get; set; } = "";

        public string referencia_03 { get; set; } = "";

        public string cod_oficina { get; set; } = "";

        public string linea1 { get; set; } = "";
        public string linea2 { get; set; } = "";
        public string linea3 { get; set; } = "";
        public string linea4 { get; set; } = "";
        public string linea5 { get; set; } = "";
        public string linea6 { get; set; } = "";
        public string linea7 { get; set; } = "";
        public string linea8 { get; set; } = "";
        public string linea9 { get; set; } = "";
        public string linea10 { get; set; } = "";
        public string linea11 { get; set; } = "";

        public string detalle { get; set; } = "";
    }

    public class SifDocsAsientoParams
    {
        public string tipodoc { get; set; } = "";
        public string numdoc { get; set; } = "";
        public decimal monto { get; set; }
        public string dc { get; set; } = "";
        public string cod_divisa { get; set; } = "";
        public int tipo_cambio { get; set; } = 1;
        public int enlace { get; set; } = 0;
        public string cod_unidad { get; set; } = "";
        public string cod_centro_costo { get; set; } = "";
        public string cuenta { get; set; } = "";
        public int operacion { get; set; } = 0;
        public string cod_concepto { get; set; } = "";
        public string deposito { get; set; } = "";
    }

    public class CxcRegistrarAsientoRequest
    {
        public int CodEmpresa { get; set; }
        public string Usuario { get; set; } = string.Empty;
        public int Documento { get; set; }
        public decimal Monto { get; set; }
        public string Dc { get; set; } = string.Empty;
        public string TipoDoc { get; set; } = string.Empty;
        public int Operacion { get; set; }
        public string CodDivisa { get; set; } = string.Empty;
        public string CodUnidad { get; set; } = string.Empty;
        public string CodCentroCosto { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string CodConcepto { get; set; } = string.Empty;
        public string Deposito { get; set; } = string.Empty;
    }

    sealed class AsientoItem
    {
        public decimal Monto { get; init; }
        public string? Cuenta { get; init; }
        public string Dc { get; init; } = string.Empty;
    }
}
