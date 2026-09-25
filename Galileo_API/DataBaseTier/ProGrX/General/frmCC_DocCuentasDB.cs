using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmCcDocCuentasDb
    {
        private const int LargoReferencia = 30;
        private const int LargoDetalle = 255;
        private const string SqlValidaCuentaAuxiliar = "exec spSIFValidaCuentas @Cuenta";
        private const string MensajeDetalleRequerido = "Ingrese el detalle de este documento ...";
        private const string MensajeCuentaFormato = "Código de cuenta inválido...";
        private const string TipoComprobanteBoletaGeneral = "00";
        private const string SqlDocumentoV2 = @"
SELECT RTRIM(ISNULL(tipo_comprobante,'')) AS tipo_comprobante,
       RTRIM(ISNULL(COD_CUENTA,'')) AS cuenta
FROM SIF_DOCUMENTOS
WHERE tipo_documento = @tipo;";
        private const string MensajeCuentaInvalida =
            "La cuenta ingresada no es válida en el plan contable o Esta siendo utiliza por algun auxiliar, verifique...";

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _mCntLinkDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmCcDocCuentasDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mCntLinkDb = new MCntLinkDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la cuenta configurada del tipo de documento e indica si debe solicitarse en frmCC_DocCuentas.
        /// Equivale a la sección vVerificar de mRecibos.fxDocumentoCuenta.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="tipo">Tipo de documento (RE, DP, ND, NC u otro de SIF_DOCUMENTOS).</param>
        /// <returns>Cuenta configurada y bandera de verificación.</returns>
        public ErrorDto<CcDocCuentasVerificarData> CC_DocCuentas_Documento_Verificar(int CodEmpresa, string? tipo)
        {
            var data = new CcDocCuentasVerificarData();
            var tipoDocumento = (tipo ?? string.Empty).Trim().ToUpperInvariant();

            if (tipoDocumento.Length == 0)
            {
                return DbHelper.CreateErrorResponse("Debe indicar el tipo de documento.", -2, data);
            }

            var sysDocVersion = _mProGrxMain.EmpresaEnlaceObtener()?.FirstOrDefault()?.SysDocVersion ?? 0;

            var resultado = DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
                sysDocVersion == 1
                    ? VerificarDocumentoV1(connection, tipoDocumento)
                    : VerificarDocumentoV2(connection, tipoDocumento));

            if (resultado.Code != 0 || resultado.Result == null)
            {
                return DbHelper.CreateErrorResponse(
                    resultado.Description ?? "No se pudo obtener la configuración del documento.",
                    -1,
                    data);
            }

            return DbHelper.CreateOkResponse(resultado.Result);
        }

        /// <summary>
        /// Obtiene la cuenta contable sin máscara, con máscara y su descripción.
        /// Equivale a Form_Load, txtCuenta_KeyDown y sbBuscaCuenta de frmCC_DocCuentas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cuenta">Cuenta contable con o sin máscara.</param>
        /// <returns>Cuenta formateada y descripción.</returns>
        public ErrorDto<CcDocCuentasCuentaData> CC_DocCuentas_Cuenta_Obtener(int CodEmpresa, string? cuenta)
        {
            var data = new CcDocCuentasCuentaData();
            var cuentaLimpia = QuitarGuiones(cuenta);

            if (cuentaLimpia.Length == 0)
            {
                return DbHelper.CreateOkResponse(data);
            }

            if (!double.TryParse(cuentaLimpia, out _))
            {
                return DbHelper.CreateErrorResponse(MensajeCuentaFormato, -2, data);
            }

            var parametros = _mCntLinkDb.sbgCntParametros(CodEmpresa);
            if (parametros.Code == -1 || parametros.Result == null)
            {
                return DbHelper.CreateErrorResponse(
                    parametros.Description ?? "No se pudo obtener los parámetros contables.",
                    -1,
                    data);
            }

            data.cod_cuenta = _mCntLinkDb.fxgCntCuentaFormato(CodEmpresa, false, cuentaLimpia, 0);
            data.cod_cuenta_mask = _mCntLinkDb.fxgCntCuentaFormato(CodEmpresa, true, cuentaLimpia, 0);
            data.descripcion = _mCntLinkDb.fxgCntCuentaDesc(CodEmpresa, data.cod_cuenta, parametros.Result.gEnlace);

            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Valida los datos del documento y devuelve los valores depurados.
        /// Equivale a btnTool_Click(0) de frmCC_DocCuentas.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Cuenta, referencia y detalle capturados.</param>
        /// <returns>Cuenta sin máscara, depósito, detalle y bandera de validez.</returns>
        public ErrorDto<CcDocCuentasResultadoData> CC_DocCuentas_Documento_Validar(
            int CodEmpresa,
            CcDocCuentasValidarRequest request)
        {
            var resultado = new CcDocCuentasResultadoData();
            var detalle = CadenaDepura(request.detalle);

            if (detalle.Length == 0)
            {
                return DbHelper.CreateErrorResponse(MensajeDetalleRequerido, -2, resultado);
            }

            var cuenta = (request.cuenta ?? string.Empty).Trim();
            var validacion = ValidaCuenta(CodEmpresa, cuenta);

            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? MensajeCuentaInvalida,
                    validacion.Code ?? -1,
                    resultado);
            }

            if (!validacion.Result)
            {
                return DbHelper.CreateErrorResponse(MensajeCuentaInvalida, -2, resultado);
            }

            resultado.detalle = Recortar(detalle, LargoDetalle).Trim().ToUpperInvariant();
            resultado.deposito = Recortar(CadenaDepura(request.referencia), LargoReferencia).Trim();
            resultado.cuenta = _mCntLinkDb.fxgCntCuentaFormato(CodEmpresa, false, cuenta, 0);
            resultado.valido = true;

            return DbHelper.CreateOkResponse(resultado);
        }

        /// <summary>
        /// fxValidaCuenta: la cuenta debe ser válida en el plan contable
        /// y no estar siendo utilizada por un auxiliar (spSIFValidaCuentas).
        /// </summary>
        private ErrorDto<bool> ValidaCuenta(int CodEmpresa, string cuenta)
        {
            if (!_mCntLinkDb.fxgCntCuentaValida(CodEmpresa, cuenta))
            {
                return DbHelper.CreateOkResponse(false);
            }

            return DbHelper.WithConn(_portalDb, CodEmpresa, connection =>
            {
                var existe = connection.QueryFirstOrDefault<int?>(
                    SqlValidaCuentaAuxiliar,
                    new { Cuenta = cuenta });

                return existe == 0;
            });
        }

        /// <summary>Control de documentos v1: cuenta en ase_consecutivos; DP, ND y NC se verifican.</summary>
        private static CcDocCuentasVerificarData VerificarDocumentoV1(SqlConnection connection, string tipo)
        {
            var sql = tipo switch
            {
                "RE" => "select CS_RE_CUENTA from ase_consecutivos",
                "DP" => "select CS_DP_CUENTA from ase_consecutivos",
                "ND" => "select CS_ND_CUENTA from ase_consecutivos",
                "NC" => "select CS_NC_CUENTA from ase_consecutivos",
                _ => string.Empty
            };

            if (sql.Length == 0)
            {
                return new CcDocCuentasVerificarData();
            }

            return new CcDocCuentasVerificarData
            {
                cuenta = (connection.QueryFirstOrDefault<string>(sql) ?? string.Empty).Trim(),
                verificar = tipo != "RE"
            };
        }

        /// <summary>Control de documentos v2: se verifica cuando el comprobante es Boleta General (00).</summary>
        private static CcDocCuentasVerificarData VerificarDocumentoV2(SqlConnection connection, string tipo)
        {
            var documento = connection.QueryFirstOrDefault<(string tipo_comprobante, string cuenta)>(
                SqlDocumentoV2,
                new { tipo });

            return new CcDocCuentasVerificarData
            {
                cuenta = documento.cuenta ?? string.Empty,
                verificar = documento.tipo_comprobante == TipoComprobanteBoletaGeneral
            };
        }

        private static string QuitarGuiones(string? valor) =>
            (valor ?? string.Empty).Trim().Replace("-", string.Empty);

        /// <summary>fxCadenaDepura: trim y elimina comillas simples.</summary>
        private static string CadenaDepura(string? valor) =>
            (valor ?? string.Empty).Trim().Replace("'", string.Empty);

        private static string Recortar(string valor, int largo) =>
            valor.Length > largo ? valor[..largo] : valor;
    }
}
