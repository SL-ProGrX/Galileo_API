using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX_Procesos;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Procesos
{
    public class FrmCcCuotaMantenimientoDB
    {
        private readonly PortalDB _portalDB;
        private readonly MProGrxMain mProGrxDll;
        private readonly MSecurityMainDb _securityDb;

        private const decimal Monto = 500m;
        private const string Codigo = "CMCR";

        public FrmCcCuotaMantenimientoDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config);
            _securityDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Consulta las instituciones activas habilitadas para la cuota de mantenimiento.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <returns>Instituciones CCSS y OPC, o el error de acceso a datos.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_CuotaMantenimiento_Instituciones_Obtener(int codEmpresa)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                const string sql = @"
                    SELECT
                        cod_institucion AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM instituciones
                    WHERE activa = 1
                      AND cod_institucion IN (1,2)
                    ORDER BY descripcion;";

                return conn.Query<DropDownListaGenericaModel>(sql).ToList();
            });
        }

        /// <summary>
        /// Consulta la fecha del proceso anterior; si no existe, retorna el proceso actual.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="tx">Transacción que protege el proceso completo.</param>
        /// <param name="proceso">Período actual de créditos.</param>
        /// <returns>Período anterior configurado.</returns>
        private static decimal FxFechaProcesoAnterior(IDbConnection connection, IDbTransaction tx, decimal proceso)
        {
            const string sql = @"SELECT ISNULL(dbo.fxSIFPrmProcesoAnt(@Proceso), @Proceso);";
            return connection.ExecuteScalar<decimal>(sql, new { Proceso = proceso }, tx, commandTimeout: 0);
        }


        /// <summary>
        /// Consulta la fecha del proceso siguiente; si no existe, retorna el proceso actual.
        /// </summary>
        /// <param name="connection">Conexión activa de la empresa.</param>
        /// <param name="tx">Transacción que protege el proceso completo.</param>
        /// <param name="proceso">Período actual de créditos.</param>
        /// <returns>Período siguiente configurado.</returns>
        private static decimal FxFechaProcesoSiguiente(IDbConnection connection, IDbTransaction tx, decimal proceso)
        {
            const string sql = @"SELECT ISNULL(dbo.fxSIFPrmProcesoSig(@Proceso), @Proceso);";
            return connection.ExecuteScalar<decimal>(sql, new { Proceso = proceso }, tx, commandTimeout: 0);
        }

        /// <summary>
        /// Ejecuta en una transacción el proceso de cuota de mantenimiento para una institución.
        /// </summary>
        /// <param name="request">Empresa, usuario, contabilidad e institución del proceso.</param>
        /// <returns>Resultado de la transacción o el error de validación/ejecución.</returns>
        public ErrorDto Crd_CuotaMantenimiento_Ejecutar(CcCuotaMantenimientoEjecutarRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            var usuario = request.Usuario?.Trim() ?? string.Empty;
            var validacion = ValidarParametros(request, usuario);
            if (validacion is not null)
            {
                return validacion;
            }

            if (CmdAplicar_Derecho_Obtener(request.CodEmpresa, usuario) == 0)
            {
                return DbHelper.ErrorResponse("El usuario no tiene derecho para ejecutar el proceso.", -2);
            }

            var globalesDto = mProGrxDll.sbSifParametrosInicializa(
                request.CodEmpresa,
                usuario,
                request.CodContabilidad);
            var glngFechaCR = globalesDto?.Result?.GlngFechaCR ?? 0;

            if (glngFechaCR <= 0)
            {
                return DbHelper.ErrorResponse("No fue posible obtener el período de créditos para ejecutar el proceso.", -2);
            }

            using var connection = DbHelper.OpenConnection(_portalDB, request.CodEmpresa);
            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                const string sqlFechaServidor = @"SELECT GETDATE();";
                var fechaServidor = connection.ExecuteScalar<DateTime>(sqlFechaServidor, transaction: tx, commandTimeout: 0);


                var fechaProcesoSiguiente = FxFechaProcesoSiguiente(connection, tx, glngFechaCR);
                var fechaProcesoAnterior = FxFechaProcesoAnterior(connection, tx, glngFechaCR);

                const string sqlExAsociados = @"
                        UPDATE R
                        SET Estado = 'C'
                        FROM reg_creditos R
                        INNER JOIN Socios S ON R.cedula = S.cedula
                        WHERE S.cod_institucion = @CodInstitucion
                          AND R.codigo = @Codigo
                          AND R.estado = 'A'
                          AND S.estadoActual NOT IN ('S');";

                connection.Execute(sqlExAsociados, new { request.CodInstitucion, Codigo }, tx, commandTimeout: 0);

                const string sqlActualizarActuales = @"
                UPDATE R
                SET montoapr = @Monto,
                    cuota    = @Monto,
                    saldo    = @Monto
                FROM reg_creditos R
                INNER JOIN Socios S ON R.cedula = S.cedula
                WHERE S.cod_institucion = @CodInstitucion
                  AND R.codigo = @Codigo
                  AND R.estado = 'A'
                  AND S.estadoActual = 'S';";

                connection.Execute(sqlActualizarActuales, new { request.CodInstitucion, Codigo, Monto }, tx, commandTimeout: 0);

                const string sqlInsertNuevos = @"
                        INSERT INTO reg_creditos(
                            codigo, id_comite, cedula, montosol, montoapr, monto_girado,
                            saldo, amortiza, interesc, saldo_mes, cuota, int, interesv, plazo,
                            userrec, userres, userfor, usertesoreria, tesoreria,
                            fechasol, fechares, fechaforp, fechaforf, fecha_calculo_int,
                            garantia, primer_cuota, tdocumento, ndocumento, pagare,
                            firma_deudor, premio, observacion, estado, prideduc, fecult,
                            estadosol, documento_referido
                        )
                        SELECT
                            @Codigo, 6, S.cedula, @Monto, @Monto, 0,
                            @Monto, 0, 0, @Monto, @Monto, 0, 0, 999,
                            @Usuario, @Usuario, @Usuario, @Usuario, @Usuario,
                            @FechaServidor, @FechaServidor, @FechaServidor, @FechaServidor, @FechaServidor,
                            'R', 'N', 'OT', '', 0,
                            1, 0, 'Proceso Automatico Cuota Mantenimiento CR',
                            'A',
                            @FechaProcesoSiguiente,
                            @FechaProcesoAnterior,
                            'F',
                            'AUTOMATICO'
                        FROM socios S
                        WHERE S.estadoactual = 'S'
                          AND S.cod_institucion = @CodInstitucion
                          AND S.cedula NOT IN (
                                SELECT R2.cedula
                                FROM reg_creditos R2
                                WHERE R2.estado = 'A'
                                  AND R2.codigo = @Codigo
                          );";

                connection.Execute(
                    sqlInsertNuevos,
                    new
                    {
                        request.CodInstitucion,
                        Codigo,
                        Monto,
                        Usuario = usuario,
                        FechaServidor = fechaServidor,
                        FechaProcesoSiguiente = fechaProcesoSiguiente,
                        FechaProcesoAnterior = fechaProcesoAnterior
                    },
                    tx,
                    commandTimeout: 0
                );

                const string sqlCongelamiento = @"
                    UPDATE R
                    SET Estado = 'C'
                    FROM reg_creditos R
                    INNER JOIN Socios S ON R.cedula = S.cedula
                    WHERE S.cod_institucion = @CodInstitucion
                      AND R.estado = 'A'
                      AND R.codigo = @Codigo
                      AND R.cedula IN (
                            SELECT cedula
                            FROM afi_congelar
                            WHERE estado = 'A'
                              AND fecha_finaliza >= GETDATE()
                              AND per_cobro_cuotaCr = 0
                      );";

                connection.Execute(sqlCongelamiento, new { request.CodInstitucion, Codigo }, tx, commandTimeout: 0);

                const string sqlSinAportes = @"
                    UPDATE reg_creditos
                    SET estado = 'C'
                    WHERE estado = 'A'
                      AND codigo = @Codigo
                      AND cedula IN (
                            SELECT A.cedula
                            FROM ahorro_consolidado A
                            INNER JOIN socios S ON A.cedula = S.cedula
                            WHERE S.estadoactual = 'S'
                              AND DATEDIFF(MONTH, A.fecAporte, GETDATE()) > 2
                              AND S.cod_institucion = @CodInstitucion
                      );";

                connection.Execute(sqlSinAportes, new { request.CodInstitucion, Codigo }, tx, commandTimeout: 0);

                tx.Commit();
                return DbHelper.OkResponse("Cuota de Mantenimiento Actualizada Satisfactoriamente...");
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Valida los datos mínimos antes de abrir la conexión transaccional.
        /// </summary>
        /// <param name="request">Datos recibidos para ejecutar el proceso.</param>
        /// <param name="usuario">Usuario normalizado.</param>
        /// <returns>Un error de validación o <see langword="null"/> si los datos son válidos.</returns>
        private static ErrorDto? ValidarParametros(
            CcCuotaMantenimientoEjecutarRequest request,
            string usuario)
        {
            if (request.CodEmpresa <= 0)
            {
                return DbHelper.ErrorResponse("No fue posible determinar la empresa de la sesión actual.", -2);
            }

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse("No fue posible determinar el usuario de la sesión actual.", -2);
            }

            if (request.CodContabilidad <= 0)
            {
                return DbHelper.ErrorResponse("No fue posible determinar la contabilidad de la sesión actual.", -2);
            }

            if (request.CodInstitucion is not (1 or 2))
            {
                return DbHelper.ErrorResponse("Debe seleccionar una institución válida.", -2);
            }

            return null;
        }

        /// <summary>
        /// Consulta el derecho de seguridad configurado para el botón Aplicar.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario de la sesión.</param>
        /// <returns>Valor del derecho configurado.</returns>
        public int CmdAplicar_Derecho_Obtener(int codEmpresa, string usuario)
        {
            return _securityDb.Derecho(new ParametrosAccesoDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                Modulo = 10,
                FormName = "frmCC_CuotaMantenimiento",
                Boton = "cmdAplicar"
            });
        }

    }
}
