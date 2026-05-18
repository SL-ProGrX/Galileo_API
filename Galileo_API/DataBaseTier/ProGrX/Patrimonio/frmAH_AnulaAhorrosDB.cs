using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.AH;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.DataBaseTier.ProGrX.Cajas;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Patrimonio
{
    public class FrmAHAnulaAhorrosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _dbBitacora;
        private readonly MRecibos _mRecibos;
        private readonly MProGrxMain _mProGrx;
        private readonly MCajas _mCajas;
        private const int vModulo = 2;

        public FrmAHAnulaAhorrosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _dbBitacora = new MSecurityMainDb(config);
            _mRecibos = new MRecibos(config);
            _mProGrx = new MProGrxMain(config);
            _mCajas = new MCajas(config);
        }

        /// <summary>
        /// Obtiene el consolidado de patrimonio del afiliado.
        /// </summary>
        public ErrorDto<FrmAhAnulaAhorrosConsultaResponse?> Patrimonio_frmAH_AnulaAhorros_Consulta_Obtener(int codEmpresa, string cedula)
        {
            const string sql = @"
select
    rtrim(cedula) as cedula,
    rtrim(nombre) as nombre,
    isnull(obrero,0) as obrero,
    isnull(patronal,0) as patronal,
    isnull(custodia,0) as custodia,
    isnull(capitaliza,0) as capitaliza,
    rtrim((select top 1 COD_DIVISA from vSys_Divisas where DIVISA_LOCAL = 1)) as cod_divisa,
    isnull(obrero,0) + isnull(patronal,0) + isnull(custodia,0) + isnull(capitaliza,0) as total,
    fecAhorro as fec_ahorro,
    fecAporte as fec_aporte,
    fecCustodia as fec_custodia,
    fecCapitaliza as fec_capitaliza
from vPAT_Consolidado
where cedula = @cedula;";

            return DbHelper.ExecuteSingleQuery<FrmAhAnulaAhorrosConsultaResponse>(
                _portalDb,
                codEmpresa,
                sql,
                default,
                new { cedula });
        }

        /// <summary>
        /// Obtiene los movimientos recientes para anulación por movimiento.
        /// </summary>
        public ErrorDto<List<FrmAhAnulaAhorrosMovimientoResponse>> Patrimonio_frmAH_AnulaAhorros_Movimientos_Obtener(int codEmpresa, string cedula, string tipoRubro)
        {
            const string sql = @"
select top 24
    rtrim(tcon) + '-' + rtrim(ncon) as documento_key,
    fecha,
    left(convert(varchar(10), fecha_proceso, 120), 7) as fecha_proceso,
    rtrim(descripcion) as descripcion,
    isnull(monto,0) as monto,
    rtrim(tcon) as tcon,
    rtrim(ncon) as ncon,
    rtrim(cod_concepto) as cod_concepto
from vPAT_Movimientos
where cedula = @cedula
  and tipo = @tipoRubro
  and monto > 0
order by fecha desc;";

            return DbHelper.ExecuteListQuery<FrmAhAnulaAhorrosMovimientoResponse>(
                _portalDb,
                codEmpresa,
                sql,
                new { cedula, tipoRubro });
        }

        /// <summary>
        /// Procesa la anulación de ahorros con generación de documento, asiento, saldo a favor e impresión.
        /// </summary>
        public ErrorDto<FrmAhAnulaAhorrosProcesarResponse> Patrimonio_frmAH_AnulaAhorros_Procesar(int codEmpresa, FrmAhAnulaAhorrosProcesarRequest request)
        {
            var response = new FrmAhAnulaAhorrosProcesarResponse();

            if (request == null || string.IsNullOrWhiteSpace(request.cedula) || string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.CreateErrorResponse("La solicitud es inválida.", -2, response);

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var consulta = conn.QueryFirstOrDefault<FrmAhAnulaAhorrosConsultaResponse>(@"
select
    rtrim(cedula) as cedula,
    rtrim(nombre) as nombre,
    isnull(obrero,0) as obrero,
    isnull(patronal,0) as patronal,
    isnull(custodia,0) as custodia,
    isnull(capitaliza,0) as capitaliza,
    rtrim((select top 1 COD_DIVISA from vSys_Divisas where DIVISA_LOCAL = 1)) as cod_divisa,
    isnull(obrero,0) + isnull(patronal,0) + isnull(custodia,0) + isnull(capitaliza,0) as total,
    fecAhorro as fec_ahorro,
    fecAporte as fec_aporte,
    fecCustodia as fec_custodia,
    fecCapitaliza as fec_capitaliza
from vPAT_Consolidado
where cedula = @cedula;", new { request.cedula }, tx);

                if (consulta == null)
                    return DbHelper.CreateErrorResponse("No se localizó la persona o sus registros de aportes.", -2, response);

                decimal monto = request.tipo_anulacion?.Trim().ToUpper() == "MOV" && request.movimientos.Count > 0
                    ? request.movimientos.Sum(x => x.monto)
                    : request.monto;

                if (monto <= 0)
                    return DbHelper.CreateErrorResponse("El monto debe ser mayor que cero.", -2, response);

                decimal saldoRubro = ObtenerSaldoRubro(consulta, request.tipo_rubro);
                if (monto > saldoRubro)
                    return DbHelper.CreateErrorResponse("El monto de la anulación no puede exceder el saldo del rubro seleccionado.", -2, response);

                var globales = _mProGrx.sbSifParametrosInicializa(codEmpresa, request.usuario).Result;
                if (globales == null)
                    return DbHelper.CreateErrorResponse("No se pudieron obtener los parámetros globales del usuario.", -2, response);

                var cuentaAporte = ConsultarCuentaAporte(conn, tx, request.cedula, request.tipo_rubro);
                if (cuentaAporte == null || string.IsNullOrWhiteSpace(cuentaAporte.Value.cuenta))
                    return DbHelper.CreateErrorResponse("No se pudo resolver la cuenta contable del rubro de patrimonio.", -2, response);

                string cuentaDestino;
                string formaPagoSaldoFavor = string.Empty;

                if ((request.accion ?? "C").Trim().ToUpper() == "S")
                {
                    var saldoFavor = conn.QueryFirstOrDefault<(string cod_forma_pago, string cod_cuenta)>(@"
select top 1
    rtrim(COD_FORMA_PAGO) as cod_forma_pago,
    rtrim(COD_CUENTA) as cod_cuenta
from SIF_FORMAS_PAGO
where TIPO = 'S' and Activa = 1;", transaction: tx);

                    if (string.IsNullOrWhiteSpace(saldoFavor.cod_cuenta))
                        return DbHelper.CreateErrorResponse("No existe una forma de pago activa para saldo a favor.", -2, response);

                    cuentaDestino = saldoFavor.cod_cuenta;
                    formaPagoSaldoFavor = saldoFavor.cod_forma_pago;
                }
                else
                {
                    cuentaDestino = _mRecibos.FxDocumentoCuenta(codEmpresa, "ND").Trim();
                    if (string.IsNullOrWhiteSpace(cuentaDestino))
                        return DbHelper.CreateErrorResponse("No se puede realizar movimiento porque no se especificó una cuenta contable válida para esta operación.", -2, response);
                }

                string tipoDocumento = "ND";
                string numDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, tipoDocumento).ToString();
                decimal tipoCambio = _mCajas.fxCajasTipoCambio(codEmpresa, 0, consulta.cod_divisa);
                decimal factorAplicado = TipoCambioApl(tipoCambio);

                decimal saldoActual = saldoRubro - monto;
                string[] lineas =
                [
                    $"Plan            : {ObtenerTextoRubro(request.tipo_rubro)}",
                    " ",
                    $"Saldo Anterior  : {cuentaAporte.Value.aporte:N2}",
                    $"Monto Anulación : {monto:N2}",
                    $"Saldo Actual    : {saldoActual:N2}",
                    " ",
                    $"Divisa          : {consulta.cod_divisa}",
                    " ",
                    $"Usuario         : {request.usuario}",
                    $"Acción          : {ObtenerTextoAccion(request.accion!)}"
                ];

                conn.Execute(@"
insert into SIF_TRANSACCIONES
(
    COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO,
    Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado,
    Referencia_01, Referencia_02, Referencia_03, cod_oficina,
    linea1, linea2, linea3, linea4, linea5, linea6, linea7, linea8, linea9, linea10,
    detalle, documento, cod_caja
)
values
(
    @cod_transaccion, @tipo_documento, dbo.MyGetdate(), @registro_usuario,
    @cliente_identificacion, @cliente_nombre, 'PAT002', @monto, 'P',
    @referencia_01, '', '', @cod_oficina,
    @linea1, @linea2, @linea3, @linea4, @linea5, @linea6, @linea7, @linea8, @linea9, @linea10,
    @detalle, @documento, @cod_caja
);", new
                {
                    cod_transaccion = numDocumento,
                    tipo_documento = tipoDocumento,
                    registro_usuario = request.usuario,
                    cliente_identificacion = request.cedula,
                    cliente_nombre = string.IsNullOrWhiteSpace(request.nombre) ? consulta.nombre : request.nombre,
                    monto = monto * -1,
                    referencia_01 = request.cedula,
                    cod_oficina = globales.GOficinaTitular,
                    linea1 = lineas[0],
                    linea2 = lineas[1],
                    linea3 = lineas[2],
                    linea4 = lineas[3],
                    linea5 = lineas[4],
                    linea6 = lineas[5],
                    linea7 = lineas[6],
                    linea8 = lineas[7],
                    linea9 = lineas[8],
                    linea10 = lineas[9],
                    detalle = string.IsNullOrWhiteSpace(request.notas) ? "Anulación de ahorro" : request.notas.Trim(),
                    documento = string.Empty,
                    cod_caja = string.Empty
                }, tx);

                EjecutarAsiento(conn, tx, tipoDocumento, numDocumento, monto * factorAplicado, "D", consulta.cod_divisa, tipoCambio, globales.GEnlace, globales.GOficinaUnidad, string.Empty, cuentaAporte.Value.cuenta, $"Pat:{request.tipo_rubro}", request.cedula, string.Empty);
                EjecutarAsiento(conn, tx, tipoDocumento, numDocumento, monto * factorAplicado, "C", consulta.cod_divisa, tipoCambio, globales.GEnlace, globales.GOficinaUnidad, string.Empty, cuentaDestino, $"Pat:{request.tipo_rubro}", request.cedula, string.Empty);

                if ((request.accion ?? "C").Trim().ToUpper() == "S")
                {
                    var sfId = conn.QueryFirstOrDefault<int>(@"
exec spCajas_SaldoFavor_Registra
    @FormaPago,
    @Referencia,
    @Monto,
    @Cedula,
    @Nombre,
    @Usuario,
    @Divisa;", new
                    {
                        FormaPago = formaPagoSaldoFavor,
                        Referencia = $"{tipoDocumento}-{numDocumento}",
                        Monto = monto,
                        Cedula = request.cedula,
                        Nombre = string.IsNullOrWhiteSpace(request.nombre) ? consulta.nombre : request.nombre,
                        Usuario = request.usuario,
                        Divisa = consulta.cod_divisa
                    }, tx);

                    conn.Execute(@"
exec spPAT_Anulacion_Saldo_Favor
    @TipoDoc,
    @NumDoc,
    @Usuario,
    @FormaPago,
    @Divisa,
    @Monto,
    @Unidad,
    @Cuenta,
    @Referencia,
    @SfId;", new
                    {
                        TipoDoc = tipoDocumento,
                        NumDoc = numDocumento,
                        Usuario = request.usuario,
                        FormaPago = formaPagoSaldoFavor,
                        Divisa = consulta.cod_divisa,
                        Monto = monto,
                        Unidad = globales.GOficinaUnidad,
                        Cuenta = cuentaDestino,
                        Referencia = $"{tipoDocumento}-{numDocumento}",
                        SfId = sfId
                    }, tx);
                }

                conn.Execute(@"
exec spPAT_Anulacion
    @Cedula,
    @Tipo,
    @Monto,
    @TipoDoc,
    @NumDoc,
    @Usuario,
    '',
    '',
    0;", new
                {
                    Cedula = request.cedula,
                    Tipo = request.tipo_rubro,
                    Monto = monto,
                    TipoDoc = tipoDocumento,
                    NumDoc = numDocumento,
                    Usuario = request.usuario
                }, tx);

                tx.Commit();

                _dbBitacora.Bitacora(new BitacoraInsertarDto
                {
                    EmpresaId = codEmpresa,
                    Usuario = request.usuario,
                    Movimiento = "Anula",
                    Modulo = vModulo,
                    DetalleMovimiento = $"{ObtenerTextoRubro(request.tipo_rubro)} Anula: {monto:N2}, Id: {request.cedula}"
                });

                var impresion = _mRecibos.sbImprimeRecibo(codEmpresa, numDocumento, tipoDocumento, request.usuario);

                response.tipo_documento = tipoDocumento;
                response.num_documento = numDocumento;
                response.monto_aplicado = monto;
                response.reporte_resultado = impresion.Code == -1 ? null : impresion.Result?.ToString();
                response.mensaje = impresion.Code == -1
                    ? $"Anulación realizada con Nota Débito {numDocumento}, pero no se pudo generar el recibo: {impresion.Description}"
                    : $"Anulación realizada ... Con Nota Débito: {numDocumento}";

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception ex)
            {
                try { tx.Rollback(); } catch { }
                return DbHelper.CreateErrorResponse(ex.Message, -1, response);
            }
        }

        private static decimal ObtenerSaldoRubro(FrmAhAnulaAhorrosConsultaResponse data, string tipoRubro) => (tipoRubro ?? "").Trim().ToUpper() switch
        {
            "O" or "OBR" => data.obrero,
            "P" or "PAT" => data.patronal,
            "C" or "CAP" => data.capitaliza,
            "X" or "CST" => data.custodia,
            _ => 0m
        };

        private static string ObtenerTextoRubro(string tipoRubro) => (tipoRubro ?? "").Trim().ToUpper() switch
        {
            "O" or "OBR" => "Aporte Obrero",
            "P" or "PAT" => "Aporte Patronal",
            "C" or "CAP" => "Capitalización",
            "X" or "CST" => "Aporte en Custodia",
            _ => "Desconocido"
        };

        private static string ObtenerTextoAccion(string accion) => (accion ?? "").Trim().ToUpper() switch
        {
            "S" => "Saldo a Favor",
            _ => "Cuenta Contable"
        };

        private static decimal TipoCambioApl(decimal tipoCambio)
        {
            if (tipoCambio == 0) tipoCambio = 1;
            return tipoCambio > 0 ? tipoCambio : 1 / Math.Abs(tipoCambio);
        }

        private static void EjecutarAsiento(SqlConnection conn, SqlTransaction tx, string tipoDocumento, string numDocumento, decimal monto, string dc, string codDivisa, decimal tipoCambio, int enlace, string codUnidad, string codCentroCosto, string cuenta, string referencia1, string referencia2, string referencia3)
        {
            conn.Execute(@"
exec spSIFDocsAsiento
    @tipo_documento,
    @num_documento,
    @monto,
    @dc,
    @cod_divisa,
    @tipo_cambio,
    @enlace,
    @cod_unidad,
    @cod_centro_costo,
    @cuenta,
    @referencia_01,
    @referencia_02,
    @referencia_03;", new
            {
                tipo_documento = tipoDocumento,
                num_documento = numDocumento,
                monto,
                dc,
                cod_divisa = codDivisa,
                tipo_cambio = tipoCambio,
                enlace,
                cod_unidad = codUnidad,
                cod_centro_costo = codCentroCosto,
                cuenta,
                referencia_01 = referencia1,
                referencia_02 = referencia2,
                referencia_03 = referencia3
            }, tx);
        }

        private static (decimal aporte, string cuenta)? ConsultarCuentaAporte(SqlConnection conn, SqlTransaction tx, string cedula, string tipoRubro)
        {
            string colAporte;
            string colCuenta;

            switch ((tipoRubro ?? "").Trim().ToUpper())
            {
                case "P":
                case "PAT":
                    colAporte = "Aporte";
                    colCuenta = "Cta_Patronal";
                    break;
                case "X":
                case "CST":
                    colAporte = "Custodia";
                    colCuenta = "cta_custodia";
                    break;
                case "C":
                case "CAP":
                    colAporte = "capitaliza";
                    colCuenta = "cta_capitaliza";
                    break;
                default:
                    colAporte = "ahorro";
                    colCuenta = "cta_obrero";
                    break;
            }

            string sql = $@"
select
    isnull(P.{colAporte},0) as aporte,
    rtrim((select {colCuenta} from par_afah where Cod_Divisa = P.Cod_Divisa)) as cuenta
from vPAT_Consulta_Integrada P
where P.cedula = @cedula;";

            return conn.QueryFirstOrDefault<(decimal aporte, string cuenta)>(sql, new { cedula }, tx);
        }
    }
}
