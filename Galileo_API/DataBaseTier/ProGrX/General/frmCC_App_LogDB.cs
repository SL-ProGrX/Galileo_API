using Dapper;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public class FrmCcAppLogDb
    {
        private readonly IConfiguration _config;

        public FrmCcAppLogDb(IConfiguration config)
        {
            _config = config;
        }

        public List<EstadisticaData> CC_Estadistica_SP(int CodEmpresa, string FechaInicio, string FechaCorte)
        {

            List<EstadisticaData> resp = new List<EstadisticaData>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                resp = ExecuteEstadisticaQuery(connection, CodEmpresa, FechaInicio, FechaCorte);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        private static List<EstadisticaData> ExecuteEstadisticaQuery(SqlConnection connection, int CodEmpresa, string FechaInicio, string FechaCorte)
        {
            var query = $@"exec spAPP_Estadistica {CodEmpresa}, '{FechaInicio} 00:00:00','{FechaCorte} 23:59:59'";
            return connection.Query<EstadisticaData>(query).ToList();
        }

        public List<EstadisticaDetalleData> CC_Estadistica_Detalle_SP(int CodEmpresa, string Codigo, string FechaInicio, string FechaCorte)
        {

            List<EstadisticaDetalleData> resp = new List<EstadisticaDetalleData>();
            try
            {
                 using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                resp = ExecuteEstadisticaDetalleQuery(connection, CodEmpresa, Codigo, FechaInicio, FechaCorte);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        private static List<EstadisticaDetalleData> ExecuteEstadisticaDetalleQuery(SqlConnection connection, int CodEmpresa, string Codigo, string FechaInicio, string FechaCorte)
        {
            var query = $@"exec spAPP_Estadistica_Detalle {CodEmpresa}, {Codigo}, '{FechaInicio} 00:00:00','{FechaCorte} 23:59:59'";
            return connection.Query<EstadisticaDetalleData>(query).ToList();
        }

        public List<EstadisticaAnalisisData> CC_Estadistica_Analisis_SP(int CodEmpresa, string FechaInicio, string FechaCorte, int Ingreso)
        {
            List<EstadisticaAnalisisData> resp = new List<EstadisticaAnalisisData>();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));
                resp = ExecuteEstadisticaAnalisisQuery(connection, CodEmpresa, FechaInicio, FechaCorte, Ingreso);
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        private static List<EstadisticaAnalisisData> ExecuteEstadisticaAnalisisQuery(SqlConnection connection, int CodEmpresa, string FechaInicio, string FechaCorte, int Ingreso)
        {
            var query = $@"exec spAPP_Estadistica_Analisis {CodEmpresa}, '{FechaInicio} 00:00:00','{FechaCorte} 23:59:59', {Ingreso}";
            return connection.Query<EstadisticaAnalisisData>(query).ToList();
        }

    }
}