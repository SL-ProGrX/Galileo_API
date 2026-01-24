using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Nucleo;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_Nucleo
{
    public class FrmSugefRomMonitorDB
    {
        private readonly PortalDB _portalDb;

        public FrmSugefRomMonitorDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene el tipo de cambio SUGEF para una fecha dada.
        /// </summary>
        public ErrorDto<SugefTipoCambioResult?> SUGEF_TipoCambio_Obtener(int codEmpresa, DateTime fecha)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var query = "SELECT dbo.fxSUGEF_Tipo_Cambio(@Fecha) AS TC";
                var param = new { Fecha = fecha.ToString("yyyy-MM-dd") };
                return conn.QueryFirstOrDefault<SugefTipoCambioResult>(query, param);
            });
        }

        /// <summary>
        /// Ejecuta el procedimiento spSUGEF_ROM_Monitor_Consulta para obtener el monitoreo ROM SUGEF.
        /// </summary>
        public ErrorDto<List<SugefRomMonitorConsultaResult>> SUGEF_ROM_Monitor_Consulta(int codEmpresa, DateTime corte)
        {
            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                var param = new { Corte = corte };
                return conn.Query<SugefRomMonitorConsultaResult>(
                    "spSUGEF_ROM_Monitor_Consulta",
                    param,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
        }
    }
}
