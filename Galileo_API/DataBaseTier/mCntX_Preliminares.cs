using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
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
        /// <param name="codContabilidad">Codigo de la contabilidad.</param>
        /// <param name="anio">Anio del periodo contable.</param>
        /// <param name="mes">Mes del periodo contable.</param>
        /// <param name="preliminar">Tipo de preliminar que se debe procesar.</param>
        /// <param name="usuario">Usuario que solicita el proceso.</param>
        /// <param name="unidad">Unidad contable; por omision procesa todas.</param>
        /// <param name="centroCosto">Centro de costo; por omision procesa todos.</param>
        /// <returns>Resultado de la ejecucion del proceso preliminar.</returns>
        public ErrorDto<bool> sbCntX_Preliminar_Montar(
            int codEmpresa,
            int codContabilidad,
            int anio,
            int mes,
            string preliminar,
            string usuario,
            string unidad = "0x0",
            string centroCosto = "0x0")
        {
            const string procedimiento = "spCntX_Preliminar_Procesa";

            return DbHelper.WithConn(_portalDb, codEmpresa, conn =>
            {
                conn.Execute(
                    procedimiento,
                    new
                    {
                        Contabilidad = codContabilidad,
                        Anio = anio,
                        Mes = mes,
                        Preliminar = preliminar,
                        Usuario = usuario,
                        Unidad = unidad,
                        CentroCosto = centroCosto
                    },
                    commandType: CommandType.StoredProcedure);

                return true;
            });
        }
    }
}
