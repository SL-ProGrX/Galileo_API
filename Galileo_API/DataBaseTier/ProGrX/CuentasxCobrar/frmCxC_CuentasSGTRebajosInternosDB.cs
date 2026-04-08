using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtRebajosInternosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MProGrxMain _mProGrx;
        private const string MENSAJEOPERACION = "La operación indicada no existe.";
        private const string MENSAJEEXISTE = "Esta Refundición Se encuentra Registrada VERIFIQUE...";
        private const string MENSAJEMONTO = "El monto a refundir de la operación es mayor al disponible...";

        public FrmCxCCuentasSgtRebajosInternosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _mProGrx = new MProGrxMain(config);
        }

        /// <summary>
        /// Obtiene la carga inicial del formulario de rebajos internos.
        /// Equivale a Form_Load + sbCargaRebajos + sbCargaCuentas de VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Operacion">Operación principal.</param>
        /// <returns>Información inicial de pantalla.</returns>
        public ErrorDto<CxCCuentasSgtRebajosInternosPantallaDto> CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(
            int CodEmpresa,
            int Operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var contexto = ObtenerContextoOperacion(conn, Operacion);

                if (contexto == null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasSgtRebajosInternosPantallaDto>(
                        MENSAJEOPERACION,
                        -2
                    );
                }

                contexto.movimientosRegistrados = ObtenerMovimientosRegistrados(conn, Operacion);
                contexto.cuentasDeudor = ObtenerCuentasActivas(conn, contexto.cedula);

                return DbHelper.CreateOkResponse(contexto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasSgtRebajosInternosPantallaDto>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Obtiene las cuentas activas de terceros por cédula.
        /// Equivale a sbCargaLswTerceros(vCedula) de VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Cedula">Cédula a consultar.</param>
        /// <returns>Lista de cuentas activas.</returns>
        public ErrorDto<List<CxCCuentaRebajoInternoDto>> CxC_Cuentas_SGT_Rebajos_Terceros_Obtener(
            int CodEmpresa,
            string Cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedula = (Cedula ?? string.Empty).Trim();
                var lista = ObtenerCuentasActivas(conn, cedula);

                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CxCCuentaRebajoInternoDto>>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Obtiene el cargo por reposición de una operación aplicada.
        /// Equivale a la consulta usada en lswCuentas_Click de VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Operacion">Operación aplicada.</param>
        /// <returns>Monto del cargo por reposición.</returns>
        public ErrorDto<decimal> CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(
            int CodEmpresa,
            int Operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
SELECT
    CAST(ISNULL(dbo.fxCxC_CuentaCargoReposicion(@Operacion, NULL), 0) AS decimal(16,2)) AS Cargo;";

                var result = conn.QueryFirstOrDefault<decimal>(
                    sql,
                    new { Operacion }
                );

                return DbHelper.CreateOkResponse(
                    result
                );
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<decimal>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Verifica si ya existe una refundición registrada para la operación aplicada.
        /// Equivale a fxExisteRefundicion de VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Operacion">Operación principal.</param>
        /// <param name="Operacion_Aplicada">Operación aplicada.</param>
        /// <returns>Indicador de existencia.</returns>
        public ErrorDto<bool> CxC_Cuentas_SGT_Rebajos_Existe_Obtener(
            int CodEmpresa,
            int Operacion,
            int Operacion_Aplicada)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
SELECT
    CASE
        WHEN EXISTS
        (
            SELECT 1
            FROM CxC_Cuentas_Rebajos
            WHERE Operacion = @Operacion
              AND Operacion_Aplicada = @Operacion_Aplicada
        )
        THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS Existe;";

                var result = conn.QueryFirstOrDefault<bool>(
                    sql,
                    new { Operacion, Operacion_Aplicada }
                );

                return DbHelper.CreateOkResponse(
                    result
                );
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<bool>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Guarda un rebajo interno.
        /// Equivale a sbRefunde de VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="Usuario">Usuario que registra.</param>
        /// <param name="req">Datos del rebajo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CxC_Cuentas_SGT_Rebajos_Guardar(
            int CodEmpresa,
            string Usuario,
            int Contabilidad,
            CxCCuentasSgtRebajosInternosGuardarDto req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                if (!OperacionExiste(conn, req.Operacion, tran))
                {
                    tran.Rollback();
                    return DbHelper.ErrorResponse(MENSAJEOPERACION);
                }

                if (RefundicionExiste(conn, req.Operacion, req.Operacion_Aplicada, tran))
                {
                    tran.Rollback();
                    return DbHelper.ErrorResponse(MENSAJEEXISTE);
                }

                var disponible = ObtenerDisponibleActual(conn, req.Operacion, tran);

                if (req.Monto > disponible)
                {
                    tran.Rollback();
                    return DbHelper.ErrorResponse(MENSAJEMONTO);
                }

                var globales = _mProGrx.sbSifParametrosInicializa(CodEmpresa, Usuario, Contabilidad).Result;

                decimal cargoReposicion = 0m;
                decimal cargosFinales = req.Cargos;

                if (req.AplicarCargoReposicion)
                {
                    cargoReposicion = ObtenerCargoReposicion(conn, req.Operacion_Aplicada, tran);

                    conn.Execute(
                        @"EXEC spCxC_CuentaCargoReposicion
                            @Operacion,
                            @Usuario,
                            @OficinaUnidad,
                            @OficinaCentroCosto,
                            NULL;",
                        new
                        {
                            Operacion = req.Operacion_Aplicada,
                            Usuario,
                            OficinaUnidad = globales.GOficinaUnidad,
                            OficinaCentroCosto = globales.GOficinaCentroCosto
                        },
                        tran
                    );

                    cargosFinales += cargoReposicion;
                }

                const string sqlInsert = @"
INSERT INTO CxC_Cuentas_Rebajos
(
    Operacion,
    Operacion_Aplicada,
    Monto,
    Saldo,
    Principal,
    Int_Cor,
    Int_Mor,
    Cargos,
    Dias,
    Dias_Mora
)
VALUES
(
    @Operacion,
    @Operacion_Aplicada,
    @Monto,
    @Saldo,
    @Principal,
    @Int_Cor,
    @Int_Mor,
    @Cargos,
    @Dias,
    @Dias_Mora
);";

                conn.Execute(
                    sqlInsert,
                    new
                    {
                        req.Operacion,
                        req.Operacion_Aplicada,
                        req.Monto,
                        req.Saldo,
                        req.Principal,
                        req.Int_Cor,
                        req.Int_Mor,
                        Cargos = cargosFinales,
                        req.Dias,
                        req.Dias_Mora
                    },
                    tran
                );

                tran.Commit();
                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                tran.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Elimina un rebajo interno registrado.
        /// Equivale a lswRefunde_Click de VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="req">Datos del registro a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CxC_Cuentas_SGT_Rebajos_Eliminar(
            int CodEmpresa,
            CxCCuentasSgtRebajosInternosEliminarDto req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
DELETE FROM CxC_Cuentas_Rebajos
WHERE Operacion = @Operacion
  AND Operacion_Aplicada = @Operacion_Aplicada;";

                conn.Execute(
                    sql,
                    new
                    {
                        req.Operacion,
                        req.Operacion_Aplicada
                    }
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static CxCCuentasSgtRebajosInternosPantallaDto? ObtenerContextoOperacion(
            SqlConnection conn,
            int Operacion)
        {
            const string sql = @"
SELECT
    c.Operacion AS Operacion,
    RTRIM(ISNULL(c.cedula, '')) AS Cedula,
    CAST(ISNULL(c.Monto, 0) AS decimal(16,2)) AS Monto,
    CAST(ISNULL(dbo.fxCxC_CuentaRebajos(c.Operacion, 'TOT'), 0) AS decimal(16,2)) AS Rebajos,
    CAST(ISNULL(dbo.fxCxC_CuentaIngresos(c.Operacion), 0) AS decimal(16,2)) AS Ingresos,
    CAST(
        ISNULL(c.Monto, 0)
        + ISNULL(dbo.fxCxC_CuentaIngresos(c.Operacion), 0)
        - ISNULL(dbo.fxCxC_CuentaRebajos(c.Operacion, 'TOT'), 0)
        AS decimal(16,2)
    ) AS Disponible
FROM CxC_Cuentas c
WHERE c.Operacion = @Operacion;";

            return conn.QueryFirstOrDefault<CxCCuentasSgtRebajosInternosPantallaDto>(
                sql,
                new { Operacion }
            );
        }

        private static List<CxCCuentaRebajoInternoDto> ObtenerMovimientosRegistrados(
            SqlConnection conn,
            int Operacion)
        {
            const string sql = @"
SELECT
    R.Operacion_Aplicada AS Operacion_Aplicada,
    RTRIM(ISNULL(X.cod_concepto, '')) AS cod_Concepto,
    RTRIM(ISNULL(X.cod_contrato, '')) AS ContratoCod,
    RTRIM(ISNULL(C.descripcion, '')) AS ConceptoDesc,
    CAST(ISNULL(R.Saldo, 0) AS decimal(16,2)) AS Saldo,
    CAST(ISNULL(R.Int_Cor, 0) AS decimal(16,2)) AS Int_Cor,
    CAST(ISNULL(R.Int_Mor, 0) AS decimal(16,2)) AS Int_Mor,
    CAST(ISNULL(R.Cargos, 0) AS decimal(16,2)) AS Cargos,
    CAST(ISNULL(R.Principal, 0) AS decimal(16,2)) AS Principal,
    ISNULL(R.Dias, 0) AS Dias,
    ISNULL(R.Dias_Mora, 0) AS Dias_Mora,
    RTRIM(ISNULL(X.num_documento, '')) AS Num_Documento,
    RTRIM(ISNULL(G.descripcion, '')) AS ContratoDesc,
    CAST(ISNULL(R.Monto, 0) AS decimal(16,2)) AS Monto
FROM CxC_Cuentas_Rebajos R
INNER JOIN CxC_Cuentas X
    ON R.Operacion_Aplicada = X.Operacion
INNER JOIN CxC_Conceptos C
    ON X.cod_concepto = C.cod_concepto
LEFT JOIN CxC_Contratos G
    ON X.cod_contrato = G.cod_contrato
WHERE R.Operacion = @Operacion
ORDER BY R.Operacion_Aplicada;";

            return conn.Query<CxCCuentaRebajoInternoDto>(
                sql,
                new { Operacion }
            ).ToList();
        }

        private static List<CxCCuentaRebajoInternoDto> ObtenerCuentasActivas(
            SqlConnection conn,
            string Cedula)
        {
            const string sql = @"
EXEC spCxC_TraCuentasActivas @Cedula;";

            return conn.Query<CxCCuentaRebajoInternoDto>(
                sql,
                new { Cedula }
            ).ToList();
        }

        private static bool OperacionExiste(
            SqlConnection conn,
            int Operacion,
            SqlTransaction? tran = null)
        {
            const string sql = @"
SELECT COUNT(1)
FROM CxC_Cuentas
WHERE Operacion = @Operacion;";

            return conn.ExecuteScalar<int>(
                sql,
                new { Operacion },
                tran
            ) > 0;
        }

        private static bool RefundicionExiste(
            SqlConnection conn,
            int Operacion,
            int Operacion_Aplicada,
            SqlTransaction? tran = null)
        {
            const string sql = @"
SELECT COUNT(1)
FROM CxC_Cuentas_Rebajos
WHERE Operacion = @Operacion
  AND Operacion_Aplicada = @Operacion_Aplicada;";

            return conn.ExecuteScalar<int>(
                sql,
                new
                {
                    Operacion,
                    Operacion_Aplicada
                },
                tran
            ) > 0;
        }

        private static decimal ObtenerDisponibleActual(
            SqlConnection conn,
            int Operacion,
            SqlTransaction? tran = null)
        {
            const string sql = @"
SELECT
    CAST(
        ISNULL(c.Monto, 0)
        + ISNULL(dbo.fxCxC_CuentaIngresos(c.Operacion), 0)
        - ISNULL(dbo.fxCxC_CuentaRebajos(c.Operacion, 'TOT'), 0)
        AS decimal(16,2)
    ) AS Disponible
FROM CxC_Cuentas c
WHERE c.Operacion = @Operacion;";

            return conn.ExecuteScalar<decimal>(
                sql,
                new { Operacion },
                tran
            );
        }

        private static decimal ObtenerCargoReposicion(
            SqlConnection conn,
            int Operacion,
            SqlTransaction? tran = null)
        {
            const string sql = @"
SELECT
    CAST(ISNULL(dbo.fxCxC_CuentaCargoReposicion(@Operacion, NULL), 0) AS decimal(16,2));";

            return conn.ExecuteScalar<decimal>(
                sql,
                new { Operacion },
                tran
            );
        }

    }
}
