using Dapper;
using Galileo.Models.GEN;
using Microsoft.Data.SqlClient;
using System.Data;

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
            List<EstadisticaData> resp = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                resp = connection.Query<EstadisticaData>(
                    "spAPP_Estadistica",
                    new
                    {
                        CodEmpresa,
                        FechaInicio = fechaInicioDt,
                        FechaCorte = fechaCorteDt
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<EstadisticaDetalleData> CC_Estadistica_Detalle_SP(int CodEmpresa, string Codigo, string FechaInicio, string FechaCorte)
        {
            List<EstadisticaDetalleData> resp = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                resp = connection.Query<EstadisticaDetalleData>(
                    "spAPP_Estadistica_Detalle",
                    new
                    {
                        CodEmpresa,
                        Codigo,
                        FechaInicio = fechaInicioDt,
                        FechaCorte = fechaCorteDt
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }

        public List<EstadisticaAnalisisData> CC_Estadistica_Analisis_SP(int CodEmpresa, string FechaInicio, string FechaCorte, int Ingreso)
        {
            List<EstadisticaAnalisisData> resp = new();
            try
            {
                using var connection = new SqlConnection(_config.GetConnectionString("BaseConnString"));

                var fechaInicioDt = DateTime.Parse(FechaInicio, System.Globalization.CultureInfo.InvariantCulture).Date;
                var fechaCorteDt = DateTime.Parse(FechaCorte, System.Globalization.CultureInfo.InvariantCulture).Date.AddDays(1).AddTicks(-1);

                resp = connection.Query<EstadisticaAnalisisData>(
                    "spAPP_Estadistica_Analisis",
                    new
                    {
                        CodEmpresa,
                        FechaInicio = fechaInicioDt,
                        FechaCorte = fechaCorteDt,
                        Ingreso
                    },
                    commandType: CommandType.StoredProcedure
                ).ToList();
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
            return resp;
        }
    }
}
