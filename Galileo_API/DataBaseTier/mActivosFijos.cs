using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class mActivosFijos
    {
        private readonly IConfiguration _config;
        public mActivosFijos(IConfiguration config)
        {
            _config = config;
        }

        public DateTime fxCntX_PeriodoActual(int CodEmpresa, int contabilidad)
        {


            DateTime result = new DateTime();
            CntDescripTipoAsientoDto info = new CntDescripTipoAsientoDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select dbo.fxCntX_PeriodoActual(@conta) as 'Periodo'";

                    result = connection.Query<DateTime>(query, new { conta = contabilidad }).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                throw ex;

            }

            return result;
        }

        public DateTime fxActivos_FechaUltimoCierre(int CodEmpresa)
        {


            DateTime result = new DateTime();
            CntDescripTipoAsientoDto info = new CntDescripTipoAsientoDto();
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select dbo.fxActivos_UltimoPeriodoCerrado() as 'Fecha'";
                    result = connection.Query<DateTime>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            { 
                throw ex;

            }

            return result;
        }
    }
}
