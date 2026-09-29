using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.DataBaseTier
{
    public class FrmInvReporteInventariosDB
    {
        private const int CodigoValidacion = -2;

        private const string MensajeEmpresaRequerida =
            "El c&oacute;digo de la empresa es requerido.";

        private const string MensajeLineaRequerida =
            "La l&iacute;nea de producto es requerida.";

        private readonly PortalDB _portalDb;

        public FrmInvReporteInventariosDB(
            IConfiguration config)
        {
            ArgumentNullException.ThrowIfNull(config);
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene las bodegas disponibles para los reportes de inventarios.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de bodegas.</returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<string>>>
            INV_ReporteInventarios_Bodegas_Obtener(
                int CodEmpresa)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<string>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string query = """
                SELECT
                    RTRIM(COD_BODEGA) AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM PV_BODEGAS
                ORDER BY DESCRIPCION;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel<string>>(
                    _portalDb,
                    CodEmpresa,
                    query);
        }

        /// <summary>
        /// Obtiene las líneas de productos disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de líneas.</returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_ReporteInventarios_Lineas_Obtener(
                int CodEmpresa)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<int>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string query = """
                SELECT
                    COD_PRODCLAS AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM PV_PROD_CLASIFICA
                ORDER BY COD_PRODCLAS;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel<int>>(
                    _portalDb,
                    CodEmpresa,
                    query);
        }

        /// <summary>
        /// Obtiene las sublíneas pertenecientes a una línea de productos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodLinea">Código de la línea.</param>
        /// <returns>Listado de sublíneas.</returns>
        public ErrorDto<
            List<DropDownListaGenericaModel<int>>>
            INV_ReporteInventarios_Sublineas_Obtener(
                int CodEmpresa,
                int CodLinea)
        {
            var lista =
                new List<
                    DropDownListaGenericaModel<int>>();

            if (CodEmpresa <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeEmpresaRequerida,
                    CodigoValidacion,
                    lista);
            }

            if (CodLinea <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeLineaRequerida,
                    CodigoValidacion,
                    lista);
            }

            const string query = """
                SELECT
                    COD_LINEA_SUB AS item,
                    RTRIM(DESCRIPCION) AS descripcion
                FROM PV_PROD_CLASIFICA_SUB
                WHERE COD_PRODCLAS = @CodLinea
                ORDER BY COD_LINEA_SUB;
                """;

            return DbHelper.ExecuteListQuery<
                DropDownListaGenericaModel<int>>(
                    _portalDb,
                    CodEmpresa,
                    query,
                    new
                    {
                        CodLinea
                    });
        }
    }
}