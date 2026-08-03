namespace Galileo_API.Models.ProGrX.Cajas
{
    public class CajasSaldoFavorTipoLiquidacionParams
    {
        public string? Usuario { get; set; } = string.Empty;
        public string? TipoDoc { get; set; } = string.Empty;
    }

    public class CajasSaldoFavorTipoLiquidacionResult
    {
        public string? Tipo { get; set; } = string.Empty;
    }

    public class CajasSaldosFavorConsultaParams
    {
        public int? CodEmpresa { get; set; }
        public bool? SaldoMayorCero { get; set; } // true: >0, false: <=0, null: no filtra
        public bool? FiltrarFechas { get; set; } // true: filtra por fechas, false/null: no filtra
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? DocTipo { get; set; }
        public string? DocNumero { get; set; }
        public string? Usuario { get; set; }
        public string? CodEntidadPago { get; set; }
        public string? CodOrigenRecursos { get; set; }
        public bool? SoloConOrigenRecursos { get; set; } // true: agrega IS NOT NULL
        public decimal? MontoInicio { get; set; }
        public decimal? MontoFin { get; set; }
    }

    public class CajasSaldosFavorConsultaResult
    {
        public int? Linea { get; set; }
        public string? Cedula { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; } = string.Empty;
        public decimal? Monto_Registro { get; set; }
        public decimal? Monto { get; set; }
        public decimal? Saldo { get; set; }
        public string? Doc_Tipo { get; set; } = string.Empty;
        public string? Doc_Numero { get; set; } = string.Empty;
        public int? Doc_Transac_Id { get; set; }
        public string? Cod_Caja { get; set; } = string.Empty;
        public string? Cod_Apertura { get; set; } = string.Empty;
        public DateTime? Liq_Fecha { get; set; }
        public string? Liq_Usuario { get; set; } = string.Empty;
        public decimal? Liq_Monto { get; set; }
        public int? Liq_Nsolicitud { get; set; }
        public string? Cod_Divisa { get; set; } = string.Empty;
        public decimal? Saldo_Divisa_Real { get; set; }
        public decimal? Monto_Divisa_Real { get; set; }
        public decimal? Tipo_Cambio { get; set; }
        public string? Cuenta_Bancaria { get; set; } = string.Empty;
        public string? Cod_Cuenta { get; set; } = string.Empty;
        public string? Liq_Tipo_Doc { get; set; } = string.Empty;
        public string? Liq_Num_Doc { get; set; } = string.Empty;
        public string? Liq_Plan { get; set; } = string.Empty;
        public int? Liq_Contrato { get; set; }
        public int? Id_Banco { get; set; }
        public string? Notas { get; set; } = string.Empty;
        public string? Cod_Entidad_Pago { get; set; } = string.Empty;
        public string? Cod_Origen_Recursos { get; set; } = string.Empty;
        public string? Valida_Requiere { get; set; } = string.Empty;
        public DateTime? Valida_Fecha { get; set; }
        public string? Valida_Usuario { get; set; } = string.Empty;
        public string? Valida_Estados { get; set; } = string.Empty;
        public string? Valida_Notas { get; set; } = string.Empty;
        public string? Nombre { get; set; } = string.Empty;
        public string? EntidadPagoDesc { get; set; } = string.Empty;
        public string? OrigenRecursosDesc { get; set; } = string.Empty;
        public string? BancoDesc { get; set; } = string.Empty;
        public string? Registro_Fecha_Format { get; set; } = string.Empty;
        public string? Autoriza_Estado_Desc { get; set; } = string.Empty;
    }

    public class CajasDepositosCuentaBancariaAutParams
    {
        public string? FormaPago { get; set; } = string.Empty;
    }

    public class CajasDepositosCuentaBancariaAutResult
    {
        public int? Id_Banco { get; set; }
        public string? Descripcion { get; set; } = string.Empty;
        public string? Cta { get; set; } = string.Empty;
        public int? IdX { get; set; }
        public string? ItmX { get; set; } = string.Empty;
    }

    public class CajasDepositosTramiteIdentificaParams
    {
        public int? CodEmpresa { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string? Documento { get; set; }
        public int? IdBanco { get; set; }
        public decimal? MontoInicio { get; set; }
        public decimal? MontoFin { get; set; }
    }

    public class CajasDepositosTramiteIdentificaResult
    {
        public int? DP_Tramite_Id { get; set; }
        public int? Id_Banco { get; set; }
        public string? Documento { get; set; } = string.Empty;
        public int? NSolicitud { get; set; }
        public DateTime Fecha { get; set; }
        public decimal? Monto { get; set; }
        public string? Descripcion { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string? Registro_Usuario { get; set; } = string.Empty;
        public int? Id_Requerida { get; set; }
        public int? Identificado { get; set; }
        public DateTime? Identifica_Fecha { get; set; }
        public string? Identifica_Usuario { get; set; } = string.Empty;
        public int? Cliente_Id { get; set; }
        public string? Cliente_Nombre { get; set; } = string.Empty;
        public string? Cod_Cuenta { get; set; } = string.Empty;
        public int? Tes_Aplicado { get; set; }
        public DateTime? Tes_Aplicado_Fecha { get; set; }
        public string? Tes_Aplicado_Usuario { get; set; } = string.Empty;
        public int? Id_Saldo_Favor { get; set; }
        public string? Cod_Transaccion { get; set; } = string.Empty;
        public string? Tipo_Documento { get; set; } = string.Empty;
        public int? Carga_Lote { get; set; }
        public string? Cod_Origen_Recursos { get; set; } = string.Empty;
        public string? Cod_Entidad_Pago { get; set; } = string.Empty;
        public string? Id_Servicio { get; set; }
        public string? Cuenta_Iban_Origen { get; set; } = string.Empty;
        public string? Cedula_Origen { get; set; } = string.Empty;
        public string? Nombre_Origen { get; set; } = string.Empty;
        public string? Telefono_Origen { get; set; } = string.Empty;
        public string? Banco_Origen { get; set; } = string.Empty;
        public string? BancoDesc { get; set; } = string.Empty;
    }

    public class CajasFormasPagoTipoResult
    {
        public string? Tipo { get; set; } = string.Empty;
    }

    public class CajasDepositosCargadoParams
    {
        public int? CodEmpresa { get; set; }
        public int? IdBanco { get; set; }
        public string? Documento { get; set; } = string.Empty;
        public decimal? Monto { get; set; }
    }

    public class CajasDepositosCargadoResult
    {
        public int? Existe { get; set; }
    }

    public class CajasDepositosTramiteInsertParams
    {
        public int? CodEmpresa { get; set; }
        public int? IdBanco { get; set; }
        public string? Documento { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public decimal? Monto { get; set; }
        public string? Descripcion { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
        public int? IdRequerida { get; set; }
        public string? CodCuenta { get; set; } = string.Empty;
    }

    public class CajasIdentificaTesDepositosParams
    {
        public int? CodEmpresa { get; set; }
        public int? IdBanco { get; set; }
        public string? Documento { get; set; } = string.Empty;
        public string? Cedula { get; set; } = string.Empty;
        public string? Nombre { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
    }

    public class CajasDepositosTramiteInconsistenciaInsertParams
    {
        public int? CodEmpresa { get; set; }
        public int? IdBanco { get; set; }
        public string? Documento { get; set; } = string.Empty;
        public DateTime? Fecha { get; set; }
        public decimal? Monto { get; set; }
        public string? Descripcion { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
        public string? Inconsistencia { get; set; } = string.Empty;
    }

    public class CajasSaldoFavorCargaParams
    {
        public int? CodEmpresa { get; set; }
        public string? CodFormaPago { get; set; } = string.Empty;
        public string? Documento { get; set; } = string.Empty;
        public string? Cedula { get; set; } = string.Empty;
        public string? Nombre { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
    }

    public class CajasIdentificaTesDepositosFullParams
    {
        public int? CodEmpresa { get; set; }
        public int? IdBanco { get; set; }
        public string? Documento { get; set; } = string.Empty;
        public string? Cedula { get; set; } = string.Empty;
        public string? Nombre { get; set; } = string.Empty;
        public string? Usuario { get; set; } = string.Empty;
        public string? CodEntidadPago { get; set; } = string.Empty;
        public string? CodOrigenRecursos { get; set; } = string.Empty;
        public int? DepositoId { get; set; }
    }

    public class CajasNotificaDepositosParams
    {
        public int? CodEmpresa { get; set; }
        public int? IdSaldoFavor { get; set; }
    }

    public class CajasSaldoFavorLiquidacionParams
    {
        public int? CodEmpresa { get; set; }
        public string? Metodo { get; set; } = string.Empty; // "T", "F", "E", "C"
        public int? Linea { get; set; }
        public string? Usuario { get; set; } = string.Empty;
    }

    public class CajasCargaSaldosFavorRetencionRequest
    {
        public List<long> DepositoIds { get; set; } = [];
        public string RetencionCodigo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class CajasTransacSFLiqTipoSaldoResult
    {
        public string? item { get; set; } = string.Empty;
        public string? descripcion { get; set; } = string.Empty;
    }

    public class CajasTransacSFLiqLiquidarParams
    {
        public int? CodEmpresa { get; set; }
        public int Linea { get; set; } = 0;
        public string Metodo { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Caja { get; set; } = string.Empty;
        public int Apertura { get; set; } = 0;
    }

    public class CajasTransacSFLiqLiquidarResult
    {
        public long NumDoc { get; set; } = 0;
        public string TipoDoc { get; set; } = string.Empty;
    }
}
