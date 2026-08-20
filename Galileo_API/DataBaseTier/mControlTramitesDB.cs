using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo.DataBaseTier
{
    public static class MControlTramitesDB
    {
        public static string fxEstadoCivil(string? vEstadoCivil)
        {
            string estadoCivil = vEstadoCivil ?? string.Empty;

            return estadoCivil.ToUpperInvariant() switch
            {
                "S" => "Soltero",
                "C" => "Casado",
                "D" => "Divorciado",
                "V" => "Viudo",
                "U" => "Union libre",
                "O" => "Otro",
                "SOLTERO" => "S",
                "CASADO" => "C",
                "DIVORCIADO" => "D",
                "VIUDO" => "V",
                "UNION LIBRE" => "U",
                "OTRO" => "O",
                _ => estadoCivil
            };
        }

        public static string fxNombre(
            SqlConnection connection,
            string? strCedula)
        {
            string cedula = (strCedula ?? string.Empty).Trim();

            if (cedula.Length == 0)
            {
                return string.Empty;
            }

            const string sql = """
                SELECT ISNULL(NOMBRE, '')
                FROM SOCIOS
                WHERE CEDULA = @cedula;
                """;

            return connection.QueryFirstOrDefault<string>(
                sql,
                new
                {
                    cedula
                }) ?? string.Empty;
        }
    }
}