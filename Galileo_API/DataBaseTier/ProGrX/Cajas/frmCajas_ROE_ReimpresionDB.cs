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

                if (param.FechaDesde.HasValue && param.FechaHasta.HasValue)
                {
                    sql.Append(" AND FECHA BETWEEN @FechaDesde AND @FechaHasta");
                    parameters.Add("FechaDesde", param.FechaDesde.Value.Date);
                    parameters.Add("FechaHasta", param.FechaHasta.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59));
                }
                if (param.IdSesion.HasValue)
                {
                    sql.Append(" AND ID_SESION = @IdSesion");
                    parameters.Add("IdSesion", param.IdSesion.Value);
                }
                if (!string.IsNullOrWhiteSpace(param.CedulaAso) || !string.IsNullOrWhiteSpace(param.IdentificacionDepo))
                {
                    sql.Append(" AND (");
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
                    sql.Append(string.Join(" OR ", conds));
                    sql.Append(')');
                }
                if (!string.IsNullOrWhiteSpace(param.NombreDepo))
                {
                    sql.Append(" AND (NOMBRE_DEPO LIKE @NombreDepo)");
                    parameters.Add("NombreDepo", $"%{param.NombreDepo}%");
                }
                // EstadoFiltro: "Activo", "Inactivo", "PendienteActu", "PendienteImprimir"
                switch (param.EstadoFiltro)
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

                sql.Append(" ORDER BY FECHA DESC");

                return conn.Query<CajasRoeConsultaResult>(sql.ToString(), parameters).ToList();
            });
        }

        public ErrorDto<CajasRoeImprimeValidaResult?> CajasRoe_Imprime_Valida(int codEmpresa, int idRoe)
        {
            var query = "SELECT dbo.fxCajas_ROE_Imprime_Valida(@IdRoe) AS Imprime";
            var param = new { IdRoe = idRoe };
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
                conn.QueryFirstOrDefault<CajasRoeImprimeValidaResult>(query, param)
            );
        }

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
    }
}
