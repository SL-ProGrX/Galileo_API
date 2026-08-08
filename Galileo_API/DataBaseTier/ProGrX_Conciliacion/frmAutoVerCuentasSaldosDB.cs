using Dapper;
using Galileo.BusinessLogic;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Conciliacion;

namespace Galileo_API.DataBaseTier.ProGrX_Conciliacion
{
    public sealed class FrmAutoVerCuentasSaldosDB
    {
        private const string PeriodoInvalidoMensaje =
            "El a&ntilde;o y mes indicados no son v&aacute;lidos.";

        private const string ResumenSelect = """
            SELECT
                anio,
                mes,
                RTRIM(cod_cuenta_mask) AS cod_cuenta_mask,
                RTRIM(descripcion) AS descripcion,
                ISNULL(saldo, 0) AS saldo,
                ISNULL(saldo_contable, 0) AS saldo_contable,
                ISNULL(diferencia, 0) AS diferencia,
                ISNULL(operaciones, 0) AS operaciones,
                RTRIM(ISNULL(currency_sim, '')) AS currency_sim,
                RTRIM(ISNULL(divisa_desc, '')) AS divisa_desc
            """;

        private const string ResumenWhere = """
            WHERE anio = @anio
              AND mes = @mes
            ORDER BY cod_cuenta_mask;
            """;

        private readonly PortalDB _portalDb;
        private readonly MCntLinkDB _cntLinkDb;

        public FrmAutoVerCuentasSaldosDB(
            IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _cntLinkDb = new MCntLinkDB(config);
        }

