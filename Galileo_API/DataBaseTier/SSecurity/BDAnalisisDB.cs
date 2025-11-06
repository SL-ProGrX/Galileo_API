using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.Security;

namespace PgxAPI.DataBaseTier
{
    public class BDAnalisisDB
    {
        private readonly IConfiguration _config;

        public BDAnalisisDB(IConfiguration config)
        {
            _config = config;
        }

        public List<string> TablasCargar(string Connectionstring)
        {
            List<string> resp = new List<string>();
            try
            {
                using (var connection = new SqlConnection(Connectionstring))
                {

                    var strSQL = "select name  from sys.objects "
                                   + " where type = 'U' "
                                   + "'order by name";

                    resp = connection.Query<string>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                resp = null;
            }
            return resp;
        }

        //private void cbo_Click(string text)
        //{
        //    string pDataBase = "" , pServer = "", pUser, pKey;

        //    switch (text)
        //    {
        //        case "CODEAS":
        //            pDataBase = "CODEAS_Migra";
        //            pServer = "progrx.centralus.cloudapp.azure.com";
        //            pUser = "Sys_Migracion";
        //            pKey = "/f0rDymK3yL0g1n.";
        //            break;
        //        case "CINGE":
        //            pDataBase = "CINGE_Migra";
        //            pServer = "progrx.centralus.cloudapp.azure.com";
        //            pUser = "Sys_Migracion";
        //            pKey = "/f0rDymK3yL0g1n.";
        //            break;
        //        case "OPTISOFT":
        //            pDataBase = "CODEAS_Migra";
        //            pServer = "progrx.centralus.cloudapp.azure.com";
        //            pUser = "Sys_Migracion";
        //            pKey = "/f0rDymK3yL0g1n.";
        //            break;
        //        case "SIBU":
        //            pDataBase = "CODEAS_Migra";
        //            pServer = "progrx.centralus.cloudapp.azure.com";
        //            pUser = "Sys_Migracion";
        //            pKey = "/f0rDymK3yL0g1n.";
        //            break;
        //        case "AXAPTA":
        //            pDataBase = "ASECCSSProd";
        //            pServer = "10.10.1.36";
        //            pUser = "soporte_systemlogic";
        //            pKey = "H$#0D$Cju0xAei(-V298";
        //            break;

        //    }

        //    /*string connectionString = $"PROVIDER=MSDASQL;Driver=SQL Server;Server={pServer};" +
        //        $"Database={pDataBase};APP=PGX_Portal_Admin;tcp:{pServer},{SIFGlobal.PuertosDisponibles};";*/

        //    string connectionString = $"PROVIDER=MSDASQL;Driver=SQL Server;Server={pServer};" +
        //        $"Database={pDataBase};APP=PGX_Portal_Admin;tcp:{pServer};";

        //    TablasCargar(connectionString);
        //}

        public ResultadoConsultaDto sbCargaResultados(string pObjeto)
        {
            ResultadoConsultaDto resultado = new ResultadoConsultaDto();

            try
            {

                // Consulta para cargar resultados
                string strSQL = "SELECT TOP 50 * from " + pObjeto;

                using (var connection = new SqlConnection(_config.GetConnectionString("DefaultConnString")))
                {
                    var results = connection.Query(strSQL);

                    if (results != null && results.Any())
                    {
                        // Obtener nombres de columnas y tipos de datos
                        //resultado.Columnas = results.FirstOrDefault().Dictionary.ToDictionary(pair => UCase(pair.Key), pair => pair.Value.GetType().Name);

                        // Obtener datos de las filas
                        resultado.Datos = new List<Dictionary<string, string>>();

                        foreach (var row in results)
                        {
                            var rowDictionary = new Dictionary<string, string>();
                            foreach (var column in row.Dictionary)
                            {
                                //rowDictionary[UCase(column.Key)] = column.Value.ToString();
                            }
                            resultado.Datos.Add(rowDictionary);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Manejar excepciones aquí si es necesario
            }

            return resultado;
        }



    }
}
