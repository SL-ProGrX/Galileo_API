using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Microsoft.CodeAnalysis;
using Org.BouncyCastle.Utilities;

namespace Galileo_API.DataBaseTier
{
    public class MCobroDb
    {
        private readonly PortalDB _portalDB;
        public MCobroDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }

        public decimal fxFechaProcesoAnterior(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoAnt(@proceso) as 'Result'";
            var parametros = new { proceso = pProceso };

            return DbHelper.ExecuteSingleQuery<decimal>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
        }

        public decimal fxFechaProcesoSiguiente(int CodEmpresa, decimal pProceso)
        {
            string sql = "select dbo.fxSIFPrmProcesoSig(@proceso) as 'Result'";
            var parametros = new { proceso = pProceso };

            return DbHelper.ExecuteSingleQuery<decimal>(_portalDB, CodEmpresa, sql, 0, parametros).Result;
        }

        public decimal fxCalcula_Cuota(decimal Monto,int Plazo,object Interes, string? Frecuencia = "M")
        {
            decimal curInteresMensual = 0, curfactor = 1;

            switch (Frecuencia)
            {
                case "M":
                    curInteresMensual = Convert.ToDecimal(Interes) / (12 * 100);
                    break;
                case "Q":
                    curInteresMensual = Convert.ToDecimal(Interes) / (24 * 100);
                    break;
                default:
                    curInteresMensual = Convert.ToDecimal(Interes) / (12 * 100);
                    break;
            }

            for (int i = 1; i <= Plazo; i++)
            {
                curfactor = curfactor * (1 +curInteresMensual);
            }
            var curCuota = Monto * curInteresMensual * curfactor / (curfactor - 1);
            curCuota = (Convert.ToDecimal(Interes) == 0)? Monto / Plazo : curCuota;

            return Math.Round(curCuota, 2); 

        }

    }
}
