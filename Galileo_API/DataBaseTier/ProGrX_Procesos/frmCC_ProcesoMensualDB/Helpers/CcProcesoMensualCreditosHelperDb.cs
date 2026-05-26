using Dapper;
using System.Data; 

namespace Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.Helpers
{
    public static class CcProcesoMensualCreditosHelperDb
    {
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
