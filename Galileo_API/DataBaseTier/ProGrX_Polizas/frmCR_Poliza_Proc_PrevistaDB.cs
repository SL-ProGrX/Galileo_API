using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaProcPrevistaDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrPolizaProcPrevistaDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Metodo para obtener pólizas facturables
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cr_PolProcPrevista_PolizaFacturables_Lista(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spPoliza_Facturables_Lista";

                var result = conn.Query<dynamic>(query).ToList();

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,      
                    descripcion = x.ItmX  
                }).ToList();

                return lista;
            });
        }
    }
}
