using Dapper;
using Microsoft.Data.SqlClient;

namespace PgxAPI.DataBaseTier
{
    public class MActivosFijos
    {
        private readonly IConfiguration _config;
        public MActivosFijos(IConfiguration config)
        {
            _config = config;
        }

        public DateTime fxCntX_PeriodoActual(int CodEmpresa, int contabilidad)
        {
            DateTime result;
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            using var connection = new SqlConnection(stringConn);
            {
                var query = $@"select dbo.fxCntX_PeriodoActual(@conta) as 'Periodo'";

                result = connection.Query<DateTime>(query, new { conta = contabilidad }).FirstOrDefault();

            }

            return result;
        }

        public DateTime fxActivos_FechaUltimoCierre(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            DateTime result;

            using var connection = new SqlConnection(stringConn);
            {
                var query = $@"select dbo.fxActivos_UltimoPeriodoCerrado() as 'Fecha'";
                result = connection.Query<DateTime>(query).FirstOrDefault();
            }

            return result;
        }
    }
}
