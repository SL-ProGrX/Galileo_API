using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvParametrosDB
    {
        private readonly IConfiguration _config;

        private const string MensajeOk = "Ok";
        private const string ErrorParametrosGenerales = "Error al obtener los parámetros generales.";
        private const string SinParametrosGenerales = "No se encontraron parámetros generales.";
        private const string ErrorActualizarParametros = "Error al actualizar los parámetros generales.";
        private const string QueryParametrosGenerales = "SELECT * FROM PV_PARAMETROS_GEN";
        private const string QueryContabilidades = "SELECT * FROM CntX_Contabilidades";
        private const string QueryDescripcionesCuenta = "SELECT Cod_Cuenta, Descripcion FROM CNTX_CUENTAS";
        private const string QueryDescripcionesAsiento = "SELECT Tipo_Asiento, Descripcion FROM CntX_Tipos_Asientos";
        private const string QueryAsientos = "SELECT * FROM CNTX_TIPOS_ASIENTOS";

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvParametrosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvParametrosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta estándar para operaciones de consulta única.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="errorMessage">Mensaje cuando ocurre un error.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar para una sola entidad.</returns>
        private static ErrorDto<T> CrearRespuestaSingle<T>(ErrorDto<T?> result, string errorMessage, string notFoundMessage)
            where T : class
        {
            if (result.Code != 0)
            {
                return new ErrorDto<T>
                {
                    Code = result.Code,
                    Description = result.Description ?? errorMessage,
                    Result = null
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<T>
                {
                    Code = -2,
                    Description = notFoundMessage,
                    Result = null
                };
        }

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
        /// Ejecuta una consulta de listado usando el helper estándar de base de datos.
        /// </summary>
        /// <typeparam name="T">Tipo de datos a devolver.</typeparam>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="query">Consulta SQL fija.</param>
        /// <returns>Listado solicitado.</returns>
        private ErrorDto<List<T>> EjecutarListado<T>(int codEmpresa, string query)
        {
            return DbHelper.ExecuteListQuery<T>(
                CreatePortalDb(),
                codEmpresa,
                query);
        }

        /// <summary>
        /// Crea el objeto de parámetros para actualización de parámetros generales.
        /// </summary>
        private static object CrearParametrosActualizacion(ParametrosGenDto data) => new
        {
            data.Cta_Comisiones,
            data.Cta_Imp_Renta,
            data.Cta_Imp_Consumo,
            data.Cta_Gastos,
            data.Cta_Costo_Ventas,
            data.Cta_Recibos,
            data.Cta_Notas,
            data.Cta_Ventas_Ing,
            data.Ta_Factura_Man,
            data.Ta_Factura_Auto,
            data.Ta_Entradas,
            data.Ta_Salidas,
            data.Ta_Traslados,
            data.Ta_Devoluciones,
            data.Ta_Nc,
            data.Ta_Recibos,
            data.Ta_Nd,
            data.Ta_Gen,
            data.Enlace_Conta,
            data.Enlace_Sif,
            data.Cod_Par
        };

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene los parámetros generales de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Parámetros generales encontrados.</returns>
        public ErrorDto<ParametrosGenDto> Parametros_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteSingleQuery<ParametrosGenDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryParametrosGenerales,
                null);

            return CrearRespuestaSingle(
                result,
                ErrorParametrosGenerales,
                SinParametrosGenerales);
        }

        /// <summary>
        /// Obtiene las contabilidades disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de contabilidades.</returns>
        public ErrorDto<List<CntXContaDto>> ObtenerContabilidades(int CodEmpresa)
        {
            return EjecutarListado<CntXContaDto>(CodEmpresa, QueryContabilidades);
        }

        /// <summary>
        /// Obtiene las descripciones de cuentas contables.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de cuentas contables.</returns>
        public ErrorDto<List<DescripcionCuentasDto>> Obtener_DescripcionesCuenta(int CodEmpresa)
        {
            return EjecutarListado<DescripcionCuentasDto>(CodEmpresa, QueryDescripcionesCuenta);
        }

        /// <summary>
        /// Obtiene las descripciones de tipos de asiento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de tipos de asiento.</returns>
        public ErrorDto<List<DescripcionTipoAsientoDto>> Obtener_DescripcionesAsiento(int CodEmpresa)
        {
            return EjecutarListado<DescripcionTipoAsientoDto>(CodEmpresa, QueryDescripcionesAsiento);
        }

        /// <summary>
        /// Obtiene todos los tipos de asientos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de tipos de asientos.</returns>
        public ErrorDto<List<DescripcionTipoAsientoDto>> Asientos_Obtener(int CodEmpresa)
        {
            return EjecutarListado<DescripcionTipoAsientoDto>(CodEmpresa, QueryAsientos);
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza los parámetros generales de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de parámetros a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto actualizar_Parametros(int CodEmpresa, ParametrosGenDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE PV_PARAMETROS_GEN SET
                        Cta_Comisiones = @Cta_Comisiones,
                        Cta_Imp_Renta = @Cta_Imp_Renta,
                        Cta_Imp_Consumo = @Cta_Imp_Consumo,
                        Cta_Gastos = @Cta_Gastos,
                        Cta_Costo_Ventas = @Cta_Costo_Ventas,
                        Cta_Recibos = @Cta_Recibos,
                        Cta_Notas = @Cta_Notas,
                        Cta_Ventas_Ing = @Cta_Ventas_Ing,
                        Ta_Factura_Man = @Ta_Factura_Man,
                        Ta_Factura_Auto = @Ta_Factura_Auto,
                        Ta_Entradas = @Ta_Entradas,
                        Ta_Salidas = @Ta_Salidas,
                        Ta_Traslados = @Ta_Traslados,
                        Ta_Devoluciones = @Ta_Devoluciones,
                        Ta_Nc = @Ta_Nc,
                        Ta_Recibos = @Ta_Recibos,
                        Ta_Nd = @Ta_Nd,
                        Ta_Gen = @Ta_Gen,
                        Enlace_Conta = @Enlace_Conta,
                        Enlace_Sif = @Enlace_Sif
                  WHERE COD_PAR = @Cod_Par;",
                CrearParametrosActualizacion(data));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorActualizarParametros);
        }

        #endregion
    }
}