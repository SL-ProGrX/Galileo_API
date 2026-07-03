using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Pasivos;

namespace Galileo_API.DataBaseTier.ProGrX_Pasivos
{
    public class FrmCrApaMovimientosDB
    {
        private readonly PortalDB _portalDb;
        private readonly MRecibos _mRecibos; 
        private readonly MProGrxMain _mProGrxMain;
        private readonly MCntLinkDB _mCntLinkDb;

        public FrmCrApaMovimientosDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
            _mRecibos = new MRecibos(config);
            _mCntLinkDb = new MCntLinkDB(config);
        }

        private const string MsgErrorAplicar = "No fue posible aplicar el movimiento.";

        /// <summary>
        /// Consulta los datos base del acreedor seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosAcreedorDto?> CR_APA_Movimientos_Acreedor_Obtener(
            int codEmpresa,
            string cod_acreedor)
        {
            return DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosAcreedorDto?>(
                _portalDb,
                codEmpresa,
                "exec spAPA_ConsultaAcreedor @Acreedor",
                null,
                new { Acreedor = cod_acreedor.Trim() });
        }

        /// <summary>
        /// Consulta el resumen principal de una operacion APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosOperacionDto?> CR_APA_Movimientos_Operacion_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {

            return DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosOperacionDto?>(
                _portalDb,
                codEmpresa,
                "exec spAPA_ConsultaOperacion @Acreedor, @Operacion",
                null,
                new
                {
                    Acreedor = cod_acreedor.Trim(),
                    Operacion = operacion
                });
        }

        /// <summary>
        /// Consulta el detalle historico de movimientos de la operacion APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cod_acreedor"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaMovimientosDetalleDto>> CR_APA_Movimientos_Detalle_Obtener(
            int codEmpresa,
            string cod_acreedor,
            string operacion)
        {
            return DbHelper.ExecuteListQuery<FrmCrApaMovimientosDetalleDto>(
                _portalDb,
                codEmpresa,
                "exec spAPA_ConsultaOperacionDetalle @Acreedor, @Operacion",
                new
                {
                    Acreedor = cod_acreedor.Trim(),
                    Operacion = operacion
                });
        }

        /// <summary>
        /// Obtiene la cuenta contable por defecto para afectar movimientos APA.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosCuentaDto?> CR_APA_Movimientos_Cuenta_Obtener(
            int codEmpresa, string usuario)
        {
            var globales = _mProGrxMain.sbSifParametrosInicializa(codEmpresa, usuario).Result;
            if (globales != null) { 
                return DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosCuentaDto?>(
                        _portalDb,
                        codEmpresa,
                        @"select top 1
                            D.cod_cuenta, 
                            isnull(C.COD_CUENTA_MASK,C.COD_CUENTA) AS 'cuenta_mask', 
                            C.descripcion
                        From SIF_DOCUMENTOS D 
                            left join CntX_Cuentas C on C.cod_Contabilidad = @CodConta
                            and D.cod_cuenta = C.cod_Cuenta
                        where D.TIPO_DOCUMENTO = 'APA';",
                        null,
                        new { CodConta = globales.GEnlace });
            } else {
                return DbHelper.CreateErrorResponse<FrmCrApaMovimientosCuentaDto?>(
                    "Error al obtener al cargar la cuenta de SIF_DOCUMENTOS",
                    -2,
                    null);
            }
        }

        /// <summary>
        /// Navega a la operacion anterior o siguiente del mismo acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosNavegarDto?> CR_APA_Movimientos_Operacion_Navegar(
            int codEmpresa,
            FrmCrApaMovimientosNavegarRequest request)
        {
            string sql = string.Equals(request.direccion, "A", StringComparison.OrdinalIgnoreCase)
                ? @"
                select top 1
                    operacion
                from CRD_APA_OPERACIONES
                where cod_acreedor = @Acreedor
                  and operacion < @Operacion
                  and (@SoloConSaldo = 0 or saldo > 0)
                order by operacion desc;"
                                : @"
                select top 1
                    operacion
                from CRD_APA_OPERACIONES
                where cod_acreedor = @Acreedor
                  and operacion > @Operacion
                  and (@SoloConSaldo = 0 or saldo > 0)
                order by operacion asc;";

            var response = DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosNavegarDto?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    Acreedor = request.cod_acreedor.Trim(),
                    Operacion = request.operacion,
                    SoloConSaldo = request.solo_con_saldo ? 1 : 0
                });

            if (response.Code == 0 && response.Result is null)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaMovimientosNavegarDto?>(
                    "No se encontraron mas operaciones.",
                    -2,
                    null);
            }

            return response;
        }

        /// <summary>
        /// Aplica un movimiento APA y devuelve la informacion del recibo generado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FrmCrApaMovimientosAplicarResultadoDto?> CR_APA_Movimientos_Aplicar(
            int codEmpresa,
            FrmCrApaMovimientosAplicarRequest request)
        {

            string cuentaFormato = _mCntLinkDb.fxgCntCuentaFormato(codEmpresa, false, request.cuenta, 0);
            if (string.IsNullOrWhiteSpace(cuentaFormato))
            {
                return DbHelper.CreateErrorResponse<FrmCrApaMovimientosAplicarResultadoDto?>(
                    "Cuenta indicada no es v&aacute;lida.",
                    -2,
                    null);
            }

            var spResponse = DbHelper.ExecuteSingleQuery<FrmCrApaMovimientosAplicarSpDto?>(
                _portalDb,
                codEmpresa,
                @"exec spAPA_Movimiento
                    @Acreedor,
                    @Operacion,
                    @Tipo,
                    @Amortiza,
                    @Intereses,
                    @Comision,
                    @Cargos,
                    @Notas,
                    @Cuenta,
                    @DocRef,
                    @Usuario",
                null,
                new
                {
                    Acreedor = request.cod_acreedor.Trim(),
                    Operacion = request.operacion,
                    Tipo = request.tipo,
                    Amortiza = request.amortiza,
                    Intereses = request.intereses,
                    Comision = request.comision,
                    Cargos = request.cargos,
                    Notas = request.notas.Trim(),
                    Cuenta = cuentaFormato,
                    DocRef = request.doc_ref.Trim(),
                    Usuario = request.usuario
                });

            if (spResponse.Result is null)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaMovimientosAplicarResultadoDto?>(
                    MsgErrorAplicar,
                    -1,
                    null);
            }

            if (spResponse.Code != 0)
            {
                return DbHelper.CreateErrorResponse<FrmCrApaMovimientosAplicarResultadoDto?>(
                    string.IsNullOrWhiteSpace(spResponse.Description) ? MsgErrorAplicar : spResponse.Description,
                    spResponse.Code ?? -1,
                    null);
            }

            string tipoDocumento = spResponse.Result.tipo_documento ?? string.Empty;
            string codTransaccion = spResponse.Result.cod_transaccion ?? string.Empty;
            string usuario = request.usuario ?? string.Empty;

            object reporte = new object();

            if (!string.IsNullOrWhiteSpace(tipoDocumento) &&
                !string.IsNullOrWhiteSpace(codTransaccion) &&
                !string.IsNullOrWhiteSpace(usuario))
            {
                reporte = _mRecibos
                    .sbImprimeRecibo(codEmpresa, tipoDocumento, codTransaccion, usuario)?
                    .Result ?? reporte;
            }

            return DbHelper.CreateOkResponse(
                new FrmCrApaMovimientosAplicarResultadoDto
                {
                    cod_transaccion = codTransaccion,
                    tipo_documento = tipoDocumento,
                    mensaje = $"Movimiento Realizado Satisfactoriamente!",
                    reporte_resultado = reporte
                } ?? null);
        }

        /// <summary>
        /// Obtiene la lista de acreedores.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_APA_Movimientos_Acreedores_Obtener(
            int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                @"
                select
                    cod_acreedor AS 'item', descripcion 
                from CRD_APA_ACREEDORES
                ");
        }

        /// <summary>
        /// Obtiene la lista de operaciones del acreedor.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="acreedor"></param>
        /// <returns></returns>
        public ErrorDto<List<FrmCrApaMovimientosOperacionBusquedaDto>> CR_APA_Movimientos_Operaciones_Obtener(
            int codEmpresa,
            string acreedor)
        {
            return DbHelper.ExecuteListQuery<FrmCrApaMovimientosOperacionBusquedaDto>(
                _portalDb,
                codEmpresa,
                @"
                select 
                    OPERACION,COD_ACREEDOR, MONTO, SALDO, FECHA_FORMALIZA AS 'FORMALIZA'
                from CRD_APA_OPERACIONES
                where COD_ACREEDOR = @Acreedor
                order by operacion;",
                new
                {
                    Acreedor = acreedor.Trim()
                });
        }
    }
}