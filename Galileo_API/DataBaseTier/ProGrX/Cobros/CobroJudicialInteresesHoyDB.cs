using Dapper;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    /// <summary>
    /// Compatibiliza la consulta de intereses de cobro con las distintas firmas
    /// instaladas de spCbrCobroJudicialInteresesHoy.
    /// </summary>
    internal static class CobroJudicialInteresesHoyDB
    {
        private const string NombreProcedimiento = "dbo.spCbrCobroJudicialInteresesHoy";

        /// <summary>
        /// Consulta los intereses de una operación utilizando únicamente los
        /// parámetros admitidos por la versión instalada del procedimiento.
        /// </summary>
        /// <param name="connection">Conexión a la base de datos de la empresa.</param>
        /// <param name="operacion">Número de operación de crédito.</param>
        /// <param name="fechaCorte">Fecha utilizada para calcular los intereses.</param>
        /// <param name="cobroJudicial">Indica si el cálculo corresponde a cobro judicial.</param>
        /// <returns>Detalle de intereses calculado por el procedimiento.</returns>
        internal static CobroJudicialInteresesHoyData? Consultar(
            SqlConnection connection,
            int operacion,
            DateTime fechaCorte,
            int cobroJudicial = 0)
        {
            const string sqlCantidadParametros = @"
                SELECT COUNT(*)
                FROM sys.parameters
                WHERE object_id = OBJECT_ID(@NombreProcedimiento, 'P')
                  AND is_output = 0;";

            var cantidadParametros = connection.QuerySingle<int>(
                sqlCantidadParametros,
                new { NombreProcedimiento });

            if (cantidadParametros == 0)
            {
                throw new InvalidOperationException(
                    $"No se encontró el procedimiento {NombreProcedimiento}.");
            }

            var parametros = new DynamicParameters();
            parametros.Add("operacion", operacion);

            string sql;
            if (cantidadParametros == 1)
            {
                sql = $"EXEC {NombreProcedimiento} @operacion";
            }
            else if (cantidadParametros == 2)
            {
                parametros.Add("fechaCorte", fechaCorte.ToString("yyyy/MM/dd"));
                sql = $"EXEC {NombreProcedimiento} @operacion, @fechaCorte";
            }
            else
            {
                parametros.Add("fechaCorte", fechaCorte.ToString("yyyy/MM/dd"));
                parametros.Add("cobroJudicial", cobroJudicial);
                sql = $"EXEC {NombreProcedimiento} @operacion, @fechaCorte, @cobroJudicial";
            }

            return connection.QueryFirstOrDefault<CobroJudicialInteresesHoyData>(
                sql,
                parametros);
        }
    }

    /// <summary>
    /// Resultado normalizado de spCbrCobroJudicialInteresesHoy.
    /// </summary>
    internal sealed class CobroJudicialInteresesHoyData
    {
        public decimal? RegIntCor { get; set; }
        public decimal? RegIntMor { get; set; }
        public decimal? Poliza { get; set; }
        public decimal? Cargos { get; set; }
        public decimal? RegPrincipal { get; set; }
        public string? Antiguedad { get; set; }
    }
}
