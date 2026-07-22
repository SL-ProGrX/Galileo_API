using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX.Creditos;

namespace Galileo_API.DataBaseTier.ProGrX.Creditos
{
    public class FrmCrReportesRevisionDB
    {
        private readonly PortalDB _portalDb;

        public FrmCrReportesRevisionDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene grupos para combos de usuarios de revisión.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_UsuariosGrupos_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_grupo) AS item,
                    RTRIM(cod_grupo) + ' - ' + RTRIM(descripcion) AS descripcion
                FROM crd_grupos;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene tipos de garantía para combo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Garantias_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(Garantia) AS item,
                    RTRIM(Garantia) + ' - ' + RTRIM(descripcion) AS descripcion
                FROM crd_garantia_tipos;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene oficinas para combo ordenadas por código.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Oficinas_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_oficina) AS item,
                    RTRIM(cod_oficina) + ' - ' + RTRIM(descripcion) AS descripcion
                FROM SIF_Oficinas
                ORDER BY cod_oficina;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene comités para combo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Comites_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    CAST(id_comite AS varchar(20)) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM comites;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene instituciones para combo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Instituciones_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(cod_institucion) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM instituciones;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene etiquetas para combo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrAutorizacionTranferenciasTag>> CR_ReportesRevision_Etiquetas_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(TAG_CODIGO) AS llave,
                    RTRIM(DESCRIPCION) AS describe
                FROM CRD_TAGS;";

            return DbHelper.ExecuteListQuery<CrAutorizacionTranferenciasTag>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene omisiones de análisis para combo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Omisiones_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    CAST(ID_ERROR AS varchar(20)) AS item,
                    CAST(ID_ERROR AS VARCHAR(15)) + ' - ' + RTRIM(descripcion) AS descripcion
                FROM CRD_ANALISIS_ERRORES;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Búsqueda tipo F4 de catálogo (código y descripción).
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ReportesRevision_Catalogo_F4_Obtener(int codEmpresa)
        {
            const string sql = @"
                SELECT
                    RTRIM(codigo) AS item,
                    RTRIM(descripcion) AS descripcion
                FROM catalogo;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDb, codEmpresa, sql);
        }

        /// <summary>
        /// Obtiene descripción de línea según código de catálogo.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<string?> CR_ReportesRevision_Catalogo_Descripcion_Obtener(int codEmpresa, string codigo)
        {
            codigo = (codigo ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                return DbHelper.CreateErrorResponse<string?>("Debe indicar el código.", -1, string.Empty);
            }

            const string sql = @"
                SELECT RTRIM(descripcion)
                FROM catalogo
                WHERE codigo = @Codigo;";

            return DbHelper.ExecuteSingleQuery<string>(_portalDb, codEmpresa, sql, string.Empty, new { Codigo = codigo });
        }
    }
}
