using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvMargenUtilidadDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvMargenUtilidadDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvMargenUtilidadDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta estándar para operaciones no query.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="successMessage">Mensaje de éxito.</param>
        /// <param name="errorMessage">Mensaje de error.</param>
        /// <returns>Respuesta estándar para operaciones no query.</returns>
        private static ErrorDto CrearRespuestaNonQuery(ErrorDto result, string successMessage, string errorMessage)
        {
            return result.Code == 0
                ? DbHelper.OkResponse(successMessage)
                : DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Obtiene la consulta SQL para actualizar el margen según el tipo de cambio.
        /// </summary>
        /// <param name="cambioMargen">Tipo de cambio de margen.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ObtenerQueryCambioMargen(string cambioMargen)
        {
            return cambioMargen == "MU"
                ? @"UPDATE pv_productos
                    SET precio_regular = costo_regular + (costo_regular * @Monto / 100.0),
                        porc_utilidad = @Monto
                    WHERE estado = 'A'
                      AND cod_prodclas = @CodLinea
                      AND COD_LINEA_SUB = @CodSublinea"
                : @"UPDATE P
                    SET P.porc_utilidad = @Monto * 100.0,
                        P.PRECIO_REGULAR = P.costo_regular + (P.costo_regular * @Monto)
                    FROM pv_productos P
                    INNER JOIN pv_producto_precios X ON P.cod_producto = X.cod_producto
                    WHERE P.estado = 'A'
                      AND P.cod_prodclas = @CodLinea
                      AND P.COD_LINEA_SUB = @CodSublinea";
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene el listado de líneas de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de líneas.</returns>
        public ErrorDto<List<LineaDto>> Linea_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<LineaDto>(
                CreatePortalDb(),
                CodEmpresa,
                "select * from PV_PROD_CLASIFICA");
        }

        /// <summary>
        /// Obtiene el listado de sublíneas de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de sublíneas.</returns>
        public ErrorDto<List<SubLineaDto>> SubLinea_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<SubLineaDto>(
                CreatePortalDb(),
                CodEmpresa,
                "select * from PV_PROD_CLASIFICA_SUB");
        }

        /// <summary>
        /// Obtiene el listado de tipos de precio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de precios.</returns>
        public ErrorDto<List<PrecioDto>> ListadoPrecios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<PrecioDto>(
                CreatePortalDb(),
                CodEmpresa,
                "select * from pv_tipos_precios");
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza el margen de utilidad o precio de los productos según la línea y sublínea indicadas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="monto">Monto o porcentaje aplicado al margen.</param>
        /// <param name="cod_linea">Código de línea.</param>
        /// <param name="cod_sublinea">Código de sublínea.</param>
        /// <param name="cambio_margen">Tipo de cambio de margen.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto cambio_margen(int CodEmpresa, int monto, int cod_linea, int cod_sublinea, string cambio_margen)
        {
            var query = ObtenerQueryCambioMargen(cambio_margen);
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new
                {
                    Monto = monto,
                    CodLinea = cod_linea,
                    CodSublinea = cod_sublinea
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar el margen de utilidad.");
        }

        #endregion
    }
}