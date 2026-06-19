using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvUnidadesDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvUnidadesDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvUnidadesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de unidades.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static UnidadesDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Unidades = new List<UnidadMedicionDto>()
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
        /// Agrega filtro LIKE al listado de unidades.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroUnidades(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" WHERE COD_UNIDAD LIKE @Filtro OR DESCRIPCION LIKE @Filtro ");
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
        /// Completa la descripción de estado en cada unidad.
        /// </summary>
        /// <param name="unidades">Listado de unidades.</param>
        private static void CompletarEstadoUnidades(IEnumerable<UnidadMedicionDto> unidades)
        {
            foreach (UnidadMedicionDto dt in unidades)
            {
                dt.Estado = dt.Activo ? "ACTIVO" : "INACTIVO";
            }
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene la lista lazy de unidades.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Listado de unidades.</returns>
        public ErrorDto<UnidadesDataLista> UnidadMedicion_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM pv_unidades");

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder(@"SELECT cod_unidad,
                                                             descripcion,
                                                             ISNULL(Unidad_Hacienda_Id, 'Unid') as hacienda,
                                                             activo
                                                      FROM pv_unidades");

                AgregarFiltroUnidades(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY COD_unidad ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Unidades = connection.Query<UnidadMedicionDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener unidades de medición.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Obtiene la lista de unidades de medición con detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado detallado de unidades.</returns>
        public ErrorDto<List<UnidadMedicionDto>> UnidadMedicion_ObtenerTodosDetalle(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<UnidadMedicionDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_unidad, descripcion, ISNULL(Unidad_Hacienda_Id, 'Unid') as hacienda, activo FROM pv_unidades ORDER BY cod_unidad");

            if (result.Code == 0 && result.Result is not null)
            {
                CompletarEstadoUnidades(result.Result);
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<UnidadMedicionDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el detalle de unidades.", result.Code.GetValueOrDefault(-1), new List<UnidadMedicionDto>());
        }

        /// <summary>
        /// Obtiene lista de unidades de medición para select.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de unidades para selección.</returns>
        public ErrorDto<List<UnidadMedicion>> UnidadMedicion_ObtenerTodos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<UnidadMedicion>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT cod_unidad, descripcion FROM pv_unidades ORDER BY cod_unidad");
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Actualiza la unidad de medición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la unidad.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UnidadMedicion_Actualizar(int CodEmpresa, UnidadMedicionDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "Update pv_unidades set descripcion = @Descripcion, activo = @Activo, Unidad_Hacienda_Id = @Hacienda where Cod_Unidad = @Cod_Unidad",
                new
                {
                    request.Cod_Unidad,
                    request.Descripcion,
                    request.Activo,
                    Hacienda = request.Hacienda
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar la unidad de medición.");
        }

        /// <summary>
        /// Agrega una unidad de medición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la unidad.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UnidadMedicion_Agregar(int CodEmpresa, UnidadMedicionDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"insert into pv_unidades(cod_unidad, descripcion, Unidad_Hacienda_Id, activo, registro_fecha, registro_usuario)
                  values(@Cod_Unidad, @Descripcion, @Hacienda, @Activo, GETDATE(), @Registro_Usuario)",
                new
                {
                    request.Cod_Unidad,
                    request.Descripcion,
                    request.Activo,
                    Hacienda = request.Hacienda,
                    request.Registro_Usuario
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al agregar la unidad de medición.");
        }

        /// <summary>
        /// Elimina una unidad de medición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="unidad">Código de la unidad.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UnidadMedicion_Eliminar(int CodEmpresa, string unidad)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_unidades where Cod_Unidad = @Cod_Unidad",
                new { Cod_Unidad = unidad });

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar la unidad de medición.");
        }

        #endregion
    }
}