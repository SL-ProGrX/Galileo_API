using static PgxAPI.Models.ProGrX.Cajas.CajasDesglocePagoRequest;

namespace PgxAPI.Models.ProGrX.Cajas
{
    public class CajasDisponibleFondosDto
    {
        public decimal monto { get; set; }
        public decimal disponible { get; set; }
        public string? divisa_desc { get; set; }
    }

    public class CajasSaldoFavorDto
    {
        public string? clienteid { get; set; }
        public int? referencia { get; set; }
        public string? referenciatexto { get; set; }

    }

    public class CajasDivisaFuncionalDto
    {
        public string? divisa { get; set; }
    }

    public class CajasDepositosCuentasCancariasDto
    {
        public string? idx { get; set; }
        public string? itmx { get; set; }
        public string? cta { get; set; }
    }

    public sealed class CajasDesglocePago
    {
        public int? linea { get; set; }
        public string? cod_caja { get; set; }
        public int? cod_apertura { get; set; }
        public string? ticket { get; set; }
        public string? cod_forma_pago { get; set; }
        public decimal? monto { get; set; }
        public string? usuario { get; set; }
        public DateTime? fecha { get; set; }
        public string? formapagodesc { get; set; }
        public string? tipo { get; set; }
    }

    public class CajasDesglocePagoDto
    {
        public int linea { get; set; }
        public string? ticket { get; set; }
        public string? cod_caja { get; set; }
        public int cod_apertura { get; set; }
        public decimal monto { get; set; }
        public string? cod_divisa { get; set; }
        public decimal tipo_cambio { get; set; }
        public DateTime registro_fecha { get; set; }
        public string? registro_usuario { get; set; }
        public string? cod_tarjeta { get; set; }
        public string? tarjeta_numero { get; set; }
        public string? tarjeta_autorizacion { get; set; }
        public string? cheque_emisor { get; set; }
        public string? cheque_numero { get; set; }
        public string? cuenta_bancaria { get; set; }
        public string? num_referencia { get; set; }
        public string? cod_cuenta { get; set; }
        public int aplica_saldo_favor { get; set; }
        public decimal saldo_favor { get; set; }
        public int saldo_favor_id { get; set; }
        public string? observaciones { get; set; }
        public string? cod_forma_pago { get; set; }
        public int dp_banco { get; set; }
        public DateTime? dp_fecha { get; set; }
        public string? cod_plan { get; set; }
        public int? cod_contrato { get; set; }
        public string? cod_entidad_pago { get; set; }
        public string? cod_origen_recursos { get; set; }
    }

    public class DistribuyeSaldoFavorDto
    {
        public string cod_caja { get; set; }
        public int cod_apertura { get; set; }
        public string ticket { get; set; }
        public string usuario { get; set; }
        public decimal total_aplicar { get; set; }
        public string divisa { get; set; }
    }

    public class CajasDesglocePagoRequest
    {
        public string? cod_caja { get; set; }
        public int cod_apertura { get; set; }
        public string? ticket { get; set; }
        public string? usuario { get; set; }
        public string? cod_forma_pago { get; set; }
        public string? cuenta_bancaria { get; set; }
        public string? num_referencia { get; set; }
        public decimal monto { get; set; }
        public string? cod_divisa { get; set; }
        public decimal tipo_cambio { get; set; }
        public string? notas { get; set; }
        public int? saldo_favor_id { get; set; }
        public string? cod_tarjeta { get; set; }
        public string? tarjeta_numero { get; set; }
        public string? tarjeta_autorizacion { get; set; }
        public string? cheque_emisor { get; set; }
        public string? cheque_numero { get; set; }
        public DateTime? dp_fecha { get; set; }
        public int? dp_banco { get; set; }
        public string? cod_plan { get; set; }
        public int? cod_contrato { get; set; }

        public class CajasSaldoaFavorDto
        {
            public int linea { get; set; }
            public string? doc_numero { get; set; }
            public string? doc_tipo { get; set; }
            public decimal monto { get; set; }
            public decimal saldo { get; set; }
            public string? cod_divisa { get; set; }
            public decimal tipocambio { get; set; }
            public string? divisa { get; set; }
            public string? divisa_desc { get; set; }
            public string? idx { get; set; }
            public string? itmx { get; set; }
        }
    }

    public class CajasCatalogosDto
    {
        public List<DropDownListaGenericaModel> Divisas { get; set; } = new();
        public List<DropDownListaGenericaModel> Emisores { get; set; } = new();
        public List<DropDownListaGenericaModel> Tarjetas { get; set; } = new();
        public List<DropDownListaGenericaModel> Pagadores { get; set; } = new();
        public List<DropDownListaGenericaModel> OrigenRecursos { get; set; } = new();
        public List<CajasSaldoaFavorDto> SaldosFavor { get; set; } = new();
        public List<DropDownListaGenericaModel> Fondos { get; set; } = new();
    }

    public class CajasFormaPagoDto
    {
        public string? cod_forma_pago { get; set; }
        public string? descripcion { get; set; }
        public string? tipo { get; set; }
        public string? cod_cuenta { get; set; }
        public int aplica_saldos_favor { get; set; }
        public int or_aplica { get; set; }
    }

    public class CajasTiqueteDto
    {
        public int linea { get; set; }
        public string? forma_pago_desc { get; set; }
        public string? tipo { get; set; }
        public decimal monto { get; set; }
        public decimal saldo_favor { get; set; }
        public string? divisa { get; set; }
        public decimal tipo_cambio { get; set; }
        public string? num_referencia { get; set; }
        public string? cheque_numero { get; set; }
        public string? tarjeta_numero { get; set; }
        public string? cod_plan { get; set; }
        public string? cod_contrato { get; set; }
    }
}