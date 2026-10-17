using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class MFndXtaDB
    {
        private readonly PortalDB _portalDB;

        public MFndXtaDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene la descripción de un plan de fondos.
        /// </summary>
        public string fxFND_DesPlan(int codEmpresa, string vCodPlan)
        {
            if (string.IsNullOrWhiteSpace(vCodPlan))
                return string.Empty;

            const string sql = """
                SELECT TOP 1 LTRIM(RTRIM(DESCRIPCION))
                FROM FND_PLANES
                WHERE COD_PLAN = @CodPlan;
                """;

            return ObtenerDescripcion(
                codEmpresa,
                sql,
                new { CodPlan = vCodPlan.Trim() });
        }

        /// <summary>
        /// Obtiene la descripción de una operadora de fondos.
        /// </summary>
        public string fxFND_DesOperadora(int codEmpresa, int vCodOperadora)
        {
            const string sql = """
                SELECT TOP 1 LTRIM(RTRIM(DESCRIPCION))
                FROM FND_OPERADORAS
                WHERE COD_OPERADORA = @CodOperadora;
                """;

            return ObtenerDescripcion(
                codEmpresa,
                sql,
                new { CodOperadora = vCodOperadora });
        }

        private string ObtenerDescripcion(
            int codEmpresa,
            string sql,
            object parametros)
        {
            try
            {
                using SqlConnection connection = _portalDB.CreateConnection(codEmpresa);

                return connection.QueryFirstOrDefault<string>(sql, parametros)
                    ?? string.Empty;
            }
            catch (SqlException ex)
            {
                _ = ex.Message;
                return string.Empty;
            }
        }
    }
}