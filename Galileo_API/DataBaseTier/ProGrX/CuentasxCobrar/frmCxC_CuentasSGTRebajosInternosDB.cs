using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.CuentasxCobrar;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasSGTRebajosInternosDB
    {
        private readonly PortalDB _portalDB;
        private readonly MSecurityMainDb _securityMainDb;
        private const string MENSAJEOPERACION = "La operación indicada no existe.";

        public FrmCxCCuentasSGTRebajosInternosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _securityMainDb = new MSecurityMainDb(config);
        }

        /// <summary>
        /// Obtiene la carga inicial del formulario de rebajos internos:
        /// contexto de la operación, cuentas del deudor y movimientos registrados.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CxCCuentasSGTRebajosInternosPantallaDto> CxC_Cuentas_SGT_Rebajos_Operacion_Obtener(
            int codEmpresa,
            int operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var contexto = ObtenerContextoOperacion(conn, operacion);

                if (contexto == null)
                    return DbHelper.CreateErrorResponse<CxCCuentasSGTRebajosInternosPantallaDto>(
                        MENSAJEOPERACION,
                        -2
                    );

                contexto.movimientosRegistrados = ObtenerMovimientosRegistrados(conn, operacion);
                contexto.cuentasDeudor = ObtenerCuentasDeudor(conn, contexto.cedula);

                return DbHelper.CreateOkResponse(contexto);
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<CxCCuentasSGTRebajosInternosPantallaDto>(
                    ex.Message,
                    -1
                );
            }
        }

        /// <summary>
        /// Obtiene el contexto general de la operación principal.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        private CxCCuentasSGTRebajosInternosPantallaDto? ObtenerContextoOperacion(
            SqlConnection conn,
            int operacion)
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
WHERE c.Operacion = @operacion;";

            return conn.QueryFirstOrDefault<CxCCuentasSGTRebajosInternosPantallaDto>(
                sql,
                new { operacion }
            );
        }

        /// <summary>
        /// Obtiene los rebajos ya registrados para la operación principal.
        /// Equivale a sbCargaRebajos de VB6.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        private List<CxCCuentaRebajoInternoDto> ObtenerMovimientosRegistrados(
            SqlConnection conn,
            int operacion)
        {
            const string sql = @"
SELECT
    R.Operacion_Aplicada AS operacionAplicada,
    RTRIM(ISNULL(X.cod_concepto, '')) AS codConcepto,
    RTRIM(ISNULL(X.cod_contrato, '')) AS codContrato,
    RTRIM(ISNULL(C.descripcion, '')) AS conceptoDesc,
    CAST(ISNULL(R.Saldo, 0) AS decimal(16,2)) AS saldo,
    CAST(ISNULL(R.Int_Cor, 0) AS decimal(16,2)) AS intCor,
    CAST(ISNULL(R.Int_Mor, 0) AS decimal(16,2)) AS intMor,
    CAST(ISNULL(R.Cargos, 0) AS decimal(16,2)) AS cargos,
    CAST(ISNULL(R.Principal, 0) AS decimal(16,2)) AS principal,
    ISNULL(R.Dias, 0) AS dias,
    ISNULL(R.Dias_Mora, 0) AS diasMora,
    RTRIM(ISNULL(X.num_documento, '')) AS numDocumento,
    RTRIM(ISNULL(G.descripcion, '')) AS contratoDesc,
    CAST(ISNULL(R.Monto, 0) AS decimal(16,2)) AS monto
FROM CxC_Cuentas_Rebajos R
INNER JOIN CxC_Cuentas X
    ON R.Operacion_Aplicada = X.Operacion
INNER JOIN CxC_Conceptos C
    ON X.cod_concepto = C.cod_concepto
LEFT JOIN CxC_Contratos G
    ON X.cod_contrato = G.cod_contrato
WHERE R.Operacion = @operacion
ORDER BY R.Operacion_Aplicada;";

            return conn.Query<CxCCuentaRebajoInternoDto>(
                sql,
                new { operacion }
            ).ToList();
        }

        /// <summary>
        /// Obtiene las cuentas activas del deudor.
        /// Equivale a sbCargaCuentas de VB6.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private List<CxCCuentaRebajoInternoDto> ObtenerCuentasDeudor(
            SqlConnection conn,
            string cedula)
        {
            const string sql = @"
EXEC spCxC_TraCuentasActivas @cedula;";

            return conn.Query<CxCCuentaRebajoInternoDto>(
                sql,
                new { cedula }
            ).ToList();
        }

        /// <summary>
        /// Obtiene el cargo por reposición de una operación aplicada.
        /// Equivale a la consulta usada en lswCuentas_Click de VB6.
        /// </summary>
        /// <param name="codEmpresa">Código de empresa.</param>
        /// <param name="operacion">Operación aplicada.</param>
        /// <returns>Monto del cargo por reposición.</returns>
        public ErrorDto<decimal> CxC_Cuentas_SGT_Rebajos_CargoReposicion_Obtener(
            int codEmpresa,
            int operacion)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                const string sql = @"
SELECT
    CAST(ISNULL(dbo.fxCxC_CuentaCargoReposicion(@operacion, NULL), 0) AS decimal(16,2)) AS Cargo;";

                var result = conn.QueryFirstOrDefault<decimal>(
                    sql,
                    new { operacion }
                );

                return DbHelper.CreateOkResponse(
                    result
                );
            }
            catch (SqlException ex)
            {
                return DbHelper.CreateErrorResponse<decimal>(
                    ex.Message
                );
            }
        }

    }
}
