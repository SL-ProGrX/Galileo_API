using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace Galileo_API.DataBaseTier
{
    public static class MCxCDb
    {
        public static bool fxCxCSaldoVerifica(SqlConnection conn, long pOperacion, decimal pSaldo)
        {
            try
            {
                const string query = @"
                    SELECT Saldo FROM CxC_Cuentas
                    WHERE Operacion = @operacion;";

                decimal? curSaldo = conn.QueryFirstOrDefault<decimal?>(
                    query,
                    new { operacion = pOperacion }
                );

                if (!curSaldo.HasValue)
                    return false;

                return curSaldo.Value == pSaldo;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Obtiene el valor de un parámetro de CxC.
        /// Si no existe, devuelve "3" según la lógica original de VB6.
        /// </summary>
        /// <param name="codEmpresa">Empresa activa.</param>
        /// <param name="codParametro">Código del parámetro.</param>
        /// <returns>Valor del parámetro.</returns>
        public static ErrorDto<string> fxCxC_Parametro(SqlConnection conn, int codEmpresa, string codParametro)
        {
            if (string.IsNullOrWhiteSpace(codParametro))
            {
                return DbHelper.CreateErrorResponse("El código del parámetro es requerido.", result: "3");
            }

            try
            {
                const string sql = @"
                    SELECT TOP 1 RTRIM(ISNULL(valor, ''))
                    FROM CxC_Parametros
                    WHERE cod_parametro = @codParametro;";

                var valor = conn.QueryFirstOrDefault<string>(sql, new
                {
                    codParametro = codParametro.Trim()
                });

                return new ErrorDto<string>
                {
                    Code = 0,
                    Description = "Ok",
                    Result = string.IsNullOrWhiteSpace(valor) ? "3" : valor.Trim()
                };
            }
            catch (DbException ex)
            {
                return DbHelper.CreateErrorResponse($"No fue posible consultar el parámetro de CxC. {ex.Message}", result: "3");
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse($"Error inesperado al consultar el parámetro de CxC. {ex.Message}", result: "3");
            }
        }
    }
}
