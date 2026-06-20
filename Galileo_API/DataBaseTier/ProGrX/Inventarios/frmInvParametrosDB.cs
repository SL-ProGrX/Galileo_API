using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvParametrosDB
    {
        private readonly IConfiguration _config;

        private static readonly string[] CamposActualizacion =
        {
            nameof(ParametrosGenDto.Cta_Comisiones),
            nameof(ParametrosGenDto.Cta_Imp_Renta),
            nameof(ParametrosGenDto.Cta_Imp_Consumo),
            nameof(ParametrosGenDto.Cta_Gastos),
            nameof(ParametrosGenDto.Cta_Costo_Ventas),
            nameof(ParametrosGenDto.Cta_Recibos),
            nameof(ParametrosGenDto.Cta_Notas),
            nameof(ParametrosGenDto.Cta_Ventas_Ing),
            nameof(ParametrosGenDto.Ta_Factura_Man),
            nameof(ParametrosGenDto.Ta_Factura_Auto),
            nameof(ParametrosGenDto.Ta_Entradas),
            nameof(ParametrosGenDto.Ta_Salidas),
            nameof(ParametrosGenDto.Ta_Traslados),
            nameof(ParametrosGenDto.Ta_Devoluciones),
            nameof(ParametrosGenDto.Ta_Nc),
            nameof(ParametrosGenDto.Ta_Recibos),
            nameof(ParametrosGenDto.Ta_Nd),
            nameof(ParametrosGenDto.Ta_Gen),
            nameof(ParametrosGenDto.Enlace_Conta),
            nameof(ParametrosGenDto.Enlace_Sif)
        };

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
        /// Ejecuta una consulta única usando el helper estándar y aplica el formato de respuesta esperado.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="codEmpresa">Código de la empresa.</param>
        /// <param name="query">Consulta SQL fija.</param>
        /// <param name="errorMessage">Mensaje cuando ocurre un error.</param>
        /// <param name="notFoundMessage">Mensaje cuando no se encuentra información.</param>
        /// <returns>Respuesta estándar para una sola entidad.</returns>
        private ErrorDto<T> EjecutarConsultaUnica<T>(int codEmpresa, string query, string errorMessage, string notFoundMessage)
            where T : class
        {
            var result = DbHelper.ExecuteSingleQuery<T>(
                CreatePortalDb(),
                codEmpresa,
                query,
                null);

            return CrearRespuestaSingle(result, errorMessage, notFoundMessage);
        }

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
        /// Crea la consulta de actualización de parámetros generales.
        /// </summary>
        /// <returns>Consulta SQL de actualización.</returns>
        private static string CrearConsultaActualizacion()
        {
            var asignaciones = string.Join(", ", CamposActualizacion.Select(campo => $"{campo} = @{campo}"));
            return $"UPDATE PV_PARAMETROS_GEN SET {asignaciones} WHERE COD_PAR = @Cod_Par;";
        }

        /// <summary>
        /// Crea el objeto de parámetros para actualización de parámetros generales.
        /// </summary>
        /// <param name="data">Datos de parámetros generales.</param>
        /// <returns>Parámetros para Dapper.</returns>
        private static Dapper.DynamicParameters CrearParametrosActualizacion(ParametrosGenDto data)
        {
            var parametros = new Dapper.DynamicParameters();
            var tipo = typeof(ParametrosGenDto);

            foreach (var campo in CamposActualizacion)
            {
                parametros.Add(campo, tipo.GetProperty(campo)?.GetValue(data));
            }

            parametros.Add(nameof(ParametrosGenDto.Cod_Par), data.Cod_Par);
            return parametros;
        }

        #endregion

        #region Consultas

        public ErrorDto<ParametrosGenDto> Parametros_Obtener(int CodEmpresa) =>
            EjecutarConsultaUnica<ParametrosGenDto>(
                CodEmpresa,
                "SELECT * FROM PV_PARAMETROS_GEN",
                "Error al obtener los parámetros generales.",
                "No se encontraron parámetros generales.");

        public ErrorDto<List<CntXContaDto>> ObtenerContabilidades(int CodEmpresa) =>
            EjecutarListado<CntXContaDto>(CodEmpresa, "SELECT * FROM CntX_Contabilidades");

        public ErrorDto<List<DescripcionCuentasDto>> Obtener_DescripcionesCuenta(int CodEmpresa) =>
            EjecutarListado<DescripcionCuentasDto>(CodEmpresa, "SELECT Cod_Cuenta, Descripcion FROM CNTX_CUENTAS");

        public ErrorDto<List<DescripcionTipoAsientoDto>> Obtener_DescripcionesAsiento(int CodEmpresa) =>
            EjecutarListado<DescripcionTipoAsientoDto>(CodEmpresa, "SELECT Tipo_Asiento, Descripcion FROM CntX_Tipos_Asientos");

        public ErrorDto<List<DescripcionTipoAsientoDto>> Asientos_Obtener(int CodEmpresa) =>
            EjecutarListado<DescripcionTipoAsientoDto>(CodEmpresa, "SELECT * FROM CNTX_TIPOS_ASIENTOS");

        #endregion

        #region Mantenimiento

        public ErrorDto actualizar_Parametros(int CodEmpresa, ParametrosGenDto data)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                CrearConsultaActualizacion(),
                CrearParametrosActualizacion(data));

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar los parámetros generales.");
        }

        #endregion
    }
}