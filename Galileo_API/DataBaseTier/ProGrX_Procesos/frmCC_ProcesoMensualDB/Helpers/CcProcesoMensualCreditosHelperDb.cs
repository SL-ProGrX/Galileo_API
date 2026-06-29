using Dapper;
using System.Data; 

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers
{
    public static class CcProcesoMensualCreditosHelperDb
    {
        /// <summary>
        /// Calcula el saldo del mes para los créditos de una institución específica.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="codInstitucion"></param>
        /// <param name="proceso"></param>
        public static void SbCrCalculaSaldoMes( IDbConnection connection, int codInstitucion, decimal proceso = 0)
        {
            const string query = @" EXEC spPrmSaldoMesCreditos @CodInstitucion,@Proceso";

            connection.Execute(query, new
            {
                CodInstitucion = codInstitucion,
                Proceso = proceso
            });
        }
        
    }
}
