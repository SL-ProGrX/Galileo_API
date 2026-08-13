using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo_API.DataBaseTier.ProGrX_ARF
{
    public class FrmArfInformesDb
    {
        private readonly PortalDB _portalDb;

        public FrmArfInformesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las oficinas o unidades disponibles para los filtros de informes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Resultado con las unidades ordenadas por código.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Unidades_Listar(int codEmpresa)
        {
            const string sql = @"SELECT
                                     COD_LOCAL AS item,
                                     Descripcion AS descripcion
                                 FROM ARF_UNIDADES
                                 ORDER BY COD_LOCAL";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql
            );
        }

        /// <summary>
        /// Obtiene los arrendadores disponibles para los filtros de informes.
        /// </summary>
        /// <param name="codEmpresa">Código de la empresa que se consultará.</param>
        /// <returns>Resultado con los arrendadores ordenados por código.</returns>
        public ErrorDto<List<DropDownListaGenericaModel>> ARF_Arrendadores_Listar(int codEmpresa)
        {
            const string sql = @"SELECT
                                     COD_ACREEDOR AS item,
                                     Descripcion AS descripcion
                                 FROM ARF_ACREEDORES
                                 ORDER BY COD_ACREEDOR";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql
            );
        }
    }
}
