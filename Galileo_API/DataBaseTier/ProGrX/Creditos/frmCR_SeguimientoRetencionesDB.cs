using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrSeguimientoRetencionesDb
    {
        private const string MsgOperacionRequerida = "Debe indicar la operaci&oacute;n.";
        private const string MsgFechaDesembolsoRequerida = "Debe indicar la fecha de desembolso.";
        private const string MsgPriDeducRequerida = "Debe indicar la primera deducci&oacute;n.";
        private const string MsgDiaPagoRequerido = "Debe indicar el d&iacute;a de pago.";
        private const string MsgOperacionNoExiste = "No se encontr&oacute; la operaci&oacute;n.";
        private const string MsgOperacionRefundeRequerida = "Debe indicar la operaci&oacute;n a refundir.";
        private const string MsgCodigoRefundeRequerido = "Debe indicar el c&oacute;digo de la operaci&oacute;n a refundir.";
        private const string MsgSaldoInvalido = "El saldo no es v&aacute;lido.";
        private const string MsgSaldoMayorOriginal = "El saldo es mayor que el original.";
        private const string MsgMontoMayorDisponible = "El monto a refundir de la operaci&oacute;n es mayor al disponible.";
        private const string MsgRefundicionExiste = "Esta refundici&oacute;n se encuentra registrada, verifique.";
        private const string MsgGuardarError = "No fue posible registrar la refundici&oacute;n.";
        private const string MsgEliminarError = "No fue posible eliminar la refundici&oacute;n.";
        private const string MsgCargarPantallaError = "No fue posible cargar la pantalla de refundiciones de retenciones.";

        private readonly PortalDB _portalDb;
        private readonly MCobroDb _mCobroDb;

        public FrmCrSeguimientoRetencionesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mCobroDb = new MCobroDb(config);
        }

        /// <summary>
        /// Obtiene la informacion de la pantalla de seguimiento de retenciones para una operacion especifica.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CrSeguimientoRetencionesPantallaData> CR_SeguimientoRetenciones_Inicializar(
            int codEmpresa,
            CrSeguimientoRetencionesInicializarRequest request)
        {
            ErrorDto validacion = ValidarInicializar(request);
            if (validacion.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    validacion.Description ?? MsgCargarPantallaError,
                    validacion.Code ?? -2,
                    new CrSeguimientoRetencionesPantallaData());
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                CrSeguimientoRetencionesOperacionBaseData? baseData =
                    ObtenerOperacionBase(conn, request.operacion);

                if (baseData is null || string.IsNullOrWhiteSpace(baseData.cedula))
                {
                    return DbHelper.CreateErrorResponse(
                        MsgOperacionNoExiste,
                        -2,
                        new CrSeguimientoRetencionesPantallaData());
                }

                baseData.fecha_desembolso = request.fecha_desembolso;
                baseData.pri_deduc = request.pri_deduc;
                baseData.dia_pago = request.dia_pago;

                decimal disponible = CalcularDisponible(
                    codEmpresa,
                    request.operacion,
                    baseData);

                List<CrSeguimientoRetencionesOperacionData> operaciones =
                    ObtenerOperacionesRetencion(conn, baseData.cedula);

                List<CrSeguimientoRetencionesRefundicionData> refundiciones =
                    ObtenerRefundiciones(conn, request.operacion);

                return DbHelper.CreateOkResponse(new CrSeguimientoRetencionesPantallaData
                {
                    disponible = disponible,
                    editable = true,
                    operaciones = operaciones,
                    refundiciones = refundiciones
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    $"{MsgCargarPantallaError} {ex.Message}",
                    -1,
                    new CrSeguimientoRetencionesPantallaData());
            }
        }

        /// <summary>
        /// Guardar refundicion de retenciones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoRetenciones_Guardar(
            int codEmpresa,
            CrSeguimientoRetencionesGuardarRequest request)
        {
            ErrorDto validacion = ValidarGuardar(request);
            if (validacion.Code != 0)
            {
                return validacion;
            }

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

                CrSeguimientoRetencionesOperacionBaseData? baseData =
                    ObtenerOperacionBase(conn, request.operacion_base);

                if (baseData is null)
                {
                    return DbHelper.ErrorResponse(MsgOperacionNoExiste, -2);
                }

                baseData.fecha_desembolso = request.fecha_desembolso;
                baseData.pri_deduc = request.pri_deduc;
                baseData.dia_pago = request.dia_pago;

                decimal disponible = CalcularDisponible(
                    codEmpresa,
                    request.operacion_base,
                    baseData);

                decimal totalRefundir = request.saldo + request.amortizacion + request.cargos;

                if (totalRefundir > disponible)
                {
                    return DbHelper.ErrorResponse(MsgMontoMayorDisponible, -2);
                }

                if (ExisteRefundicion(conn, request.operacion_base, request.operacion_refunde))
                {
                    return DbHelper.ErrorResponse(MsgRefundicionExiste, -2);
                }

                var response = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    @"
                    insert into refunde_retencion
                    (
                        id_solicitud,
                        codigo,
                        monto,
                        mora,
                        fecha,
                        codigor,
                        id_solicitudr,
                        saldo_anterior,
                        cargos
                    )
                    values
                    (
                        @OperacionRefunde,
                        @CodigoRefunde,
                        @Saldo,
                        @Amortizacion,
                        Getdate(),
                        @CodigoBase,
                        @OperacionBase,
                        @SaldoOriginal,
                        @Cargos
                    )",
                    new
                    {
                        OperacionRefunde = request.operacion_refunde,
                        CodigoRefunde = Clean(request.codigo_refunde),
                        Saldo = request.saldo,
                        Amortizacion = request.amortizacion,
                        CodigoBase = Clean(request.codigo_base),
                        OperacionBase = request.operacion_base,
                        SaldoOriginal = request.saldo_original,
                        Cargos = request.cargos
                    });

                return response.Code == 0
                    ? DbHelper.OkResponse("OK")
                    : DbHelper.ErrorResponse(
                        $"{MsgGuardarError} {response.Description}",
                        response.Code ?? -1);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"{MsgGuardarError} {ex.Message}", -1);
            }
        }

        /// <summary>
        /// Elimina una refundicion de retencion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CR_SeguimientoRetenciones_Eliminar(
            int codEmpresa,
            CrSeguimientoRetencionesEliminarRequest request)
        {
            if (request.operacion_base <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (request.operacion_refunde <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRefundeRequerida, -2);
            }

            try
            {
                var response = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    @"
                    delete refunde_retencion
                    where id_solicitud = @OperacionRefunde
                      and id_solicitudr = @OperacionBase",
                    new
                    {
                        OperacionRefunde = request.operacion_refunde,
                        OperacionBase = request.operacion_base
                    });

                return response.Code == 0
                    ? DbHelper.OkResponse("OK")
                    : DbHelper.ErrorResponse(
                        $"{MsgEliminarError} {response.Description}",
                        response.Code ?? -1);
            }
            catch (Exception ex)
            {
                return DbHelper.ErrorResponse($"{MsgEliminarError} {ex.Message}", -1);
            }
        }

        private static ErrorDto ValidarInicializar(CrSeguimientoRetencionesInicializarRequest request)
        {
            if (request is null || request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (!request.fecha_desembolso.HasValue)
            {
                return DbHelper.ErrorResponse(MsgFechaDesembolsoRequerida, -2);
            }

            if (!request.pri_deduc.HasValue || request.pri_deduc.Value <= 0)
            {
                return DbHelper.ErrorResponse(MsgPriDeducRequerida, -2);
            }

            if (!request.dia_pago.HasValue || request.dia_pago.Value <= 0)
            {
                return DbHelper.ErrorResponse(MsgDiaPagoRequerido, -2);
            }

            return DbHelper.OkResponse("OK");
        }

        private static ErrorDto ValidarGuardar(CrSeguimientoRetencionesGuardarRequest request)
        {
            if (request is null || request.operacion_base <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRequerida, -2);
            }

            if (!request.fecha_desembolso.HasValue)
            {
                return DbHelper.ErrorResponse(MsgFechaDesembolsoRequerida, -2);
            }

            if (!request.pri_deduc.HasValue || request.pri_deduc.Value <= 0)
            {
                return DbHelper.ErrorResponse(MsgPriDeducRequerida, -2);
            }

            if (!request.dia_pago.HasValue || request.dia_pago.Value <= 0)
            {
                return DbHelper.ErrorResponse(MsgDiaPagoRequerido, -2);
            }

            if (request.operacion_refunde <= 0)
            {
                return DbHelper.ErrorResponse(MsgOperacionRefundeRequerida, -2);
            }

            if (string.IsNullOrWhiteSpace(request.codigo_refunde))
            {
                return DbHelper.ErrorResponse(MsgCodigoRefundeRequerido, -2);
            }

            if (request.saldo < 0)
            {
                return DbHelper.ErrorResponse(MsgSaldoInvalido, -2);
            }

            if (request.saldo > request.saldo_original)
            {
                return DbHelper.ErrorResponse(MsgSaldoMayorOriginal, -2);
            }

            return DbHelper.OkResponse("OK");
        }

        private static CrSeguimientoRetencionesOperacionBaseData? ObtenerOperacionBase(
            Microsoft.Data.SqlClient.SqlConnection conn,
            int operacion)
        {
            return conn.QueryFirstOrDefault<CrSeguimientoRetencionesOperacionBaseData>(
                @"
                select
                    rtrim(isnull(R.Primer_Cuota, '')) as primer_cuota,
                    rtrim(isnull(R.Garantia, '')) as garantia,
                    isnull(R.montoapr, 0) as montoapr,
                    isnull(R.cuota, 0) as cuota,
                    isnull(R.int, 0) as int_credito,
                    rtrim(isnull(C.convenio, '')) as convenio,
                    rtrim(isnull(R.cod_destino, '')) as cod_destino,
                    rtrim(isnull(R.cedula, '')) as cedula,
                    rtrim(isnull(R.codigo, '')) as codigo
                from reg_creditos R
                inner join catalogo C
                    on R.codigo = C.codigo
                where R.id_solicitud = @Operacion",
                new { Operacion = operacion });
        }

        private static List<CrSeguimientoRetencionesRefundicionData> ObtenerRefundiciones(
            Microsoft.Data.SqlClient.SqlConnection conn,
            int operacionBase)
        {
            List<CrSeguimientoRetencionesRefundicionRow> rows =
                conn.Query<CrSeguimientoRetencionesRefundicionRow>(
                    @"
                    select
                        R.id_solicitud,
                        R.codigo,
                        C.descripcion,
                        isnull(R.monto, 0) as monto,
                        isnull(R.mora, 0) as mora,
                        isnull(R.cargos, 0) as cargosdef,
                        cast(0 as decimal(18, 2)) as iva
                    from refunde_retencion R
                    inner join catalogo C
                        on R.codigo = C.codigo
                    where R.id_solicitudr = @OperacionBase",
                    new { OperacionBase = operacionBase }).ToList();

            return rows.Select(MapearRefundicion).ToList();
        }

        private static List<CrSeguimientoRetencionesOperacionData> ObtenerOperacionesRetencion(
            Microsoft.Data.SqlClient.SqlConnection conn,
            string cedula)
        {
            List<CrSeguimientoRetencionesOperacionRow> rows =
                conn.Query<CrSeguimientoRetencionesOperacionRow>(
                    @"
                    select
                        R.id_solicitud,
                        R.codigo,
                        C.descripcion,
                        isnull(R.amortiza, 0) as amortiza,
                        isnull(R.cuota, 0) as cuota,
                        isnull(R.plazo, 0) as plazo,
                        isnull(V.amortiza, 0) as mora,
                        isnull(V.cargos, 0) as cargos,
                        cast(0 as decimal(18, 2)) as iva
                    from reg_creditos R
                    inner join catalogo C
                        on R.codigo = C.codigo
                       and C.retencion = 'S'
                    left join vista_morosidad V
                        on R.id_solicitud = V.id_solicitud
                    where R.proceso <> 'J'
                      and R.estado = 'A'
                      and R.plazo < 900
                      and R.cedula = @Cedula

                    union

                    select
                        R.id_solicitud,
                        R.codigo,
                        C.descripcion,
                        isnull(R.amortiza, 0) as amortiza,
                        isnull(R.cuota, 0) as cuota,
                        0 as plazo,
                        isnull(V.amortiza, 0) as mora,
                        isnull(V.cargos, 0) as cargos,
                        cast(0 as decimal(18, 2)) as iva
                    from reg_creditos R
                    inner join catalogo C
                        on R.codigo = C.codigo
                       and C.retencion = 'S'
                    inner join vista_morosidad V
                        on R.id_solicitud = V.id_solicitud
                    where R.proceso <> 'J'
                      and R.estado = 'A'
                      and R.plazo >= 900
                      and R.cedula = @Cedula",
                    new { Cedula = Clean(cedula) }).ToList();

            return rows.Select(MapearOperacion).ToList();
        }

        private decimal CalcularDisponible(
            int codEmpresa,
            int operacionBase,
            CrSeguimientoRetencionesOperacionBaseData baseData)
        {
            decimal interes = CalcularInteresFormalizacion(codEmpresa, operacionBase, baseData);
            decimal primerCuota = CalcularPrimerCuota(baseData);
            interes = AjustarInteresPrimerCuota(baseData, interes);
            decimal poliza = CalcularPoliza(codEmpresa, baseData);

            return baseData.montoapr
                - (_mCobroDb.fxMontoEnGeneral(codEmpresa, operacionBase)
                    + interes
                    + primerCuota
                    + poliza);
        }

        private decimal CalcularInteresFormalizacion(
            int codEmpresa,
            int operacionBase,
            CrSeguimientoRetencionesOperacionBaseData baseData)
        {
            if (!_mCobroDb.fxCobraTasaFormaliza(
                    codEmpresa,
                    baseData.codigo,
                    baseData.cod_destino))
            {
                return 0m;
            }

            return _mCobroDb.fxInteresesHastaFormalizar(
                codEmpresa,
                operacionBase,
                baseData.codigo,
                baseData.fecha_desembolso,
                null,
                baseData.pri_deduc ?? 0m,
                baseData.dia_pago ?? 0);
        }

        private static decimal CalcularPrimerCuota(CrSeguimientoRetencionesOperacionBaseData baseData)
        {
            return Clean(baseData.primer_cuota).Equals("S", StringComparison.OrdinalIgnoreCase)
                ? baseData.cuota
                : 0m;
        }

        private static decimal AjustarInteresPrimerCuota(
            CrSeguimientoRetencionesOperacionBaseData baseData,
            decimal interes)
        {
            if (interes <= 0)
            {
                return interes;
            }

            if (!Clean(baseData.primer_cuota).Equals("S", StringComparison.OrdinalIgnoreCase))
            {
                return interes;
            }

            return MCobroDb.fxInteresesDiasPrimerCuota(
                baseData.fecha_desembolso ?? DateTime.Today,
                baseData.montoapr,
                baseData.int_credito);
        }

        private decimal CalcularPoliza(
            int codEmpresa,
            CrSeguimientoRetencionesOperacionBaseData baseData)
        {
            if (!Clean(baseData.garantia).Equals("F", StringComparison.OrdinalIgnoreCase)
                && Clean(baseData.convenio).Equals("N", StringComparison.OrdinalIgnoreCase))
            {
                return _mCobroDb.fxCuotaPolizaVida(codEmpresa, baseData.montoapr);
            }

            return 0m;
        }

        private static bool ExisteRefundicion(
            Microsoft.Data.SqlClient.SqlConnection conn,
            int operacionBase,
            int operacionRefunde)
        {
            int existe = conn.QueryFirstOrDefault<int>(
                @"
                select isnull(count(*), 0)
                from refunde_retencion
                where id_solicitud = @OperacionRefunde
                  and id_solicitudr = @OperacionBase",
                new
                {
                    OperacionRefunde = operacionRefunde,
                    OperacionBase = operacionBase
                });

            return existe > 0;
        }

        private static CrSeguimientoRetencionesOperacionData MapearOperacion(
            CrSeguimientoRetencionesOperacionRow row)
        {
            decimal saldo = row.plazo < 900 && row.plazo > 0
                ? (row.cuota * row.plazo) - (row.amortiza + row.mora)
                : 0m;

            return new CrSeguimientoRetencionesOperacionData
            {
                operacion = row.id_solicitud,
                codigo = row.codigo,
                descripcion = row.descripcion,
                saldo = saldo,
                mora = row.mora,
                cargos = row.cargos,
                iva = row.iva
            };
        }

        private static CrSeguimientoRetencionesRefundicionData MapearRefundicion(
            CrSeguimientoRetencionesRefundicionRow row)
        {
            return new CrSeguimientoRetencionesRefundicionData
            {
                operacion = row.id_solicitud,
                codigo = row.codigo,
                descripcion = row.descripcion,
                saldo = row.monto,
                mora = row.mora,
                cargos = row.cargosdef,
                iva = row.iva
            };
        }

        private static string Clean(string? value)
        {
            return (value ?? string.Empty).Trim();
        }
    }
}