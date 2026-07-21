using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXRepBalanceSituacionDb
    {
        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;
        private readonly MCntXModuloDb _moduloDb;

        public FrmCntXRepBalanceSituacionDb(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
            _moduloDb = new MCntXModuloDb(config);
        }

        /// <summary>
        /// Obtiene los datos iniciales requeridos por el reporte
        /// de balance de situacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <returns></returns>
        public ErrorDto<CntXRepBalanceSituacionInicializarResponse>
            CntX_frmCntX_RepBalanceSituacion_Inicializar(
                int codEmpresa,
                int codContabilidad)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    new CntXRepBalanceSituacionInicializarResponse());
            }

            const string sql = """
                select
                    isnull(rtrim(min(cod_cuenta)), '') as cuenta_minima,
                    isnull(rtrim(max(cod_cuenta)), '') as cuenta_maxima
                from CntX_Cuentas
                where cod_contabilidad = @CodContabilidad;
                """;

            var inicializacion =
                DbHelper.ExecuteSingleQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new CntXRepBalanceSituacionInicializarResponse(),
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
                    new CntXRepBalanceSituacionInicializarResponse());
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
                    new CntXRepBalanceSituacionInicializarResponse());
            }

            var centrosCosto =
                CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener(
                    codEmpresa,
                    codContabilidad,
                    string.Empty);

            if (centrosCosto.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    centrosCosto.Description
                        ?? "No fue posible consultar los centros de costo.",
                    centrosCosto.Code.GetValueOrDefault(-1),
                    new CntXRepBalanceSituacionInicializarResponse());
            }

            var respuesta =
                inicializacion.Result
                ?? new CntXRepBalanceSituacionInicializarResponse();

            respuesta.unidades = unidades.Result ?? [];
            respuesta.centros_costo =
                centrosCosto.Result ?? [];

            return DbHelper.CreateOkResponse(respuesta);
        }

        /// <summary>
        /// Obtiene los centros de costo asociados a una unidad.
        /// Para una unidad consolidada devuelve todos los centros.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="unidad"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>>
            CntX_frmCntX_RepBalanceSituacion_CentrosCosto_Obtener(
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
                      or @Unidad = '0x0'
                      or exists (
                          select 1
                          from CntX_Unidades_CC U
                          where U.cod_contabilidad =
                                C.cod_contabilidad
                            and U.cod_centro_costo =
                                C.cod_centro_costo
                            and U.cod_unidad = @Unidad
                      )
                  )
                order by C.descripcion;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new
                {
                    CodContabilidad = codContabilidad,
                    Unidad =
                        unidad?.Trim() ?? string.Empty
                });
        }

        /// <summary>
        /// Formatea una cuenta contable y obtiene su descripcion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="cuenta"></param>
        /// <returns></returns>
        public ErrorDto<CntXRepBalanceSituacionCuentaData?>
            CntX_frmCntX_RepBalanceSituacion_Cuenta_Obtener(
                int codEmpresa,
                int codContabilidad,
                string? cuenta)
        {
            if (codContabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse<
                    CntXRepBalanceSituacionCuentaData?>(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    null);
            }

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                return DbHelper.CreateErrorResponse<
                    CntXRepBalanceSituacionCuentaData?>(
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
                    CntXRepBalanceSituacionCuentaData?>(
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
                CntXRepBalanceSituacionCuentaData?>(
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
        /// Ejecuta el proceso que prepara los datos temporales
        /// del reporte de balance de situacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CntXRepBalanceSituacionPrepararResponse>
            CntX_frmCntX_RepBalanceSituacion_Reporte_Preparar(
                int codEmpresa,
                CntXRepBalanceSituacionPrepararRequest request)
        {
            var validacion =
                CntX_frmCntX_RepBalanceSituacion_Request_Validar(
                    request);

            if (validacion is not null)
            {
                return validacion;
            }

            string cuentaInicio =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    request.cuenta_inicio.Trim(),
                    0);

            string cuentaCorte =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    request.cuenta_corte.Trim(),
                    0);

            if (string.IsNullOrWhiteSpace(cuentaInicio)
                || string.IsNullOrWhiteSpace(cuentaCorte))
            {
                return DbHelper.CreateErrorResponse(
                    "El rango de cuentas no es v&aacute;lido.",
                    -2,
                    new CntXRepBalanceSituacionPrepararResponse());
            }

            const string sql = """
                exec spCntX_BalanceSituacion_Procesa
                    @CodContabilidad,
                    @AnioInicio,
                    @MesInicio,
                    @AnioCorte,
                    @MesCorte,
                    @CuentaInicio,
                    @CuentaCorte,
                    @Unidad,
                    @CentroCosto,
                    @Usuario;

                select
                    dbo.fxSys_FechaAnioMesToDatetime(
                        @AnioInicio,
                        @MesInicio
                    ) as fecha_inicio,
                    dbo.fxSys_FechaAnioMesToDatetime(
                        @AnioCorte,
                        @MesCorte
                    ) as fecha_corte,
                    @CuentaInicio as cuenta_inicio,
                    @CuentaCorte as cuenta_corte;
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                    connection.QuerySingle<
                        CntXRepBalanceSituacionPrepararResponse>(
                        sql,
                        new
                        {
                            CodContabilidad =
                                request.cod_contabilidad,
                            AnioInicio =
                                request.anio_inicio,
                            MesInicio =
                                request.mes_inicio,
                            AnioCorte =
                                request.anio_corte,
                            MesCorte =
                                request.mes_corte,
                            CuentaInicio = cuentaInicio,
                            CuentaCorte = cuentaCorte,
                            Unidad = NormalizarFiltro(
                                request.unidad),
                            CentroCosto = NormalizarFiltro(
                                request.centro_costo),
                            Usuario =
                                request.usuario.Trim()
                        }));
        }

        private static ErrorDto<
            CntXRepBalanceSituacionPrepararResponse>?
            CntX_frmCntX_RepBalanceSituacion_Request_Validar(
                CntXRepBalanceSituacionPrepararRequest? request)
        {
            if (request is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los datos del reporte son requeridos.",
                    -2,
                    new CntXRepBalanceSituacionPrepararResponse());
            }

            if (request.cod_contabilidad <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    "La contabilidad indicada no es v&aacute;lida.",
                    -2,
                    new CntXRepBalanceSituacionPrepararResponse());
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es requerido.",
                    -2,
                    new CntXRepBalanceSituacionPrepararResponse());
            }

            if (!EsPeriodoValido(
                    request.anio_inicio,
                    request.mes_inicio)
                || !EsPeriodoValido(
                    request.anio_corte,
                    request.mes_corte))
            {
                return DbHelper.CreateErrorResponse(
                    "El rango de per&iacute;odos no es v&aacute;lido.",
                    -2,
                    new CntXRepBalanceSituacionPrepararResponse());
            }

            if (string.IsNullOrWhiteSpace(
                    request.cuenta_inicio)
                || string.IsNullOrWhiteSpace(
                    request.cuenta_corte))
            {
                return DbHelper.CreateErrorResponse(
                    "El rango de cuentas no es v&aacute;lido.",
                    -2,
                    new CntXRepBalanceSituacionPrepararResponse());
            }

            return null;
        }

        private static bool EsPeriodoValido(
            int anio,
            int mes)
        {
            return anio is >= 1753 and <= 9999
                && mes is >= 1 and <= 13;
        }

        private static string NormalizarFiltro(
            string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? "0x0"
                : valor.Trim();
        }
    }
}