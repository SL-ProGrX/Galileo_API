using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo.Models.Security;
using Galileo_API.Models.ProGrX.Cobros;
using Newtonsoft.Json;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoIncobrablesListGeneralDb
    {
        private readonly PortalDB _portalDB;

        public FrmCoIncobrablesListGeneralDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }


        /// <summary>
        /// Consulta de movimiento por operacion 
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperacion"></param>
        /// <param name="pCxC_Operacion"></param>
        /// <returns></returns>
        public ErrorDto<List<CbrIncobrableMovimientos>> CoIncobrablesListMovimiento_Obtener(int CodEmpresa, string pOperacion, string pCxC_Operacion)
        {

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string spName = "spCbr_Incobrable_Movimientos";

                var cmd = new CommandDefinition(
                    commandText: spName,
                    parameters: new { pOperacion, pCxC_Operacion },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60  
                );

                return conn.Query<CbrIncobrableMovimientos>(cmd).ToList();
            });
                       
                       
        }


        /// <summary>
        /// Consulta de listados de casos incobrales
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<CbrIncobrableGeneral>> CoIncobrablesListGeneral_Obtener(int CodEmpresa, CbrIncobrableFiltros filtros)
        {

            var inicio = DateTime.SpecifyKind(filtros.Inicio, DateTimeKind.Utc);
            var corte = DateTime.SpecifyKind(filtros.Corte, DateTimeKind.Utc);
            var auxiliar = DateTime.SpecifyKind(filtros.Auxiliar, DateTimeKind.Utc);

            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string spName = "spCbr_Incobrable_Lista_General";

                var cmd = new CommandDefinition(
                    commandText: spName,
                    parameters: new
                    {
                        filtros.Estado,
                        Inicio = inicio,
                        Corte = corte,
                        filtros.Filtro,
                        Auxiliar = auxiliar,
                        filtros.Usuario
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60 
                );

                return conn.Query<CbrIncobrableGeneral>(cmd).ToList();
            });

        }

    }
}
