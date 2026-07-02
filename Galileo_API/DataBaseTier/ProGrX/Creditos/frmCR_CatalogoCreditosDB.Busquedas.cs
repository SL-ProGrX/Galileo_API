using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Credito;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public partial class FrmCrCatalogoCreditosDb
    {
        /// <summary>
        /// Obtiene las oficinas disponibles para asociar a una linea de credito.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Lista de oficinas disponibles.</returns>
        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Oficinas_Obtener(int codEmpresa)
        {
            const string query = @"
                SELECT
                    RTRIM(COD_OFICINA) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM SIF_OFICINAS
                ORDER BY COD_OFICINA;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoBusquedaData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene los planes disponibles para asociar reservas o lineas revolutivas.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Lista de planes disponibles.</returns>
        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Planes_Obtener(int codEmpresa)
        {
            const string query = @"
                SELECT
                    RTRIM(cod_Plan) AS item,
                    RTRIM(Descripcion) AS descripcion
                FROM fnd_Planes
                ORDER BY cod_Plan;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoBusquedaData>(_portalDb, codEmpresa, query);
        }

        /// <summary>
        /// Obtiene las divisas disponibles para el catalogo de creditos.
        /// </summary>
        /// <param name="codEmpresa">Codigo de empresa.</param>
        /// <returns>Lista de divisas disponibles.</returns>
        public ErrorDto<List<CrCatalogoCreditoBusquedaData>> CrCatalogoCreditos_Divisas_Obtener(int codEmpresa)
        {
            const string query = @"
                SELECT
                    RTRIM(COD_DIVISA) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM vSys_Divisas
                ORDER BY DESCRIPCION;";

            return DbHelper.ExecuteListQuery<CrCatalogoCreditoBusquedaData>(_portalDb, codEmpresa, query);
        }
    }
}
