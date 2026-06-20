using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTiposPreciosDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        private const string MensajeOk = "Ok";
        private const string ErrorObtenerPrecios = "Error al obtener tipos de precio.";
        private const string ErrorObtenerTodosPrecios = "Error al obtener todos los tipos de precio.";
        private const string ErrorActualizarPrecio = "Error al actualizar el tipo de precio.";
        private const string ErrorInsertarPrecio = "Error al insertar el tipo de precio.";
        private const string ErrorEliminarPrecio = "Error al eliminar el tipo de precio.";
        private const string QueryTotalPrecios = "SELECT COUNT(*) FROM pv_tipos_precios";
        private const string QueryPreciosBase = @"SELECT cod_precio,
                                                             descripcion,
                                                             defecto as activo
                                                      FROM pv_tipos_precios";
        private const string QueryPreciosTodos = "SELECT cod_precio, descripcion, defecto as activo FROM pv_tipos_precios ORDER BY cod_precio";
        private const string QueryActualizarPrecio = "UPDATE pv_tipos_precios SET descripcion = @Descripcion, defecto = @Defecto WHERE cod_precio = @Cod_Precio";
        private const string QueryInsertarPrecio = "INSERT INTO pv_tipos_precios(cod_precio, descripcion, defecto) VALUES(@Cod_Precio, @Descripcion, @Defecto)";
        private const string QueryEliminarPrecio = "DELETE pv_tipos_precios WHERE Cod_Precio = @Cod_Precio";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTiposPreciosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTiposPreciosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de tipos de precio.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static PreciosDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Precios = new List<Precio>()
        };

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
        /// Crea una respuesta estándar para el listado paginado de tipos de precio.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <returns>Respuesta de listado paginado.</returns>
        private static ErrorDto<PreciosDataLista> CrearRespuestaLista(ErrorDto<PreciosDataLista> result)
        {
            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerPrecios, result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Crea una respuesta estándar para listados completos de tipos de precio.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <returns>Respuesta de listado completo.</returns>
        private static ErrorDto<List<Precio>> CrearRespuestaTodos(ErrorDto<List<Precio>> result)
        {
            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<Precio>())
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerTodosPrecios, result.Code.GetValueOrDefault(-1), new List<Precio>());
        }

        /// <summary>
        /// Crea los parámetros comunes para insertar o actualizar tipos de precio.
        /// </summary>
        /// <param name="request">Datos del tipo de precio.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosPrecio(Precio request) => new
        {
            request.Cod_Precio,
            request.Descripcion,
            Defecto = request.activo
        };

        /// <summary>
        /// Crea los parámetros para eliminar un tipo de precio.
        /// </summary>
        /// <param name="precio">Código del tipo de precio.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosEliminar(string precio) => new
        {
            Cod_Precio = precio
        };

        /// <summary>
        /// Agrega filtro LIKE al listado de tipos de precio.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroPrecios(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" WHERE cod_precio LIKE @Filtro OR DESCRIPCION LIKE @Filtro ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a la consulta.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (!pagina.HasValue || !paginacion.HasValue)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

        /// <summary>
        /// Asigna la descripción de omisión a cada tipo de precio.
        /// </summary>
        /// <param name="precios">Listado de tipos de precio.</param>
        private static void CompletarOmisionPrecios(IEnumerable<Precio> precios)
        {
            foreach (Precio dt in precios)
            {
                dt.omision = dt.activo ? "APLICA" : "NO_APLICA";
            }
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene la lista paginada de tipos de precio.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Listado de tipos de precio.</returns>
        public ErrorDto<PreciosDataLista> Precios_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(QueryTotalPrecios);

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder(QueryPreciosBase);

                AgregarFiltroPrecios(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY cod_precio ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Precios = connection.Query<Precio>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return CrearRespuestaLista(result);
        }

        /// <summary>
        /// Obtiene la lista completa de tipos de precio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de tipos de precio.</returns>
        public ErrorDto<List<Precio>> Precios_ObtenerTodos(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<Precio>(
                CreatePortalDb(),
                CodEmpresa,
                QueryPreciosTodos);

            if (result.Code == 0 && result.Result is not null)
            {
                CompletarOmisionPrecios(result.Result);
            }

            return CrearRespuestaTodos(result);
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza el tipo de precio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del tipo de precio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Precios_Actualizar(int CodEmpresa, Precio request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryActualizarPrecio,
                CrearParametrosPrecio(request));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorActualizarPrecio);
        }

        /// <summary>
        /// Inserta el tipo de precio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del tipo de precio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Precios_Insertar(int CodEmpresa, Precio request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryInsertarPrecio,
                CrearParametrosPrecio(request));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorInsertarPrecio);
        }

        /// <summary>
        /// Elimina el tipo de precio.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="precio">Código del tipo de precio.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Precios_Eliminar(int CodEmpresa, string precio)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryEliminarPrecio,
                CrearParametrosEliminar(precio));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorEliminarPrecio);
        }

        #endregion
    }
}