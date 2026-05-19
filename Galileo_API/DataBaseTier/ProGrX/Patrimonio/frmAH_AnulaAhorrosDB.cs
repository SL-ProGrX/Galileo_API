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
        public ErrorDto<FrmAhAnulaAhorrosProcesarResponse> Patrimonio_frmAH_AnulaAhorros_Procesar(
    int codEmpresa,
    FrmAhAnulaAhorrosProcesarRequest request)
        {
            var response = new FrmAhAnulaAhorrosProcesarResponse();

            var validacionRequest = Patrimonio_frmAH_AnulaAhorros_ValidarRequestProcesar(request, response);
            if (validacionRequest != null)
                return validacionRequest;

            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
            conn.Open();
            using var tx = conn.BeginTransaction();

            try
            {
                var consulta = Patrimonio_frmAH_AnulaAhorros_ConsultarPersona(conn, tx, request.cedula);
                if (consulta == null)
                    return DbHelper.CreateErrorResponse("No se localizó la persona o sus registros de aportes.", -2, response);

                decimal monto = Patrimonio_frmAH_AnulaAhorros_ObtenerMontoProcesar(request);
                if (monto <= 0)
                    return DbHelper.CreateErrorResponse("El monto debe ser mayor que cero.", -2, response);

                decimal saldoRubro = ObtenerSaldoRubro(consulta, request.tipo_rubro);
                if (monto > saldoRubro)
                {
                    return DbHelper.CreateErrorResponse(
                        "El monto de la anulación no puede exceder el saldo del rubro seleccionado.",
                        -2,
                        response);
                }

                var globales = _mProGrx.sbSifParametrosInicializa(codEmpresa, request.usuario).Result;
                if (globales == null)
                    return DbHelper.CreateErrorResponse("No se pudieron obtener los parámetros globales del usuario.", -2, response);

                var cuentaAporte = ConsultarCuentaAporte(conn, tx, request.cedula, request.tipo_rubro);
                if (cuentaAporte == (null, null) || string.IsNullOrWhiteSpace(cuentaAporte.cuenta))
                {
                    return DbHelper.CreateErrorResponse(
                        "No se pudo resolver la cuenta contable del rubro de patrimonio.",
                        -2,
                        response);
                }

                var destino = Patrimonio_frmAH_AnulaAhorros_ResolverCuentaDestino(conn, tx, codEmpresa, request);
                if (!destino.EsValido)
                    return DbHelper.CreateErrorResponse(destino.MensajeError, -2, response);

                const string tipoDocumento = "ND";
                string numDocumento = _mRecibos.FxDocumentoConsecutivo(codEmpresa, tipoDocumento).ToString();
                decimal tipoCambio = _mCajas.fxCajasTipoCambio(codEmpresa, 0, consulta.cod_divisa);
                decimal factorAplicado = TipoCambioApl(tipoCambio);
                decimal saldoActual = saldoRubro - monto;

                string nombreCliente = string.IsNullOrWhiteSpace(request.nombre) ? consulta.nombre : request.nombre;
                string detalle = string.IsNullOrWhiteSpace(request.notas) ? "Anulación de ahorro" : request.notas.Trim();
                string[] lineas = Patrimonio_frmAH_AnulaAhorros_ConstruirLineasProcesar(request, consulta, cuentaAporte.aporte, monto, saldoActual);

                Patrimonio_frmAH_AnulaAhorros_InsertarTransaccion(
                    conn,
                    tx,
                    request,
                    new InsertarTransaccionParametrosRequest
                    {
                        tipoDocumento = tipoDocumento,
                        numDocumento = numDocumento,
                        monto = monto,
                        detalle = detalle,
                        nombreCliente = nombreCliente,
                        oficinaTitular = globales.GOficinaTitular,
                        lineas = lineas
                    });

                decimal montoAsiento = monto * factorAplicado;

                EjecutarAsiento(
                    conn,
                    tx,
                    new EjecutarAsientoParametrosRequest
                    {
                         tipoDocumento = tipoDocumento,
                         numDocumento = numDocumento,
                         monto = montoAsiento,
                         dc = "D",
                         codDivisa = consulta.cod_divisa,
                         tipoCambio = tipoCambio,
                         enlace = globales.GEnlace,
                         codUnidad = globales.GOficinaUnidad,
                         codCentroCosto = string.Empty,
                         cuenta = cuentaAporte.cuenta,
                         referencia1 = $"Pat:{request.tipo_rubro}",
                         referencia2 = request.cedula,
                         referencia3 = string.Empty
                    });

                EjecutarAsiento(
                    conn,
                    tx,
                    new EjecutarAsientoParametrosRequest
                    {
                        tipoDocumento = tipoDocumento,
                        numDocumento = numDocumento,
                        monto = montoAsiento,
                        dc = "C",
                        codDivisa = consulta.cod_divisa,
                        tipoCambio = tipoCambio,
                        enlace = globales.GEnlace,
                        codUnidad = globales.GOficinaUnidad,
                        codCentroCosto = string.Empty,
                        cuenta = destino.CuentaDestino,
                        referencia1 = $"Pat:{request.tipo_rubro}",
                        referencia2 = request.cedula,
                        referencia3 = string.Empty
                    });

                Patrimonio_frmAH_AnulaAhorros_RegistrarSaldoFavorSiAplica(
                    conn,
                    tx,
                    request,
                    new RegistrarSaldoFavorSiAplicaParametrosRequest
                    {
                        codDivisa = consulta.cod_divisa,
                        oficinaUnidad = globales.GOficinaUnidad,
                        tipoDocumento = tipoDocumento,
                        numDocumento = numDocumento,
                        monto = monto,
                        nombreCliente = nombreCliente
                    },
                    destino);

                Patrimonio_frmAH_AnulaAhorros_EjecutarAnulacion(
                    conn,
                    tx,
                    new EjecutarAnulacionParametrosRequest
                    {
                        cedula = request.cedula,
                        tipoRubro = request.tipo_rubro,
                        monto = monto,
                        tipoDocumento = tipoDocumento,
                        numDocumento = numDocumento,
                        usuario = request.usuario
                    });

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
                try { tx.Rollback(); } catch (Exception) { 
                    return DbHelper.CreateErrorResponse($"Error al procesar la anulación: {ex.Message}. Además, no se pudo revertir la transacción.", -1, response);
                }
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

        private static void EjecutarAsiento(SqlConnection conn, SqlTransaction tx, EjecutarAsientoParametrosRequest request)
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
                tipo_documento = request.tipoDocumento,
                num_documento = request.numDocumento,
                request.monto,
                request.dc,
                cod_divisa = request.codDivisa,
                tipo_cambio = request.tipoCambio,
                request.enlace,
                cod_unidad = request.codUnidad,
                cod_centro_costo = request.codCentroCosto,
                request.cuenta,
                referencia_01 = request.referencia1,
                referencia_02 = request.referencia2,
                referencia_03 = request.referencia3
            }, tx);
        }

        private static (decimal aporte, string cuenta) ConsultarCuentaAporte(
            SqlConnection conn,
            SqlTransaction tx,
            string cedula,
            string tipoRubro)
        {
            var columnas = Patrimonio_frmAH_AnulaAhorros_ObtenerColumnasRubro(tipoRubro);
            if (columnas == null)
                return default;

            var sql = $@"
                select top 1
                    isnull({columnas.Value.ColumnaAporte}, 0) as aporte,
                    rtrim(isnull({columnas.Value.ColumnaCuenta}, '')) as cuenta
                from vPAT_Consolidado
                where cedula = @cedula;";

            return conn.QueryFirstOrDefault<(decimal aporte, string cuenta)>(
                sql,
                new { cedula },
                tx);
        }

        private static (string ColumnaAporte, string ColumnaCuenta)? Patrimonio_frmAH_AnulaAhorros_ObtenerColumnasRubro(string tipoRubro)
        {
            return (tipoRubro ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "OBR" => ("obrero", "cta_obrero"),
                "PAT" => ("patronal", "cta_patronal"),
                "CUS" => ("custodia", "cta_custodia"),
                "CAP" => ("capitaliza", "cta_capitaliza"),
                _ => null
            };
        }

        private static ErrorDto<FrmAhAnulaAhorrosProcesarResponse> Patrimonio_frmAH_AnulaAhorros_ValidarRequestProcesar(
    FrmAhAnulaAhorrosProcesarRequest request,
    FrmAhAnulaAhorrosProcesarResponse response)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.cedula) || string.IsNullOrWhiteSpace(request.usuario))
                return DbHelper.CreateErrorResponse("La solicitud es inválida.", -2, response);

            return new ErrorDto<FrmAhAnulaAhorrosProcesarResponse>();
        }

        private static FrmAhAnulaAhorrosConsultaResponse Patrimonio_frmAH_AnulaAhorros_ConsultarPersona(
            SqlConnection conn,
            SqlTransaction tx,
            string cedula)
        {
            return conn.QueryFirstOrDefault<FrmAhAnulaAhorrosConsultaResponse>(@"
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
                        where cedula = @cedula;", new { cedula }, tx) ?? new FrmAhAnulaAhorrosConsultaResponse();
        }

        private static decimal Patrimonio_frmAH_AnulaAhorros_ObtenerMontoProcesar(FrmAhAnulaAhorrosProcesarRequest request)
        {
            bool esMov = string.Equals(request.tipo_anulacion?.Trim(), "MOV", StringComparison.OrdinalIgnoreCase);
            bool tieneMovimientos = request.movimientos != null && request.movimientos.Count > 0;

            return esMov && tieneMovimientos
                ? request.movimientos!.Sum(x => x.monto)
                : request.monto;
        }

        private static string[] Patrimonio_frmAH_AnulaAhorros_ConstruirLineasProcesar(
            FrmAhAnulaAhorrosProcesarRequest request,
            FrmAhAnulaAhorrosConsultaResponse consulta,
            decimal aporte,
            decimal monto,
            decimal saldoActual)
        {
            return
            [
                $"Plan            : {ObtenerTextoRubro(request.tipo_rubro)}",
        " ",
        $"Saldo Anterior  : {aporte:N2}",
        $"Monto Anulación : {monto:N2}",
        $"Saldo Actual    : {saldoActual:N2}",
        " ",
        $"Divisa          : {consulta.cod_divisa}",
        " ",
        $"Usuario         : {request.usuario}",
        $"Acción          : {ObtenerTextoAccion(request.accion!)}"
            ];
        }

        private void Patrimonio_frmAH_AnulaAhorros_InsertarTransaccion(
            SqlConnection conn,
            SqlTransaction tx,
            FrmAhAnulaAhorrosProcesarRequest request,
            InsertarTransaccionParametrosRequest parametros
            )
        {
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
                cod_transaccion = parametros.numDocumento,
                tipo_documento = parametros.tipoDocumento,
                registro_usuario = request.usuario,
                cliente_identificacion = request.cedula,
                cliente_nombre = parametros.nombreCliente,
                monto = parametros.monto * -1,
                referencia_01 = request.cedula,
                cod_oficina = parametros.oficinaTitular,
                linea1 = parametros.lineas[0],
                linea2 = parametros.lineas[1],
                linea3 = parametros.lineas[2],
                linea4 = parametros.lineas[3],
                linea5 = parametros.lineas[4],
                linea6 = parametros.lineas[5],
                linea7 = parametros.lineas[6],
                linea8 = parametros.lineas[7],
                linea9 = parametros.lineas[8],
                linea10 = parametros.lineas[9],
                detalle = parametros.detalle,
                documento = string.Empty,
                cod_caja = string.Empty
            }, tx);
        }

        private void Patrimonio_frmAH_AnulaAhorros_EjecutarAnulacion(
            SqlConnection conn,
            SqlTransaction tx,
            EjecutarAnulacionParametrosRequest request
            )
        {
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
                Tipo = request.tipoRubro,
                Monto = request.monto,
                TipoDoc = request.tipoDocumento,
                NumDoc = request.numDocumento,
                Usuario = request.usuario
            }, tx);
        }

        private void Patrimonio_frmAH_AnulaAhorros_RegistrarSaldoFavorSiAplica(
            SqlConnection conn,
            SqlTransaction tx,
            FrmAhAnulaAhorrosProcesarRequest request,
            RegistrarSaldoFavorSiAplicaParametrosRequest parametro,
            PatrimoniofrmAHAnulaAhorrosCuentaDestino destino)
        {
            if (!destino.RequiereSaldoFavor)
                return;

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
                FormaPago = destino.FormaPagoSaldoFavor,
                Referencia = $"{parametro.tipoDocumento}-{parametro.numDocumento}",
                Monto = parametro.monto,
                Cedula = request.cedula,
                Nombre = parametro.nombreCliente,
                Usuario = request.usuario,
                Divisa = parametro.codDivisa
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
                TipoDoc = parametro.tipoDocumento,
                NumDoc = parametro.numDocumento,
                Usuario = request.usuario,
                FormaPago = destino.FormaPagoSaldoFavor,
                Divisa = parametro.codDivisa,
                Monto = parametro.monto,
                Unidad = parametro.oficinaUnidad,
                Cuenta = destino.CuentaDestino,
                Referencia = $"{parametro.tipoDocumento}-{parametro.numDocumento}",
                SfId = sfId
            }, tx);
        }

        private PatrimoniofrmAHAnulaAhorrosCuentaDestino Patrimonio_frmAH_AnulaAhorros_ResolverCuentaDestino(
            SqlConnection conn,
            SqlTransaction tx,
            int codEmpresa,
            FrmAhAnulaAhorrosProcesarRequest request
            )
        {
            if (Patrimonio_frmAH_AnulaAhorros_EsAccionSaldoFavor(request.accion))
            {
                var saldoFavor = conn.QueryFirstOrDefault<(string cod_forma_pago, string cod_cuenta)>(@"
select top 1
    rtrim(COD_FORMA_PAGO) as cod_forma_pago,
    rtrim(COD_CUENTA) as cod_cuenta
from SIF_FORMAS_PAGO
where TIPO = 'S' and Activa = 1;", transaction: tx);

                if (string.IsNullOrWhiteSpace(saldoFavor.cod_cuenta))
                {
                    return PatrimoniofrmAHAnulaAhorrosCuentaDestino.CrearInvalido(
                        "No existe una forma de pago activa para saldo a favor.");
                }

                return PatrimoniofrmAHAnulaAhorrosCuentaDestino.CrearSaldoFavor(
                    saldoFavor.cod_cuenta,
                    saldoFavor.cod_forma_pago);
            }

            string cuentaDestino = _mRecibos.FxDocumentoCuenta(codEmpresa, "ND").Trim();
            if (string.IsNullOrWhiteSpace(cuentaDestino))
            {
                return PatrimoniofrmAHAnulaAhorrosCuentaDestino.CrearInvalido(
                    "No se puede realizar movimiento porque no se especificó una cuenta contable válida para esta operación.");
            }

            return PatrimoniofrmAHAnulaAhorrosCuentaDestino.CrearNormal(cuentaDestino);
        }

        private static bool Patrimonio_frmAH_AnulaAhorros_EsAccionSaldoFavor(string accion)
        {
            return string.Equals((accion ?? "C").Trim(), "S", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class PatrimoniofrmAHAnulaAhorrosCuentaDestino
        {
            public bool EsValido { get; private set; } = false;
            public bool RequiereSaldoFavor { get; private set; } = false;
            public string CuentaDestino { get; private set; } = string.Empty;
            public string FormaPagoSaldoFavor { get; private set; } = string.Empty;
            public string MensajeError { get; private set; } = string.Empty;

            public static PatrimoniofrmAHAnulaAhorrosCuentaDestino CrearNormal(string cuentaDestino)
            {
                return new PatrimoniofrmAHAnulaAhorrosCuentaDestino
                {
                    EsValido = true,
                    CuentaDestino = cuentaDestino,
                    FormaPagoSaldoFavor = string.Empty,
                    RequiereSaldoFavor = false,
                    MensajeError = string.Empty
                };
            }

            public static PatrimoniofrmAHAnulaAhorrosCuentaDestino CrearSaldoFavor(string cuentaDestino, string formaPagoSaldoFavor)
            {
                return new PatrimoniofrmAHAnulaAhorrosCuentaDestino
                {
                    EsValido = true,
                    CuentaDestino = cuentaDestino,
                    FormaPagoSaldoFavor = formaPagoSaldoFavor,
                    RequiereSaldoFavor = true,
                    MensajeError = string.Empty
                };
            }

            public static PatrimoniofrmAHAnulaAhorrosCuentaDestino CrearInvalido(string mensajeError)
            {
                return new PatrimoniofrmAHAnulaAhorrosCuentaDestino
                {
                    EsValido = false,
                    CuentaDestino = string.Empty,
                    FormaPagoSaldoFavor = string.Empty,
                    RequiereSaldoFavor = false,
                    MensajeError = mensajeError
                };
            }
        }
    }
}
