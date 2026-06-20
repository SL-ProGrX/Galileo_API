using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTiposMarcasDB
    {
        private readonly IConfiguration _config;

        private const string MensajeOk = "Ok";
        private const string ErrorObtenerMarcas = "Error al obtener marcas.";
        private const string ErrorObtenerTodasMarcas = "Error al obtener todas las marcas.";
        private const string ErrorActualizarMarca = "Error al actualizar la marca.";
        private const string ErrorInsertarMarca = "Error al insertar la marca.";
        private const string ErrorEliminarMarca = "Error al eliminar la marca.";
        private const string QueryTotalMarcas = "SELECT COUNT(*) FROM pv_marcas";
        private const string QueryMarcasBase = @"SELECT cod_marca,
                                                             descripcion,
                                                             activo
                                                      FROM pv_marcas";
        private const string QueryMarcasTodas = "SELECT cod_marca, descripcion, activo FROM pv_marcas ORDER BY cod_marca";
        private const string QueryActualizarMarca = "UPDATE pv_marcas SET descripcion = @Descripcion, activo = @Activo WHERE Cod_Marca = @Cod_Marca";
        private const string QueryInsertarMarca = "INSERT INTO pv_marcas(cod_marca, descripcion, activo) VALUES(@Cod_Marca, @Descripcion, @Activo)";
        private const string QueryEliminarMarca = "DELETE pv_marcas WHERE Cod_Marca = @Cod_Marca";

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTiposMarcasDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTiposMarcasDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de marcas.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static MarcasDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Marcas = new List<MarcasDto>()
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
        /// Crea una respuesta estándar para el listado paginado de marcas.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <returns>Respuesta de listado paginado.</returns>
        private static ErrorDto<MarcasDataLista> CrearRespuestaLista(ErrorDto<MarcasDataLista> result)
        {
            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerMarcas, result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Crea una respuesta estándar para listados completos de marcas.
        /// </summary>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <returns>Respuesta de listado completo.</returns>
        private static ErrorDto<List<MarcasDto>> CrearRespuestaTodas(ErrorDto<List<MarcasDto>> result)
        {
            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<MarcasDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? ErrorObtenerTodasMarcas, result.Code.GetValueOrDefault(-1), new List<MarcasDto>());
        }

        /// <summary>
        /// Crea los parámetros comunes para insertar o actualizar marcas.
        /// </summary>
        /// <param name="request">Datos de la marca.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosMarca(MarcasDto request) => new
        {
            request.Cod_Marca,
            request.Descripcion,
            request.Activo
        };

        /// <summary>
        /// Crea los parámetros para eliminar una marca.
        /// </summary>
        /// <param name="marca">Código de marca.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosEliminar(string marca) => new
        {
            Cod_Marca = marca
        };

        /// <summary>
        /// Agrega filtro LIKE al listado de marcas.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroMarcas(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" WHERE cod_marca LIKE @Filtro OR DESCRIPCION LIKE @Filtro ");
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
        /// Asigna la descripción del estado a cada marca.
        /// </summary>
        /// <param name="marcas">Listado de marcas.</param>
        private static void CompletarEstadoMarcas(IEnumerable<MarcasDto> marcas)
        {
            foreach (MarcasDto dt in marcas)
            {
                dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";
            }
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene la lista paginada de marcas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Listado de marcas.</returns>
        public ErrorDto<MarcasDataLista> Marcas_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(QueryTotalMarcas);

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder(QueryMarcasBase);

                AgregarFiltroMarcas(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY cod_marca ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Marcas = connection.Query<MarcasDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return CrearRespuestaLista(result);
        }

        /// <summary>
        /// Obtiene la lista completa de marcas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de marcas.</returns>
        public ErrorDto<List<MarcasDto>> Marcas_ObtenerTodos(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<MarcasDto>(
                CreatePortalDb(),
                CodEmpresa,
                QueryMarcasTodas);

            if (result.Code == 0 && result.Result is not null)
            {
                CompletarEstadoMarcas(result.Result);
            }

            return CrearRespuestaTodas(result);
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza la marca.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la marca.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Marcas_Actualizar(int CodEmpresa, MarcasDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryActualizarMarca,
                CrearParametrosMarca(request));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorActualizarMarca);
        }

        /// <summary>
        /// Inserta la marca.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la marca.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Marcas_Insertar(int CodEmpresa, MarcasDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryInsertarMarca,
                CrearParametrosMarca(request));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorInsertarMarca);
        }

        /// <summary>
        /// Elimina la marca.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="marca">Código de la marca.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Marcas_Eliminar(int CodEmpresa, string marca)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                QueryEliminarMarca,
                CrearParametrosEliminar(marca));

            return CrearRespuestaNonQuery(result, MensajeOk, ErrorEliminarMarca);
        }

        #endregion
    }
}