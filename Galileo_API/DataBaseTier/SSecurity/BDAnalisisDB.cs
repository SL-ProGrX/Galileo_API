using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;
using System.Security;

namespace Galileo.DataBaseTier
{
    public class BDAnalisisDB
    {
        private readonly IConfiguration _config;

        // 🔒 Whitelist cerrada + mapeo seguro
        // La key es lo que llega desde fuera
        // El value es el nombre REAL de la tabla en BD
        private static readonly IReadOnlyDictionary<string, string> TablasPermitidas =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Usuarios"]  = "Usuarios",
                ["Roles"]     = "Roles",
                ["Permisos"]  = "Permisos",
                ["Logs"]      = "Logs"
            };

        public BDAnalisisDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Devuelve las tablas de usuario del sistema
        /// </summary>
        public List<string> TablasCargar()
        {
            try
            {
                using var connection =
                    new SqlConnection(_config.GetConnectionString("DefaultConnString"));

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
            if (string.IsNullOrWhiteSpace(pObjeto))
                throw new SecurityException("Tabla no indicada");

            // 🔐 Validación fuerte: solo tablas de la whitelist
            if (!TablasPermitidas.TryGetValue(pObjeto.Trim(), out var tablaSegura))
                throw new SecurityException("Tabla no permitida");

            var sql = $"SELECT TOP (50) * FROM [{tablaSegura}]";

            var resultado = new ResultadoConsultaDto
            {
                Datos = new List<Dictionary<string, string>>()
            };

            try
            {
                using var connection =
                    new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                var results = connection.Query(sql);

                foreach (var row in results)
                {
                    var rowDictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var property in (IDictionary<string, object>)row)
                    {
                        rowDictionary[property.Key] =
                            property.Value?.ToString() ?? string.Empty;
                    }

                    resultado.Datos.Add(rowDictionary);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }

            return resultado;
        }
    }
}