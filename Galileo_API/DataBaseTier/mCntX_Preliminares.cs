using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using System.Data;

namespace Galileo_API.DataBaseTier
{
    public class MCntXPreliminaresDb
    {
        private readonly PortalDB _portalDb;

        public MCntXPreliminaresDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Genera los saldos preliminares de la contabilidad para el periodo indicado.
        /// </summary>
        /// <param name="codEmpresa">Codigo de la empresa cuya conexion se utilizara.</param>
        /// <param name="request">Parametros requeridos para generar el preliminar.</param>
        /// <returns>Resultado de la ejecucion del proceso preliminar.</returns>
        public ErrorDto<bool> sbCntX_Preliminar_Montar(
            int codEmpresa,
            CntXPreliminarMontarRequest request)
        {
            const string procedimiento = "spCntX_Preliminar_Procesa";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(
                    procedimiento,
                    new
                    {
                        Contabilidad = request.codContabilidad,
                        Anio = request.anio,
                        Mes = request.mes,
                        Preliminar = "A",
                        Usuario = request.usuario,
                        Unidad = request.unidad,
                        CentroCosto = "0x0"
                    },
                    commandType: CommandType.StoredProcedure);

                return true;
            });
        }
    }
}
