using Dapper;
using Microsoft.Data.SqlClient;
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

        public static List<string> TablasCargar(string Connectionstring)
        {
            List<string> resp;
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
                resp = new List<string>();
            }
            return resp;
        }
        
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

                        // Obtener datos de las filas
                        resultado.Datos = new List<Dictionary<string, string>>();

                        foreach (var row in results)
                        {
                            var rowDictionary = new Dictionary<string, string>();
                            foreach (var column in row.Dictionary)
                            {
                                rowDictionary[column.Key] = column.Value.ToString();
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