        /// <summary>
        /// Obtiene los ultimos 36 periodos historicos disponibles.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosPeriodoData>>
            Conciliacion_AutoVerCuentasSaldos_Periodos_Obtener(
                int codEmpresa)
        {
            const string sql = """
                SELECT TOP 36
                    id_per_historico,
                    anio,
                    mes
                FROM ase_per_historico
                ORDER BY anio DESC, mes DESC;
                """;

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosPeriodoData>(
                    _portalDb,
                    codEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene el resumen comparativo por periodo y auxiliar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosResumenData>>
            Conciliacion_AutoVerCuentasSaldos_Resumen_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosResumenQuery? request)
        {
            if (!PeriodoEsValido(request))
            {
                return CrearErrorLista<AutoVerCuentasSaldosResumenData>(
                    PeriodoInvalidoMensaje);
            }

            var sqlResumen =
                ObtenerSqlResumen(
                    request.auxiliar);

            if (sqlResumen is null)
            {
                return CrearErrorLista<AutoVerCuentasSaldosResumenData>(
                    "El auxiliar indicado no es v&aacute;lido.");
            }

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosResumenData>(
                    _portalDb,
                    codEmpresa,
                    sqlResumen,
                    new
                    {
                        request.anio,
                        request.mes
                    });
        }

        /// <summary>
        /// Obtiene la tendencia contable de la cuenta seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <param name="auxiliar"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosTendenciaData>>
            Conciliacion_AutoVerCuentasSaldos_Tendencia_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosCuentaQuery? request,
                string? auxiliar)
        {
            var validacion =
                ValidarCuentaPeriodo(
                    codEmpresa,
                    request);

            if (!validacion.valido)
            {
                return CrearErrorLista<AutoVerCuentasSaldosTendenciaData>(
                    validacion.mensaje);
            }

            var tendencia =
                NormalizarAuxiliar(auxiliar);

            if (tendencia is null)
            {
                return CrearErrorLista<AutoVerCuentasSaldosTendenciaData>(
                    "El auxiliar indicado no es v&aacute;lido.");
            }

            const string sql = """
                EXEC spSys_Aux_Tendencia_Contable
                    @anio,
                    @mes,
                    @cuenta,
                    @tendencia;
                """;

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosTendenciaData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        request.anio,
                        request.mes,
                        cuenta =
                            validacion.cuenta,
                        tendencia
                    });
        }

        /// <summary>
        /// Obtiene las asignaciones relacionadas con la cuenta seleccionada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosAsignacionData>>
            Conciliacion_AutoVerCuentasSaldos_Asignacion_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosCuentaQuery? request)
        {
            var validacion =
                ValidarCuentaPeriodo(
                    codEmpresa,
                    request);

            if (!validacion.valido)
            {
                return CrearErrorLista<AutoVerCuentasSaldosAsignacionData>(
                    validacion.mensaje);
            }

            const string sql = """
                EXEC spSys_Aux_Cta_Asigna
                    @anio,
                    @mes,
                    @cuenta;
                """;

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosAsignacionData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        request.anio,
                        request.mes,
                        cuenta =
                            validacion.cuenta
                    });
        }

        /// <summary>
        /// Obtiene las formas de pago del periodo seleccionado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosFormaPagoData>>
            Conciliacion_AutoVerCuentasSaldos_FormaPago_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosPeriodoQuery? request)
        {
            if (!PeriodoEsValido(request))
            {
                return CrearErrorLista<AutoVerCuentasSaldosFormaPagoData>(
                    PeriodoInvalidoMensaje);
            }

            const string sql = """
                EXEC spSys_Aux_Cta_Forma_Pago
                    @anio,
                    @mes;
                """;

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosFormaPagoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        request.anio,
                        request.mes
                    });
        }

        /// <summary>
        /// Obtiene los valores para comparar el auxiliar y la contabilidad.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosRevisionContableData>>
            Conciliacion_AutoVerCuentasSaldos_RevisionContable_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosCuentaQuery? request)
        {
            var validacion =
                ValidarCuentaPeriodo(
                    codEmpresa,
                    request);

            if (!validacion.valido)
            {
                return CrearErrorLista<
                    AutoVerCuentasSaldosRevisionContableData>(
                        validacion.mensaje);
            }

            const string sql = """
                EXEC spSys_Aux_Cta_Mov_Rev
                    @anio,
                    @mes,
                    @cuenta;
                """;

            return DbHelper
                .ExecuteListQuery<
                    AutoVerCuentasSaldosRevisionContableData>(
                        _portalDb,
                        codEmpresa,
                        sql,
                        new
                        {
                            request.anio,
                            request.mes,
                            cuenta =
                                validacion.cuenta
                        });
        }

        /// <summary>
        /// Obtiene los movimientos auxiliares no contabilizados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosNoContabilizadoData>>
            Conciliacion_AutoVerCuentasSaldos_NoContabilizados_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosCuentaQuery? request)
        {
            var validacion =
                ValidarCuentaPeriodo(
                    codEmpresa,
                    request);

            if (!validacion.valido)
            {
                return CrearErrorLista<
                    AutoVerCuentasSaldosNoContabilizadoData>(
                        validacion.mensaje);
            }

            const string sql = """
                EXEC spSys_Aux_Cta_Mov_No_Conta
                    @anio,
                    @mes,
                    @cuenta;
                """;

            return DbHelper
                .ExecuteListQuery<
                    AutoVerCuentasSaldosNoContabilizadoData>(
                        _portalDb,
                        codEmpresa,
                        sql,
                        new
                        {
                            request.anio,
                            request.mes,
                            cuenta =
                                validacion.cuenta
                        });
        }

        /// <summary>
        /// Obtiene las operaciones con cambios contables en el periodo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosCambioData>>
            Conciliacion_AutoVerCuentasSaldos_Cambios_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosPeriodoQuery? request)
        {
            if (!PeriodoEsValido(request))
            {
                return CrearErrorLista<AutoVerCuentasSaldosCambioData>(
                    PeriodoInvalidoMensaje);
            }

            const string sql = """
                EXEC spSys_Aux_Creditos_Cambio_Cta
                    @anio,
                    @mes;
                """;

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosCambioData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        request.anio,
                        request.mes
                    });
        }

        /// <summary>
        /// Obtiene el analitico de contabilidad o del auxiliar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosAnaliticoData>>
            Conciliacion_AutoVerCuentasSaldos_Analitico_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosAnaliticoQuery? request)
        {

            if (request is null)
            {
                return CrearErrorLista<AutoVerCuentasSaldosAnaliticoData>(
                    "La solicitud no puede ser nula.");
            }

            var validacion =
                ValidarCuentaPeriodo(
                    codEmpresa,
                    request);

            if (!validacion.valido)
            {
                return CrearErrorLista<AutoVerCuentasSaldosAnaliticoData>(
                    validacion.mensaje);
            }

            var origen =
                request.origen
                .Trim()
                .ToUpperInvariant();

            var sql =
                origen switch
                {
                    "C" => """
                        EXEC spSys_Aux_Cta_Analitico
                            @anio,
                            @mes,
                            @cuenta;
                        """,

                    "A" => """
                        EXEC spSys_Aux_Cta_Analitico_Aux
                            @anio,
                            @mes,
                            @cuenta;
                        """,

                    _ => null
                };

            if (sql is null)
            {
                return CrearErrorLista<AutoVerCuentasSaldosAnaliticoData>(
                    "El origen debe ser C para Contabilidad o A para Auxiliar.");
            }

            return DbHelper
                .ExecuteListQuery<AutoVerCuentasSaldosAnaliticoData>(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        request.anio,
                        request.mes,
                        cuenta =
                            validacion.cuenta
                    });
        }

        /// <summary>
        /// Obtiene y combina los movimientos de contabilidad y auxiliar.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<AutoVerCuentasSaldosConciliaData>>
            Conciliacion_AutoVerCuentasSaldos_ConciliaMovimientos_Obtener(
                int codEmpresa,
                AutoVerCuentasSaldosConciliaQuery? request)
        {
            var validacion =
                ValidarCuentaPeriodo(
                    codEmpresa,
                    request);

            if (!validacion.valido)
            {
                return CrearErrorLista<AutoVerCuentasSaldosConciliaData>(
                    validacion.mensaje);
            }

            var tipoMovimiento =
                request?.tipo_movimiento?
                    .Trim()
                    .ToUpperInvariant();

            if (tipoMovimiento is not ("D" or "C"))
            {
                return CrearErrorLista<AutoVerCuentasSaldosConciliaData>(
                    "El tipo de movimiento debe ser D o C.");
            }

            const string sqlContabilidad = """
                EXEC spSys_Aux_Cta_Concilia_Cnt
                    @anio,
                    @mes,
                    @cuenta,
                    @tipo_movimiento;
                """;

            const string sqlAuxiliar = """
                EXEC spSys_Aux_Cta_Concilia_Aux
                    @anio,
                    @mes,
                    @cuenta,
                    @tipo_movimiento;
                """;

            return DbHelper.WithConn(
                _portalDb,
                codEmpresa,
                connection =>
                {
                    var parameters = new
                    {
                        request.anio,
                        request.mes,
                        cuenta =
                            validacion.cuenta,
                        tipo_movimiento =
                            tipoMovimiento
                    };

                    var contabilidad =
                        connection
                            .Query<AutoVerConciliaSpData>(
                                sqlContabilidad,
                                parameters)
                            .ToList();

                    var auxiliar =
                        connection
                            .Query<AutoVerConciliaSpData>(
                                sqlAuxiliar,
                                parameters)
                            .ToList();

                    return CombinarConciliacion(
                        contabilidad,
                        auxiliar,
                        validacion.cuenta,
                        tipoMovimiento);
                });
        }

        private (
            bool valido,
            string cuenta,
            string mensaje)
            ValidarCuentaPeriodo(
                int codEmpresa,
                AutoVerCuentaBase? request)
        {
            if (!PeriodoEsValido(request))
            {
                return (
                    false,
                    string.Empty,
                    PeriodoInvalidoMensaje);
            }

            if (string.IsNullOrWhiteSpace(
                    request.cuenta))
            {
                return (
                    false,
                    string.Empty,
                    "La cuenta es requerida.");
            }

            var cuenta =
                _cntLinkDb.fxgCntCuentaFormato(
                    codEmpresa,
                    false,
                    request.cuenta.Trim(),
                    0);

            if (string.IsNullOrWhiteSpace(cuenta))
            {
                return (
                    false,
                    string.Empty,
                    "La cuenta indicada no es v&aacute;lida.");
            }

            return (
                true,
                cuenta,
                string.Empty);
        }

        private static bool PeriodoEsValido(
            AutoVerPeriodoBase? request)
        {
            return request is not null &&
                   request.anio is >= 1900 and <= 9999 &&
                   request.mes is >= 1 and <= 12;
        }

        private static string? NormalizarAuxiliar(
            string? auxiliar)
        {
            return auxiliar?
                .Trim()
                .ToUpperInvariant() switch
            {
                "CREDITOS" =>
                    "Creditos",

                "CREDITOS_CA" =>
                    "Creditos_CA",

                "CREDITOS_RC" =>
                    "Creditos_RC",

                "PRODUCTO" =>
                    "Producto",

                "PRODUCTOSUSPENSO" =>
                    "ProductoSuspenso",

                "FONDOS" =>
                    "Fondos",

                "PATRIMONIO" =>
                    "Patrimonio",

                "ACTIVOS" =>
                    "Activos",

                "INVERSIONES" =>
                    "Inversiones",

                _ => null
            };
        }

        private static string? ObtenerSqlResumen(
            string? auxiliar)
        {
            return NormalizarAuxiliar(auxiliar) switch
            {
                "Creditos" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Creditos_Comparativo_Contable"),

                "Creditos_CA" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Creditos_CA_Comparativo_Contable"),

                "Creditos_RC" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Creditos_RC_Comparativo_Contable"),

                "Producto" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Producto_Comparativo_Contable"),

                "ProductoSuspenso" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_ProductoSuspenso_Comparativo_Contable"),

                "Fondos" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Fondos_Comparativo_Contable"),

                "Patrimonio" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Patrimonio_Comparativo_Contable"),

                "Activos" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Activos_Comparativo_Contable"),

                "Inversiones" =>
                    ConstruirSqlResumen(
                        "vSys_Aux_Inversiones_Comparativo_Contable"),

                _ => null
            };
        }

        private static string ConstruirSqlResumen(
            string vista)
        {
            /*
             * La vista no proviene directamente del request.
             * Se obtiene exclusivamente de la lista blanca
             * definida en ObtenerSqlResumen.
             */
            return string.Concat(
                ResumenSelect,
                Environment.NewLine,
                "FROM ",
                vista,
                Environment.NewLine,
                ResumenWhere);
        }

        private static List<AutoVerCuentasSaldosConciliaData>
            CombinarConciliacion(
                IReadOnlyList<AutoVerConciliaSpData> contabilidad,
                IReadOnlyList<AutoVerConciliaSpData> auxiliar,
                string cuenta,
                string tipoMovimiento)
        {
            var cantidad =
                Math.Max(
                    contabilidad.Count,
                    auxiliar.Count);

            var resultado =
                new List<AutoVerCuentasSaldosConciliaData>(
                    cantidad);

            for (
                var index = 0;
                index < cantidad;
                index++)
            {
                var movimientoContable =
                    index < contabilidad.Count
                        ? contabilidad[index]
                        : null;

                var movimientoAuxiliar =
                    index < auxiliar.Count
                        ? auxiliar[index]
                        : null;

                var montoContable =
                    movimientoContable?.monto ??
                    0;

                var montoAuxiliar =
                    movimientoAuxiliar?.monto ??
                    0;

                resultado.Add(
                    new AutoVerCuentasSaldosConciliaData
                    {
                        cod_cuenta_mask =
                            movimientoContable
                                ?.cod_cuenta_mask ??
                            cuenta,

                        tipo_movimiento =
                            tipoMovimiento,

                        tipo_asiento_contable =
                            movimientoContable
                                ?.tipo_asiento ??
                            string.Empty,

                        num_asiento_contable =
                            movimientoContable
                                ?.num_asiento ??
                            string.Empty,

                        fecha_asiento_contable =
                            movimientoContable
                                ?.fecha_asiento,

                        monto_contable =
                            montoContable,

                        monto_auxiliar =
                            montoAuxiliar,

                        diferencia =
                            montoContable -
                            montoAuxiliar,

                        tipo_asiento_auxiliar =
                            movimientoAuxiliar
                                ?.tipo_asiento ??
                            string.Empty,

                        num_asiento_auxiliar =
                            movimientoAuxiliar
                                ?.num_asiento ??
                            string.Empty,

                        fecha_asiento_auxiliar =
                            movimientoAuxiliar
                                ?.fecha_asiento
                    });
            }

            return resultado;
        }

        private static ErrorDto<List<T>>
            CrearErrorLista<T>(
                string mensaje)
        {
            return DbHelper
                .CreateErrorResponse<List<T>>(
                    mensaje,
                    -2,
                    result: []);
        }

        private sealed class AutoVerConciliaSpData
        {
            public string cod_cuenta_mask { get; set; } =
                string.Empty;

            public string tipo_asiento { get; set; } =
                string.Empty;

            public string num_asiento { get; set; } =
                string.Empty;

            public DateTime? fecha_asiento { get; set; } =
                null;

            public decimal monto { get; set; } = 0;
        }
    }
}