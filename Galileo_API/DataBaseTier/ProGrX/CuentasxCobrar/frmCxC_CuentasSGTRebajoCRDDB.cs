using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSgtRebajoCrdDB
    {
        private readonly PortalDB _portalDB;
        private const string MENSAJEOPERACION = "La operación indicada no existe.";
        private const string MENSAJEEXISTE = "Esta Refundición Se encuentra Registrada VERIFIQUE...";
        private const string MENSAJEMONTO = "El monto a refundir de la operación es mayor al disponible...";

        public FrmCxCCuentasSgtRebajoCrdDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la carga inicial del formulario de rebajo a créditos.
        /// Equivale a Form_Load + sbCargaRebajos + sbCargaPrestamos de VB6.
        /// </summary>
        public ErrorDto<CxCCuentasSgtRebajoCrdPantallaDto> CxC_Cuentas_SGT_Rebajo_CRD_Operacion_Obtener(
            int CodEmpresa,
            int Operacion,
            int Cta_Pendientes)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var contexto = ObtenerContextoOperacion(conn, Operacion);

                if (contexto == null)
                {
                    return DbHelper.CreateErrorResponse<CxCCuentasSgtRebajoCrdPantallaDto>(
                        MENSAJEOPERACION,
                        -2
                    );
                }

                contexto.movimientosRegistrados = ObtenerMovimientosRegistrados(conn, Operacion);
                contexto.creditosDeudor = ObtenerCreditosPersona(conn, contexto.cedula, Cta_Pendientes);

                return DbHelper.CreateOkResponse(contexto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasSgtRebajoCrdPantallaDto>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Obtiene los créditos de terceros por cédula.
        /// Equivale a sbCargaLswTerceros(vCedula) de VB6.
        /// </summary>
        public ErrorDto<List<CxCCuentaRebajoCrdDto>> CxC_Cuentas_SGT_Rebajo_CRD_Terceros_Obtener(
            int CodEmpresa,
            string Cedula,
            int Cta_Pendientes)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                var cedula = (Cedula ?? string.Empty).Trim();
                var lista = ObtenerCreditosPersona(conn, cedula, Cta_Pendientes);
                return DbHelper.CreateOkResponse(lista);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<List<CxCCuentaRebajoCrdDto>>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Valida si ya existe un rebajo a crédito registrado para la operación y solicitud.
        /// Equivale a fxExisteRefundicion de VB6.
        /// </summary>
        public ErrorDto<bool> CxC_Cuentas_SGT_Rebajo_CRD_Existe_Obtener(
            int CodEmpresa,
            int Operacion,
            int Id_Solicitud)
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
            FROM CxC_Cuentas_Rebajos_Crd
            WHERE Operacion = @Operacion
              AND id_solicitud = @Id_Solicitud
        )
        THEN CAST(1 AS bit)
        ELSE CAST(0 AS bit)
    END AS Existe;";

                var result = conn.QueryFirstOrDefault<bool>(
                    sql,
                    new { Operacion, Id_Solicitud }
                );

                return DbHelper.CreateOkResponse(result);
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
        /// Guarda un rebajo a crédito.
        /// Equivale a sbRefunde de VB6.
        /// </summary>
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Guardar(
            int CodEmpresa,
            CxCCuentasSgtRebajoCrdGuardarDto req)
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

                if (RefundicionExiste(conn, req.Operacion, req.Id_Solicitud, tran))
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

                const string sqlInsert = @"
INSERT INTO CxC_Cuentas_Rebajos_Crd
(
    Operacion,
    id_solicitud,
    Monto,
    Int_Cor,
    Int_Mor,
    Principal,
    cargos,
    Saldo,
    Poliza,
    CTA_PENDIENTES
)
VALUES
(
    @Operacion,
    @Id_Solicitud,
    @Monto,
    @Int_Cor,
    @Int_Mor,
    @Principal,
    @Cargos,
    @Saldo,
    @Poliza,
    @Cta_Pendientes
);";

                conn.Execute(
                    sqlInsert,
                    new
                    {
                        req.Operacion,
                        req.Id_Solicitud,
                        req.Monto,
                        req.Int_Cor,
                        req.Int_Mor,
                        req.Principal,
                        req.Cargos,
                        req.Saldo,
                        req.Poliza,
                        req.Cta_Pendientes
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
        /// Elimina un rebajo a crédito registrado.
        /// Equivale a lswRefunde_ItemClick de VB6.
        /// </summary>
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Eliminar(
            int CodEmpresa,
            CxCCuentasSgtRebajoCrdEliminarDto req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                const string sql = @"
DELETE FROM CxC_Cuentas_Rebajos_Crd
WHERE Operacion = @Operacion
  AND id_solicitud = @Id_Solicitud;";

                conn.Execute(
                    sql,
                    new
                    {
                        req.Operacion,
                        req.Id_Solicitud
                    }
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        /// <summary>
        /// Ejecuta la actualización de créditos asociada al formulario.
        /// Equivale a btnActualizar_Click de VB6.
        /// </summary>
        public ErrorDto CxC_Cuentas_SGT_Rebajo_CRD_Actualizar(
            int CodEmpresa,
            CxCCuentasSgtRebajoCrdActualizarDto req)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);

            try
            {
                conn.Execute(
                    "EXEC spCxC_TraCrdRefActualiza @Operacion, @CtaPendientes;",
                    new
                    {
                        Operacion = req.Operacion,
                        CtaPendientes = req.Cta_Pendientes
                    }
                );

                return DbHelper.CreateOkResponse();
            }
            catch (SqlException ex)
            {
                return DbHelper.ErrorResponse(ex.Message);
            }
        }

        private static CxCCuentasSgtRebajoCrdPantallaDto? ObtenerContextoOperacion(
            SqlConnection conn,
            int Operacion)
        {
            const string sql = @"
SELECT
    c.Operacion AS operacion,
    RTRIM(ISNULL(c.cedula, '')) AS cedula,
    CAST(ISNULL(c.Monto, 0) AS decimal(16,2)) AS monto,
    CAST(ISNULL(dbo.fxCxC_CuentaRebajos(c.Operacion, 'TOT'), 0) AS decimal(16,2)) AS rebajosTotales,
    CAST(ISNULL(dbo.fxCxC_CuentaIngresos(c.Operacion), 0) AS decimal(16,2)) AS ingresosTotales,
    CAST(
        ISNULL(c.Monto, 0)
        + ISNULL(dbo.fxCxC_CuentaIngresos(c.Operacion), 0)
        - ISNULL(dbo.fxCxC_CuentaRebajos(c.Operacion, 'TOT'), 0)
        AS decimal(16,2)
    ) AS disponible
FROM CxC_Cuentas c
WHERE c.Operacion = @Operacion;";

            return conn.QueryFirstOrDefault<CxCCuentasSgtRebajoCrdPantallaDto>(
                sql,
                new { Operacion }
            );
        }

        private static List<CxCCuentaRebajoCrdDto> ObtenerMovimientosRegistrados(
            SqlConnection conn,
            int Operacion)
        {
            const string sql = @"
SELECT
    R.Operacion AS operacion,
    R.id_solicitud AS id_Solicitud,
    RTRIM(ISNULL(X.codigo, '')) AS codigo,
    RTRIM(ISNULL(G.descripcion, '')) AS garantiaX,
    RTRIM(ISNULL(C.descripcion, '')) AS descripcion,
    CAST(ISNULL(R.Saldo, 0) AS decimal(16,2)) AS saldo,
    CAST(ISNULL(R.Int_Cor, 0) AS decimal(16,2)) AS int_Cor,
    CAST(ISNULL(R.Int_Mor, 0) AS decimal(16,2)) AS int_Mor,
    CAST(ISNULL(R.Principal, 0) AS decimal(16,2)) AS principal,
    CAST(ISNULL(R.Cargos, 0) AS decimal(16,2)) AS cargos,
    CAST(ISNULL(R.Poliza, 0) AS decimal(16,2)) AS poliza,
    CAST(ISNULL(R.Monto, 0) AS decimal(16,2)) AS monto,
    ISNULL(R.CTA_PENDIENTES, 0) AS cta_Pendientes
FROM CxC_Cuentas_Rebajos_Crd R
INNER JOIN Reg_Creditos X
    ON R.id_solicitud = X.id_solicitud
INNER JOIN Catalogo C
    ON X.codigo = C.codigo
INNER JOIN crd_garantia_tipos G
    ON X.garantia = G.garantia
WHERE R.Operacion = @Operacion
ORDER BY R.id_solicitud;";

            return conn.Query<CxCCuentaRebajoCrdDto>(
                sql,
                new { Operacion }
            ).ToList();
        }

        private static List<CxCCuentaRebajoCrdDto> ObtenerCreditosPersona(
            SqlConnection conn,
            string Cedula,
            int Cta_Pendientes)
        {
            const string sql = @"
EXEC spCrdSGTListaCreditosPersona @Cedula, 'N', @SGT;";

            return conn.Query<CxCCuentaRebajoCrdDto>(
                sql,
                new
                {
                    Cedula = Cedula,
                    SGT = (Cta_Pendientes == 1) ? 'S': 'N'
                }
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
            int Id_Solicitud,
            SqlTransaction? tran = null)
        {
            const string sql = @"
SELECT COUNT(1)
FROM CxC_Cuentas_Rebajos_Crd
WHERE Operacion = @Operacion
  AND id_solicitud = @Id_Solicitud;";

            return conn.ExecuteScalar<int>(
                sql,
                new
                {
                    Operacion,
                    Id_Solicitud
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
    }
}
