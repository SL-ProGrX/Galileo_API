using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Cajas;
using System.Text;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cajas
{
    public class FrmCajasRoeReimpresionDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasRoeReimpresionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Consulta los registros de CAJAS_ROE según los filtros enviados.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros de filtro para la consulta.</param>
        /// <returns>Lista de registros de CAJAS_ROE que cumplen con los filtros.</returns>
        public ErrorDto<List<CajasRoeConsultaResult>> CajasRoe_Consulta(int codEmpresa, CajasRoeConsultaParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var sql = new StringBuilder(@"
                    SELECT ID_ROE, TIPOROE, rtrim(CEDULA_ASO) as CEDULA_ASO, IDENTIFICACION_DEPO, NOMBRE_DEPO, FECHA, USUARIO, MONTO_LOCAL, MONTO_DOL, TIPO_CAMBIO,
                           REGISTRO_FECHA, REGISTRO_USUARIO, ACTUALIZA_FECHA, ACTUALIZA_USUARIO, USUARIO_ANULACION, FECHA_ANULACION, OBSERV_ANULACION, IMPRIME_FECHA, IMPRIME_USUARIO,
                           ISNULL(ID_SESION,'') AS ID_SESION, ESTADO
                    FROM CAJAS_ROE
                    WHERE 1=1
                ");
                var parameters = new DynamicParameters();

                AppendFechaFiltro(sql, parameters, param);
                AppendSesionFiltro(sql, parameters, param);
                AppendCedulaIdentificacionFiltro(sql, parameters, param);
                AppendNombreDepoFiltro(sql, parameters, param);
                AppendEstadoFiltro(sql, param.EstadoFiltro);

                sql.Append(" ORDER BY FECHA DESC");

                return conn.Query<CajasRoeConsultaResult>(sql.ToString(), parameters).ToList();
            });
        }

        /// <summary>
        /// Valida si un ROE puede ser impreso según la función fxCajas_ROE_Imprime_Valida.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="idRoe">ID del ROE a validar.</param>
        /// <returns>Resultado de validación (1: puede imprimir, 0: no puede).</returns>
        public ErrorDto<CajasRoeImprimeValidaResult?> CajasRoe_Imprime_Valida(int codEmpresa, int idRoe)
        {
            var query = "SELECT dbo.fxCajas_ROE_Imprime_Valida(@IdRoe) AS Imprime";
            var param = new { IdRoe = idRoe };
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CajasRoeImprimeValidaResult>(query, param)
            );
        }

        /// <summary>
        /// Ejecuta el procedimiento spCajas_ROE_Imprime para registrar la impresión de un ROE.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="param">Parámetros con el ID del ROE y el usuario.</param>
        /// <returns>Resultado de la operación (Pass y Mensaje).</returns>
        public ErrorDto<CajasRoeImprimeResult?> CajasRoe_Imprime(int codEmpresa, CajasRoeImprimeParams param)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var dbParam = new { ROE = param.Roe, param.Usuario };
                return conn.QueryFirstOrDefault<CajasRoeImprimeResult>(
                    "spCajas_ROE_Imprime",
                    dbParam,
                    commandType: CommandType.StoredProcedure
                );
            });
        }

        // Métodos auxiliares privados para filtros (sin cambios)
        private static void AppendFechaFiltro(StringBuilder sql, DynamicParameters parameters, CajasRoeConsultaParams param)
        {
            if (param.FechaDesde.HasValue && param.FechaHasta.HasValue)
            {
                sql.Append(" AND FECHA BETWEEN @FechaDesde AND @FechaHasta");
                parameters.Add("FechaDesde", param.FechaDesde.Value.Date);
                parameters.Add("FechaHasta", param.FechaHasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
            }
        }

        private static void AppendSesionFiltro(StringBuilder sql, DynamicParameters parameters, CajasRoeConsultaParams param)
        {
            if (param.IdSesion.HasValue)
            {
                sql.Append(" AND ID_SESION = @IdSesion");
                parameters.Add("IdSesion", param.IdSesion.Value);
            }
        }

        private static void AppendCedulaIdentificacionFiltro(StringBuilder sql, DynamicParameters parameters, CajasRoeConsultaParams param)
        {
            var conds = new List<string>();
            if (!string.IsNullOrWhiteSpace(param.CedulaAso))
            {
                conds.Add("CEDULA_ASO LIKE @CedulaAso");
                parameters.Add("CedulaAso", $"%{param.CedulaAso}%");
            }
            if (!string.IsNullOrWhiteSpace(param.IdentificacionDepo))
            {
                conds.Add("IDENTIFICACION_DEPO LIKE @IdentificacionDepo");
                parameters.Add("IdentificacionDepo", $"%{param.IdentificacionDepo}%");
            }
            if (conds.Count > 0)
            {
                sql.Append(" AND (");
                sql.Append(string.Join(" OR ", conds));
                sql.Append(')');
            }
        }

        private static void AppendNombreDepoFiltro(StringBuilder sql, DynamicParameters parameters, CajasRoeConsultaParams param)
        {
            if (!string.IsNullOrWhiteSpace(param.NombreDepo))
            {
                sql.Append(" AND (NOMBRE_DEPO LIKE @NombreDepo)");
                parameters.Add("NombreDepo", $"%{param.NombreDepo}%");
            }
        }

        private static void AppendEstadoFiltro(StringBuilder sql, string? estadoFiltro)
        {
            switch (estadoFiltro)
            {
                case "Activo":
                    sql.Append(" AND ESTADO = 'A'");
                    break;
                case "Inactivo":
                    sql.Append(" AND ESTADO = 'I'");
                    break;
                case "PendienteActu":
                    sql.Append(" AND ESTADO = 'A' AND REGISTRO_FECHA IS NOT NULL AND ACTUALIZA_FECHA IS NULL");
                    break;
                case "PendienteImprimir":
                    sql.Append(" AND ESTADO = 'A' AND REGISTRO_FECHA IS NOT NULL AND IMPRIME_FECHA IS NULL");
                    break;
            }
        }
    }
}
