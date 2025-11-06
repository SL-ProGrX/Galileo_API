using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models.GEN;

namespace PgxAPI.DataBaseTier
{
    public class frmCC_App_LogDB
    {
        private readonly IConfiguration _config;

        public frmCC_App_LogDB(IConfiguration config)
        {
            _config = config;
        }

        public List<EstadisticaData> CC_Estadistica_SP(int CodEmpresa, string FechaInicio, string FechaCorte)
        {

            List<EstadisticaData> resp = new List<EstadisticaData>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var query = $@"exec spAPP_Estadistica {CodEmpresa}, '{FechaInicio} 00:00:00','{FechaCorte} 23:59:59'";
                    resp = connection.Query<EstadisticaData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<EstadisticaDetalleData> CC_Estadistica_Detalle_SP(int CodEmpresa, string Codigo, string FechaInicio, string FechaCorte)
        {

            List<EstadisticaDetalleData> resp = new List<EstadisticaDetalleData>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var query = $@"exec spAPP_Estadistica_Detalle {CodEmpresa}, {Codigo}, '{FechaInicio} 00:00:00','{FechaCorte} 23:59:59'";
                    resp = connection.Query<EstadisticaDetalleData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<EstadisticaAnalisisData> CC_Estadistica_Analisis_SP(int CodEmpresa, string FechaInicio, string FechaCorte, int Ingreso)
        {
            List<EstadisticaAnalisisData> resp = new List<EstadisticaAnalisisData>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                {
                    var query = $@"exec spAPP_Estadistica_Analisis {CodEmpresa}, '{FechaInicio} 00:00:00','{FechaCorte} 23:59:59', {Ingreso}";
                    resp = connection.Query<EstadisticaAnalisisData>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }
    }
}