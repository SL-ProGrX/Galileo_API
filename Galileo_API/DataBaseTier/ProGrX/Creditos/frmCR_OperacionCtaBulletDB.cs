using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public sealed class FrmCrOperacionCtaBulletDb
    {
        private const string MensajeOperacionRequerida =
            "Debe indicar la operacion.";

        private const string MensajeOperacionNoExiste =
            "No se encontro la operacion indicada.";

        private const string MensajeUsuarioRequerido =
            "Debe indicar el usuario.";

        private const string MensajeConsultaError =
            "No fue posible consultar la informacion de la operacion.";

        private const string MensajeGuardarError =
            "No fue posible establecer la cuota Bullet.";

        private const string ConsultaOperacionSql = """
            SELECT
                R.id_solicitud AS operacion,
                RTRIM(ISNULL(S.cedula, '')) AS cedula,
                RTRIM(ISNULL(S.nombre, '')) AS nombre,
                RTRIM(ISNULL(R.codigo, '')) AS codigo,
                RTRIM(ISNULL(C.descripcion, '')) AS descripcion,
                RTRIM(ISNULL(Ofi.descripcion, '')) AS oficina,
                ISNULL(R.montoapr, 0) AS montoapr,
                ISNULL(R.saldo, 0) AS saldo,
                ISNULL(R.interesv, 0) AS interesv,
                ISNULL(R.[int], 0) AS tasa_o,
                R.estado AS estado,
                RTRIM(ISNULL(R.base_calculo, '')) AS base_calculo,
                ISNULL(
                    dbo.fxCrdPlanPagoPlzRestante(R.id_solicitud),
                    0
                ) AS plazo_restante,
                ISNULL(
                    dbo.fxCrdPlanPagoSldPendientePrg(R.id_solicitud),
                    0
                ) AS saldo_plan,
                ISNULL(R.BULLET_CTA, 0) AS bullet_cta,
                ISNULL(R.BULLET_CTA_AJUSTE, 1) AS bullet_ajuste
            FROM dbo.Socios AS S
            INNER JOIN dbo.Reg_creditos AS R
                ON S.cedula = R.cedula
            INNER JOIN dbo.catalogo AS C
                ON R.codigo = C.codigo
            LEFT JOIN dbo.SIF_Oficinas AS Ofi
                ON R.cod_oficina_r = Ofi.cod_oficina
            WHERE R.id_solicitud = @Operacion;
            """;

        private readonly PortalDB _portalDb;
        private readonly MProGrxMain _mProGrxMain;

        public FrmCrOperacionCtaBulletDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            _mProGrxMain = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene los datos y parametros actuales de la cuota Bullet.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CrOperacionCtaBulletData>
            CrOperacionCtaBullet_Operacion_Obtener(
                int codEmpresa,
                int operacion)
        {
            if (operacion <= 0)
            {
                return CrearErrorConsulta(
                    MensajeOperacionRequerida,
                    -2);
            }

            ErrorDto<CrOperacionCtaBulletRow?> consulta =
                ConsultarOperacion(
                    codEmpresa,
                    operacion);

            if (consulta.Code != 0)
            {
                return CrearErrorConsulta(
                    consulta.Description ?? MensajeConsultaError,
                    consulta.Code ?? -1);
            }

            if (consulta.Result is not { operacion: > 0 } fila)
            {
                return CrearErrorConsulta(
                    MensajeOperacionNoExiste,
                    -2);
            }

            return DbHelper.CreateOkResponse(
                CrearRespuesta(fila));
        }

        /// <summary>
        /// Establece o actualiza la cuota Bullet de una operacion.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CrOperacionCtaBullet_Guardar(
            int codEmpresa,
            CrOperacionCtaBulletGuardarRequest request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "No se recibieron los datos de la cuota Bullet.",
                    -2);
            }

            ErrorDto? validacion = ValidarRequest(request);

            if (validacion is not null)
            {
                return validacion;
            }

            string usuario = request.usuario.Trim();

            var globales =
                _mProGrxMain.sbSifParametrosInicializa(
                    codEmpresa,
                    usuario);

            if (globales.Code != 0 || globales.Result is null)
            {
                return DbHelper.ErrorResponse(
                    globales.Description ??
                        "No fue posible obtener los parametros globales.",
                    globales.Code ?? -1);
            }

            try
            {
                using var connection =
                    DbHelper.OpenConnection(
                        _portalDb,
                        codEmpresa);

                using var transaction =
                    connection.BeginTransaction();

                try
                {
                    CrOperacionCtaBulletRow? operacion =
                        ConsultarOperacionEnTransaccion(
                            connection,
                            transaction,
                            request.operacion);

                    if (operacion is not { operacion: > 0 })
                    {
                        return DbHelper.ErrorResponse(
                            MensajeOperacionNoExiste,
                            -2);
                    }

                    CrOperacionCtaBulletData datos =
                        CrearRespuesta(operacion);

                    validacion =
                        ValidarCuota(
                            request,
                            datos);

                    if (validacion is not null)
                    {
                        return validacion;
                    }

                    ActualizarCuotaBullet(
                        connection,
                        transaction,
                        request);

                    if (globales.Result.SysPlanPagos == 1 &&
                        datos.activa)
                    {
                        RegenerarPlanPagos(
                            connection,
                            transaction,
                            request.operacion);
                    }

                    RegistrarBitacora(
                        connection,
                        transaction,
                        request,
                        operacion);

                    transaction.Commit();

                    return DbHelper.CreateOkResponse();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch (Exception ex)
                when (ex is SqlException or InvalidOperationException)
            {
                return DbHelper.ErrorResponse(
                    $"{MensajeGuardarError} {ex.Message}",
                    -1);
            }
        }

        private ErrorDto<CrOperacionCtaBulletRow?> ConsultarOperacion(
            int codEmpresa,
            int operacion)
        {
            return DbHelper.ExecuteSingleQuery(
                _portalDb,
                codEmpresa,
                ConsultaOperacionSql,
                new CrOperacionCtaBulletRow(),
                new
                {
                    Operacion = operacion
                });
        }

        private static CrOperacionCtaBulletRow?
            ConsultarOperacionEnTransaccion(
                SqlConnection connection,
                SqlTransaction transaction,
                int operacion)
        {
            return connection.QueryFirstOrDefault<CrOperacionCtaBulletRow>(
                ConsultaOperacionSql,
                new
                {
                    Operacion = operacion
                },
                transaction);
        }

        private static CrOperacionCtaBulletData CrearRespuesta(
            CrOperacionCtaBulletRow fila)
        {
            bool activa = fila.estado is not null;

            decimal saldoBase =
                activa
                    ? fila.saldo_plan
                    : fila.montoapr;

            decimal cuotaMinima =
                CalcularCuotaMinima(
                    saldoBase,
                    fila.interesv,
                    fila.base_calculo);

            return new CrOperacionCtaBulletData
            {
                operacion = fila.operacion,
                cedula = fila.cedula,
                nombre = fila.nombre,
                codigo = fila.codigo,
                descripcion = fila.descripcion,
                oficina = fila.oficina,
                monto = fila.montoapr,
                saldo_real = fila.saldo,
                saldo_base = saldoBase,
                plazo_restante = fila.plazo_restante,
                tasa_actual = fila.interesv,
                tasa_original = fila.tasa_o,
                cuota_bullet_actual = fila.bullet_cta,
                ajuste_actual = fila.bullet_ajuste,
                cuota_bullet = cuotaMinima,
                ajuste = fila.bullet_ajuste,
                cuota_minima = cuotaMinima,
                activa = activa
            };
        }

        private static decimal CalcularCuotaMinima(
            decimal saldoBase,
            decimal tasa,
            string baseCalculo)
        {
            int dias =
                baseCalculo.Trim() == "04"
                    ? 31
                    : 30;

            decimal cuota =
                saldoBase *
                dias *
                tasa /
                36000m;

            return decimal.Round(
                cuota,
                2,
                MidpointRounding.ToEven);
        }

        private static ErrorDto? ValidarRequest(
            CrOperacionCtaBulletGuardarRequest request)
        {
            if (request.operacion <= 0)
            {
                return DbHelper.ErrorResponse(
                    MensajeOperacionRequerida,
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    MensajeUsuarioRequerido,
                    -2);
            }

            return null;
        }

        private static ErrorDto? ValidarCuota(
            CrOperacionCtaBulletGuardarRequest request,
            CrOperacionCtaBulletData datos)
        {
            List<string> errores = [];

            if (request.ajuste > datos.plazo_restante)
            {
                errores.Add(
                    "El periodo de ajuste es mayor que el plazo restante.");
            }

            if (request.cuota_bullet < datos.cuota_minima)
            {
                errores.Add(
                    "La cuota Bullet es menor que la cuota minima aplicable.");
            }

            if (request.cuota_bullet > datos.saldo_base)
            {
                errores.Add(
                    "La cuota Bullet es mayor que el saldo base.");
            }

            if (errores.Count == 0)
            {
                return null;
            }

            string mensaje =
                string.Join(
                    Environment.NewLine,
                    errores.Select(
                        error => $" - {error}"));

            return DbHelper.ErrorResponse(
                mensaje,
                -2);
        }

        private static void ActualizarCuotaBullet(
            SqlConnection connection,
            SqlTransaction transaction,
            CrOperacionCtaBulletGuardarRequest request)
        {
            const string query = """
                UPDATE dbo.reg_creditos
                SET
                    BULLET_IND = 1,
                    BULLET_CTA = @CuotaBullet,
                    BULLET_CTA_AJUSTE = @Ajuste
                WHERE id_solicitud = @Operacion;
                """;

            int registros =
                connection.Execute(
                    query,
                    new
                    {
                        CuotaBullet = request.cuota_bullet,
                        Ajuste = request.ajuste,
                        Operacion = request.operacion
                    },
                    transaction);

            if (registros == 0)
            {
                throw new InvalidOperationException(
                    MensajeOperacionNoExiste);
            }
        }

        private static void RegenerarPlanPagos(
            SqlConnection connection,
            SqlTransaction transaction,
            int operacion)
        {
            const string query = """
                EXEC dbo.spCrdPlanPagos
                     @Operacion,
                     1;
                """;

            connection.Execute(
                query,
                new
                {
                    Operacion = operacion
                },
                transaction);
        }

        private static void RegistrarBitacora(
            SqlConnection connection,
            SqlTransaction transaction,
            CrOperacionCtaBulletGuardarRequest request,
            CrOperacionCtaBulletRow operacion)
        {
            const string query = """
                INSERT INTO dbo.credito_subit
                (
                    usuario,
                    tipo,
                    fecha,
                    movimiento,
                    detalle,
                    id_solicitud,
                    codigo,
                    notas
                )
                VALUES
                (
                    @Usuario,
                    'C',
                    GETDATE(),
                    '22',
                    @Detalle,
                    @Operacion,
                    @Codigo,
                    ''
                );
                """;

            connection.Execute(
                query,
                new
                {
                    Usuario =
                        request.usuario
                            .Trim()
                            .ToUpperInvariant(),
                    Detalle =
                        CrearDetalleBitacora(
                            operacion,
                            request),
                    Operacion = request.operacion,
                    Codigo =
                        operacion.codigo
                            .Trim()
                            .ToUpperInvariant()
                },
                transaction);
        }

        private static string CrearDetalleBitacora(
            CrOperacionCtaBulletRow operacion,
            CrOperacionCtaBulletGuardarRequest request)
        {
            string cuotaAnterior =
                operacion.bullet_cta.ToString(
                    "N2",
                    CultureInfo.CurrentCulture);

            string cuotaNueva =
                request.cuota_bullet.ToString(
                    "N2",
                    CultureInfo.CurrentCulture);

            return
                $"De.:{cuotaAnterior} " +
                $"(Aj.{operacion.bullet_ajuste}) " +
                $"A..:{cuotaNueva} " +
                $"(Aj.{request.ajuste}";
        }

        private static ErrorDto<CrOperacionCtaBulletData>
            CrearErrorConsulta(
                string mensaje,
                int codigo)
        {
            return DbHelper.CreateErrorResponse(
                mensaje,
                codigo,
                new CrOperacionCtaBulletData());
        }
    }
}