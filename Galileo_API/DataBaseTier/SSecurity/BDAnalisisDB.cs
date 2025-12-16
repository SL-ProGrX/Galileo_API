using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;
using System.Security;

namespace Galileo.DataBaseTier
{
    public class BDAnalisisDB
    {
        private readonly IConfiguration _config;

        // 🔒 Lista blanca de tablas permitidas
        private static readonly HashSet<string> TablasPermitidas = new()
        {
            "Usuarios",
            "Roles",
            "Permisos",
            "Logs"
            // 👉 agrega aquí SOLO las tablas que deban consultarse
        };

        public BDAnalisisDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Devuelve las tablas de usuario del sistema
        /// </summary>
        public static List<string> TablasCargar()
        {
            try
            {
                using var connection =
                    new SqlConnection("DefaultConnString");

                const string sql = @"
                    SELECT name
                    FROM sys.objects
                    WHERE type = 'U'
                    ORDER BY name";

                return connection.Query<string>(sql).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        /// <summary>
        /// Carga los primeros 50 registros de una tabla permitida
        /// </summary>
        public ResultadoConsultaDto sbCargaResultados(string pObjeto)
        {
            if (!TablasPermitidas.Contains(pObjeto))
                throw new SecurityException("Tabla no permitida");

            ResultadoConsultaDto resultado = new();

            string sql = $"SELECT TOP 50 * FROM [{pObjeto}]";

            try
            {
                using var connection =
                    new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                var results = connection.Query(sql);

                if (results != null && results.Any())
                {
                    resultado.Datos = new List<Dictionary<string, string>>();

                    foreach (var row in results)
                    {
                        var rowDictionary = new Dictionary<string, string>();

                        foreach (var property in (IDictionary<string, object>)row)
                        {
                            rowDictionary[property.Key] =
                                property.Value?.ToString() ?? string.Empty;
                        }

                        resultado.Datos.Add(rowDictionary);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log real aquí si aplica
                _ = ex.Message;
            }

            return resultado;
        }
    }
}