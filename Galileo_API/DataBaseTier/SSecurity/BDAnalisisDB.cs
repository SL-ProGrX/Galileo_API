using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models.Security;
using System.Security;
using System.Text.RegularExpressions;

namespace Galileo.DataBaseTier
{
    public class BDAnalisisDB
    {
        private readonly IConfiguration _config;

        // =======================
        // Seguridad identificadores
        // =======================

        private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

        private static readonly Regex IdentRegex = new(
            @"^[A-Za-z_][A-Za-z0-9_]*$",
            RegexOptions.Compiled | RegexOptions.CultureInvariant,
            RegexTimeout
        );

        private static string SafeIdent(string ident)
        {
            if (string.IsNullOrWhiteSpace(ident) || !IdentRegex.IsMatch(ident))
                throw new SecurityException("Identificador SQL inválido.");

            return $"[{ident}]";
        }

        // =======================
        // Whitelist de tablas
        // =======================

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

        // =======================
        // API pública
        // =======================

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

            // 1️⃣ Whitelist estricta
            if (!TablasPermitidas.TryGetValue(pObjeto.Trim(), out var tablaPermitida))
                throw new SecurityException("Tabla no permitida");

            // 2️⃣ Identificador SQL seguro
            var table = SafeIdent(tablaPermitida);

            var resultado = new ResultadoConsultaDto
            {
                Datos = new List<Dictionary<string, string>>()
            };

            try
            {
                using var connection =
                    new SqlConnection(_config.GetConnectionString("DefaultConnString"));

                // 3️⃣ SQL SIN VARIABLE INTERMEDIA
                var rows = connection.Query($"SELECT TOP (50) * FROM {table};");

                foreach (var row in rows)
                {
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var kv in (IDictionary<string, object>)row)
                        dict[kv.Key] = kv.Value?.ToString() ?? string.Empty;

                    resultado.Datos.Add(dict);
                }
            }
            catch (Exception ex)
            {
                // aquí iría log real
                _ = ex.Message;
            }

            return resultado;
        }
    }
}