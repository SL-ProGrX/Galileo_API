using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class ParametrosDB
    {
        private readonly IConfiguration _config;

        public ParametrosDB(IConfiguration config)
        {
            _config = config;
        }

        public ParametrosDTO Parametros_Obtener()
        {
            ParametrosDTO param = new ParametrosDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    //var procedure = "[spPGX_TiposId_Obtener]";
                    // types = connection.Query<TipoId>(procedure, commandType: CommandType.StoredProcedure).ToList();
                    var query = "SELECT * FROM US_PARAMETROS";
                    param = connection.Query<ParametrosDTO>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return param;
        }

        public ErrorParametroDTO Parametros_Insertar(ParametrosDTO request)
        {
            ErrorParametroDTO resp = new ErrorParametroDTO();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var procedure = "[spPGX_W_Parametros_Insertar]";
                    var values = new
                    {
                        ID_PARAMETRO = request.ID_PARAMETRO,
                        KEY_LENMIN = request.KEY_LENMIN,
                        KEY_LENMAX = request.KEY_LENMAX,
                        KEY_RENEW_DAY = request.KEY_RENEW_DAY,
                        KEY_REMAIN_DAYS = request.KEY_REMAIN_DAYS,
                        KEY_HISTORY = request.KEY_HISTORY,
                        TIME_LOCK = request.TIME_LOCK,
                        KEY_INTENTOS = request.KEY_INTENTOS,
                        KEY_CAPCHAR = request.KEY_CAPCHAR,
                        KEY_SIMCHAR = request.KEY_SIMCHAR,
                        KEY_NUMCHAR = request.KEY_NUMCHAR,

                    };

                    resp.Code = connection.Query<int>(procedure, values, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    resp.Description = "Ok";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }




    }
}
