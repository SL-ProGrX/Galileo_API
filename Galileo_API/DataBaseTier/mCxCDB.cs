using Dapper;
using Microsoft.Data.SqlClient;

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

    }
}
