using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.ERROR;
using System.Data;

namespace Galileo.DataBaseTier
{

    public class PortalWebAppSincronizaDb
    {
        private readonly IConfiguration _config;

        public PortalWebAppSincronizaDb(IConfiguration config)
        {
            _config = config;
        }

        public ErrorDto ServidorPrincipalSincronizar()
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    connection.QueryFirst<int>("spPersona_Portal_Sincroniza", commandType: CommandType.StoredProcedure);
                    resp.Code = 1;
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

        public ErrorDto SincronizarWebApps(int paso, string server)
        {
            ErrorDto resp = new ErrorDto();
            try
            {
                if (paso == 1)
                {
                    using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                    {
                        var values = new
                        {
                            Paso = 1,
                            Empresa = 0,
                            Cedula = ""
                        };
                        // Lógica para el Paso 1
                        connection.Execute("spPortal_Sincroniza_WebApps", values, commandType: CommandType.StoredProcedure);
                        resp.Code = 1;
                        resp.Description = "Paso 1 ok";
                    }
                }
                else if (paso == 2)
                {

                    string connectionString = $"PROVIDER=MSDASQL;Driver={{SQL Server}};Server={server};" +
                       $"Database=PGX_BASE;APP=PGX_Portal;tcp:{server};";

                    using (var connection = new SqlConnection(connectionString))
                    {
                        string strSQL = "SELECT * FROM vPortal_AppWeb_Sincronizar";
                        var data = connection.Query(strSQL).ToList();

                        foreach (var item in data)
                        {
                            string cedula = item.Cedula.ToString();
                            int codEmpresa = Convert.ToInt32(item.cod_Empresa);
                            var values = new
                            {
                                Paso = 2,
                                Empresa = codEmpresa,
                                Cedula = cedula
                            };
                            // Realizar la lógica de ;sincronización con los datos obtenidos
                            connection.Execute("spPortal_Sincroniza_WebApps", values, commandType: CommandType.StoredProcedure);
                        }
                        resp.Code = 1;
                        resp.Description = "Paso 2 ok";
                    }
                }
                else if (paso == 3)
                {
                    using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                    {
                        var values = new
                        {
                            Paso = 3,
                            Empresa = 0,
                            Cedula = ""
                        };
                        connection.Execute("spPortal_Sincroniza_WebApps", values, commandType: CommandType.StoredProcedure);
                        resp.Code = 1;
                        resp.Description = "Paso 3 ok";
                    }
                }
                else
                {
                    resp.Code = 0;
                    resp.Description = "Paso no válido";
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
                throw;
            }


            return resp;
        }

    }
}