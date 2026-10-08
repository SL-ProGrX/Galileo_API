using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier.ProGrX_BeneficiosFosol
{
    /// <summary>
    /// Acceso a datos de los reportes de beneficios FOSOL
    /// correspondientes a frmFSL_Reportes.
    /// </summary>
    public sealed class FrmFslReportesDB
    {
        private readonly PortalDB _portalDb;

        /// <summary>
        /// Inicializa el acceso a datos del formulario
        /// frmFSL_Reportes.
        /// </summary>
        /// <param name="config">
        /// Configuración general de la aplicación.
        /// </param>
        public FrmFslReportesDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);

            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las oficinas disponibles para filtrar los
        /// reportes de expedientes.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <returns>
        /// Catálogo de oficinas, incluyendo la opción TODOS.
        /// </returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            FSL_Reportes_Oficinas_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    C.item,
                    C.descripcion
                FROM
                (
                    SELECT
                        'TODOS' AS item,
                        'TODOS' AS descripcion,
                        0 AS orden

                    UNION ALL

                    SELECT
                        RTRIM(
                            ISNULL(O.cod_oficina, '')
                        ) AS item,
                        RTRIM(
                            ISNULL(O.cod_oficina, '')
                        ) + ' - ' +
                        RTRIM(
                            ISNULL(O.descripcion, '')
                        ) AS descripcion,
                        1 AS orden
                    FROM SIF_Oficinas O
                ) C
                ORDER BY
                    C.orden,
                    C.item;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel<string>>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }

        /// <summary>
        /// Obtiene los planes activos disponibles para
        /// filtrar los reportes de expedientes.
        /// </summary>
        /// <param name="CodEmpresa">
        /// Código de empresa.
        /// </param>
        /// <returns>
        /// Catálogo de planes activos, incluyendo la opción
        /// TODOS.
        /// </returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            FSL_Reportes_Planes_Obtener(
                int CodEmpresa)
        {
            const string sql = """
                SELECT
                    C.item,
                    C.descripcion
                FROM
                (
                    SELECT
                        'TODOS' AS item,
                        'TODOS' AS descripcion,
                        0 AS orden

                    UNION ALL

                    SELECT
                        RTRIM(
                            ISNULL(P.COD_PLAN, '')
                        ) AS item,
                        RTRIM(
                            ISNULL(P.COD_PLAN, '')
                        ) + ' - ' +
                        RTRIM(
                            ISNULL(P.DESCRIPCION, '')
                        ) AS descripcion,
                        1 AS orden
                    FROM FSL_PLANES P
                    WHERE P.ACTIVO = 1
                ) C
                ORDER BY
                    C.orden,
                    C.item;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel<string>>(
                    _portalDb,
                    CodEmpresa,
                    sql);
        }
    }
}