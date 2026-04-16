using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class MActivosFijos
    {
        private readonly PortalDB _portalDB;
        public MActivosFijos(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public DateTime fxCntX_PeriodoActual(int CodEmpresa, int contabilidad)
        {
            DateTime result;
           
            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var query = $@"select dbo.fxCntX_PeriodoActual(@conta) as 'Periodo'";
            result = connection.Query<DateTime>(query, new { conta = contabilidad }).FirstOrDefault();
            
            return result;
        }

        public DateTime fxActivos_FechaUltimoCierre(int CodEmpresa)
        {
           
            DateTime result;

            using var connection = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var query = $@"select dbo.fxActivos_UltimoPeriodoCerrado() as 'Fecha'";
            result = connection.Query<DateTime>(query).FirstOrDefault();

            return result;
        }
    }
}
