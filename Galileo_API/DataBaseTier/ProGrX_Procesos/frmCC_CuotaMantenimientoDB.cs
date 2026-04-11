using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.Security;
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
        /// Consulta el listado de instituciones
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
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
        /// Consulta la fecha del proceso anterior dado un proceso actual, si no encuentra un proceso anterior retorna el mismo proceso actual
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tx"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        private static decimal FxFechaProcesoAnterior(IDbConnection connection, IDbTransaction tx, decimal proceso)
        {
            const string sql = @"SELECT ISNULL(dbo.fxSIFPrmProcesoAnt(@Proceso), @Proceso);";
            return connection.ExecuteScalar<decimal>(sql, new { Proceso = proceso }, tx);
        }


        /// <summary>
        /// Consulta la fecha del proceso siguiente dado un proceso actual, si no encuentra un proceso siguiente retorna el mismo proceso actual
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tx"></param>
        /// <param name="proceso"></param>
        /// <returns></returns>
        private static decimal FxFechaProcesoSiguiente(IDbConnection connection, IDbTransaction tx, decimal proceso)
        {
            const string sql = @"SELECT ISNULL(dbo.fxSIFPrmProcesoSig(@Proceso), @Proceso);";
            return connection.ExecuteScalar<decimal>(sql, new { Proceso = proceso }, tx);
        }

        /// <summary>
        /// Ejecuta el proceso de cuota de mantenimiento para los socios activos de una institución, el proceso se encarga de:
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="codInstitucion"></param>
        /// <returns></returns>
        public ErrorDto Crd_CuotaMantenimiento_Ejecutar(int codEmpresa, string usuario, int codContabilidad, int codInstitucion)
        {
            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);
            connection.Open();

            using var tx = connection.BeginTransaction();

            try
            {
                var globalesDto = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad);
                var glngFechaCR = globalesDto?.Result?.GlngFechaCR ?? 0;

                if (glngFechaCR <= 0)
                {
                    tx.Rollback();
                    return DbHelper.ErrorResponse("No fue posible obtener el período (GlngFechaCR) para ejecutar el proceso.");
                    
                }

                const string sqlFechaServidor = @"SELECT GETDATE();";
                var fechaServidor = connection.ExecuteScalar<DateTime>(sqlFechaServidor, transaction: tx);


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

                connection.Execute(sqlExAsociados, new { CodInstitucion = codInstitucion, Codigo }, tx);

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

                connection.Execute(sqlActualizarActuales, new { CodInstitucion = codInstitucion, Codigo, Monto }, tx);

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
                        CodInstitucion = codInstitucion,
                        Codigo,
                        Monto,
                        Usuario = usuario,
                        FechaServidor = fechaServidor,
                        FechaProcesoSiguiente = fechaProcesoSiguiente,
                        FechaProcesoAnterior = fechaProcesoAnterior
                    },
                    tx
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

                connection.Execute(sqlCongelamiento, new { CodInstitucion = codInstitucion, Codigo }, tx);

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

                connection.Execute(sqlSinAportes, new { CodInstitucion = codInstitucion, Codigo }, tx);

                tx.Commit();
                return DbHelper.OkResponse("Cuota de Mantenimiento Actualizada Satisfactoriamente...");
            }
            catch (Exception ex)
            {
                tx.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

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