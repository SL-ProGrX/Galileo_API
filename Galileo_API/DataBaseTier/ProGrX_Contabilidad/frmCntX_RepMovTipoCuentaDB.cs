using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepMovTipoCuentaDb
    {
        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MCntXCalculosDb _calculosDb;
        private readonly MCntXModuloDb _moduloDb;

        public FrmCntXRepMovTipoCuentaDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _calculosDb = new MCntXCalculosDb(config);
            _moduloDb = new MCntXModuloDb(config);
        }

        /// <summary>
        /// Obtiene la informacion inicial requerida por el reporte.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CntXRepMovTipoCuentaInicializarResponse>
            CntX_frmCntX_RepMovTipoCuenta_Inicializar(
                int codEmpresa,
                int codContabilidad)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    new CntXRepMovTipoCuentaInicializarResponse());
            }

            const string sql = """
                select
                    getdate() as fecha_servidor,
                    isnull(rtrim(min(cod_cuenta)), '') as cuenta_minima,
                    isnull(rtrim(max(cod_cuenta)), '') as cuenta_maxima
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad;
                """;

            var inicializacion =
                DbHelper.ExecuteSingleQuery<CntXRepMovTipoCuentaInicializarResponse>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new CntXRepMovTipoCuentaInicializarResponse(),
                    new
                    {
                        CodContabilidad = codContabilidad
                    });

            if (inicializacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    inicializacion.Description
                        ?? "No fue posible inicializar el reporte.",
                    inicializacion.Code.GetValueOrDefault(-1),
                    new CntXRepMovTipoCuentaInicializarResponse());
            }

            var unidades =
                _moduloDb.sbCntX_CargaCboUnidades(
                    codEmpresa,
                    codContabilidad);

            if (unidades.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    unidades.Description
                        ?? "No fue posible consultar las unidades.",
                    unidades.Code.GetValueOrDefault(-1),
                    new CntXRepMovTipoCuentaInicializarResponse());
            }

            var centrosCosto =
                CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener(
                    codEmpresa,
                    codContabilidad,
                    string.Empty);

            if (centrosCosto.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    centrosCosto.Description
                        ?? "No fue posible consultar los centros de costo.",
                    centrosCosto.Code.GetValueOrDefault(-1),
                    new CntXRepMovTipoCuentaInicializarResponse());
            }

            var respuesta =
                inicializacion.Result
                ?? new CntXRepMovTipoCuentaInicializarResponse();

            respuesta.unidades = unidades.Result ?? [];
            respuesta.centros_costo =
                centrosCosto.Result ?? [];

            return DbHelper.CreateOkResponse(respuesta);
        }

        /// <summary>
        /// Obtiene los centros de costo asociados a una unidad.
        /// Si la unidad esta vacia, devuelve todos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_RepMovTipoCuenta_CentrosCosto_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? unidad)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    new List<DropDownListaGenericaModel>());
            }

            const string sql = """
                select
                    rtrim(C.cod_centro_costo) as item,
                    rtrim(C.descripcion) as descripcion
                from CntX_Centro_Costos C
                where C.cod_contabilidad = @CodContabilidad
                  and (
                      @Unidad = ''
                      or exists (
                          select 1
                          from CntX_Unidades_CC U
                          where U.cod_contabilidad = C.cod_contabilidad
                            and U.cod_centro_costo = C.cod_centro_costo
                            and U.cod_unidad = @Unidad
                      )
                  )
                order by C.descripcion;
                """;

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodContabilidad = codContabilidad,
                    Unidad = unidad?.Trim() ?? string.Empty
                });
        }

        /// <summary>
        /// Formatea una cuenta contable y obtiene su descripcion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<CntXRepMovTipoCuentaData?>
            CntX_frmCntX_RepMovTipoCuenta_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    CntXRepMovTipoCuentaData?>(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                return DbHelper.CreateErrorResponse<
                    CntXRepMovTipoCuentaData?>(
                    "Cuenta no es v&aacute;lida.",
                    -2,
                    null);
            }

            string cuentaFormateada =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    cuenta.Trim(),
                    0);

            if (string.IsNullOrWhiteSpace(cuentaFormateada))
            {
                return DbHelper.CreateErrorResponse<
                    CntXRepMovTipoCuentaData?>(
                    "Cuenta no es v&aacute;lida.",
                    -2,
                    null);
            }

            const string sql = """
                select top 1
                    rtrim(cod_cuenta) as cod_cuenta,
                    rtrim(cod_cuenta_mask) as cod_cuenta_mask,
                    rtrim(descripcion) as descripcion
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad
                  and cod_cuenta = @Cuenta;
                """;

            return DbHelper.ExecuteSingleQuery<
                CntXRepMovTipoCuentaData?>(
                _portalDb,
                codEmpresa,
                sql,
                null,
                new
                {
                    CodContabilidad = codContabilidad,
                    Cuenta = cuentaFormateada
                });
        }

        /// <summary>
        /// Prepara los saldos y movimientos temporales del reporte.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto
            CntX_frmCntX_RepMovTipoCuenta_Reporte_Preparar(
                int codEmpresa,
                CntXRepMovTipoCuentaPrepararRequest request)
        {
            if (request.cod_contabilidad <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    -2);
            }

            if (request.fecha_inicio == default
                || request.fecha_corte == default
                || request.fecha_inicio.Date >
                   request.fecha_corte.Date)
            {
                return DbHelper.ErrorResponse(
                    "El rango de fechas no es v&aacute;lido.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cuenta_inicio)
                || string.IsNullOrWhiteSpace(request.cuenta_corte))
            {
                return DbHelper.ErrorResponse(
                    "El rango de cuentas no es v&aacute;lido.",
                    -2);
            }

            var movimientoRequest =
                new CntXCalculosMovimientoCuentasRequest
                {
                    cod_contabilidad =
                        request.cod_contabilidad,
                    usuario = request.usuario.Trim(),
                    fecha_desde =
                        request.fecha_inicio.Date,
                    fecha_hasta =
                        request.fecha_corte.Date,
                    cuenta_inicio =
                        request.cuenta_inicio.Trim(),
                    cuenta_corte =
                        request.cuenta_corte.Trim(),
                    mov_en_cero =
                        request.mostrar_cuentas_cero ? 1 : 0,
                    unidad =
                        string.IsNullOrWhiteSpace(request.unidad)
                            ? "0x0"
                            : request.unidad.Trim(),
                    centro_costo =
                        string.IsNullOrWhiteSpace(
                            request.centro_costo)
                            ? "0x0"
                            : request.centro_costo.Trim(),
                    divisa_origen =
                        request.mostrar_divisa_origen ? 1 : 0,
                    pendientes =
                        request.mostrar_pendientes ? 1 : 0
                };

            return _calculosDb.SbCntX_MovimientoCuentas(
                codEmpresa,
                movimientoRequest);
        
        }
    }
}