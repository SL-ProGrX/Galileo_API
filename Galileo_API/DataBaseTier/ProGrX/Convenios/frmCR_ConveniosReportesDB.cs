using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Convenios;

namespace Galileo_API.DataBaseTier.ProGrX.Convenios
{
    public class FrmCrConveniosReportesDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrConveniosReportesDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Lista el catálogo (codigo, descripcion) para la búsqueda F4 del campo Línea.
        /// </summary>
        public ErrorDto<List<CrConveniosReportesCatalogoData>> CrConveniosReportes_Catalogo_Obtener(int codEmpresa)
        {
            const string sql = @"
                select
                    rtrim(isnull(codigo, '')) as codigo,
                    rtrim(isnull(descripcion, '')) as descripcion
                from catalogo
                order by descripcion;";

            return DbHelper.ExecuteListQuery<CrConveniosReportesCatalogoData>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene la descripción de un código del catálogo (fxDescribeCodigo del VB6).
        /// </summary>
        public ErrorDto<CrConveniosReportesCatalogoData> CrConveniosReportes_Catalogo_Describe(int codEmpresa, string codigo)
        {
            const string sql = @"
                select
                    rtrim(isnull(codigo, '')) as codigo,
                    rtrim(isnull(descripcion, '')) as descripcion
                from catalogo
                where codigo = @codigo;";

            try
            {
                using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);
                var item = conn.QueryFirstOrDefault<CrConveniosReportesCatalogoData>(sql, new { codigo = (codigo ?? string.Empty).Trim() });

                return item is null
                    ? DbHelper.CreateErrorResponse<CrConveniosReportesCatalogoData>($"No se encontró código - {codigo}")
                    : DbHelper.CreateOkResponse(item);
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse<CrConveniosReportesCatalogoData>(ex.Message);
            }
        }
    }
}
