using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;

namespace Galileo.DataBaseTier
{
    public class FrmInvDepartamentosDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvDepartamentosDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvDepartamentosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de departamentos.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static DepartamentosDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Departamentos = new List<DepartamentosDto>()
        };


        /// <summary>
        /// Agrega un filtro LIKE al listado de departamentos.
        /// </summary>
        /// <param name="filtro">Texto a filtrar.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroDepartamentos(string? filtro, System.Text.StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" WHERE COD_departamento LIKE @Filtro OR DESCRIPCION LIKE @Filtro");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH al listado de departamentos.
        /// </summary>
        /// <param name="pagina">Fila inicial.</param>
        /// <param name="paginacion">Cantidad de filas.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacion(int? pagina, int? paginacion, System.Text.StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (!pagina.HasValue || !paginacion.HasValue)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

        /// <summary>
        /// Crea los parámetros comunes para un departamento.
        /// </summary>
        /// <param name="codDepartamento">Código del departamento.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosDepartamento(string codDepartamento) => new
        {
            Cod_Departamento = codDepartamento,
            cod_departamento = codDepartamento
        };

        /// <summary>
        /// Crea los parámetros comunes para una asignación departamento-línea.
        /// </summary>
        /// <param name="codDepartamento">Código del departamento.</param>
        /// <param name="codProdclas">Código de clasificación.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosAsignacion(string codDepartamento, string codProdclas) => new
        {
            Cod_Departamento = codDepartamento,
            cod_departamento = codDepartamento,
            Cod_Prodclas = codProdclas,
            cod_prodclas = codProdclas
        };

        #endregion

        #region Departamentos

        /// <summary>
        /// Obtiene la lista paginada de departamentos.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Listado de departamentos.</returns>
        public ErrorDto<DepartamentosDataLista> Departamentos_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                var parametros = new DynamicParameters();
                var totalQuery = "SELECT COUNT(*) FROM pv_Departamentos";
                respuesta.Total = connection.QueryFirstOrDefault<int>(totalQuery);

                var detalleQuery = new System.Text.StringBuilder(@"SELECT COD_departamento,
                                                                         descripcion,
                                                                         activo
                                                                  FROM pv_Departamentos");

                AgregarFiltroDepartamentos(filtro, detalleQuery, parametros);
                detalleQuery.Append(" ORDER BY COD_departamento");
                AgregarPaginacion(pagina, paginacion, detalleQuery, parametros);

                respuesta.Departamentos = connection.Query<DepartamentosDto>(detalleQuery.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener departamentos.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Actualiza el departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del departamento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Departamentos_Actualizar(int CodEmpresa, DepartamentosDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE pv_Departamentos
                  SET descripcion = @Descripcion,
                      activo = @Activo
                  WHERE cod_departamento = @Cod_Departamento",
                new
                {
                    request.Cod_Departamento,
                    request.Descripcion,
                    request.Activo
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el departamento.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un nuevo departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del departamento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Departamentos_Insertar(int CodEmpresa, DepartamentosDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"INSERT INTO pv_Departamentos(cod_departamento, descripcion, activo)
                  VALUES(@Cod_Departamento, @Descripcion, @Activo)",
                new
                {
                    request.Cod_Departamento,
                    request.Descripcion,
                    request.Activo
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el departamento.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="departamento">Código del departamento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Departamentos_Eliminar(int CodEmpresa, string departamento)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_Departamentos WHERE cod_departamento = @cod_departamento",
                CrearParametrosDepartamento(departamento));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar el departamento.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Asignaciones

        /// <summary>
        /// Obtiene todas las asignaciones de líneas para un departamento.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="departamento">Código del departamento.</param>
        /// <returns>Listado de asignaciones.</returns>
        public ErrorDto<List<AsignacionesDto>> Asignaciones_ObtenerTodos(int CodEmpresa, string departamento)
        {
            return DbHelper.ExecuteListQuery<AsignacionesDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT c.cod_prodclas,
                         c.descripcion,
                         c.cod_alter,
                         c.costeo,
                         c.cod_cuenta,
                         c.valuacion,
                         l.cod_departamento
                  FROM pv_prod_clasifica c
                  LEFT JOIN pv_lineasdep l ON c.cod_prodclas = l.cod_prodclas
                                          AND l.cod_departamento = @cod_departamento
                  ORDER BY l.cod_departamento DESC",
                CrearParametrosDepartamento(departamento));
        }

        /// <summary>
        /// Inserta una asignación departamento-línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la asignación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Asignaciones_Insertar(int CodEmpresa, AsignacionesDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "INSERT PV_LINEASDEP(cod_departamento, cod_prodclas) VALUES(@Cod_Departamento, @Cod_Prodclas)",
                CrearParametrosAsignacion(request.Cod_Departamento, request.Cod_Prodclas));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la asignación.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina una asignación departamento-línea.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Departamento">Código del departamento.</param>
        /// <param name="Cod_Prodclas">Código de clasificación.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Asignaciones_Eliminar(int CodEmpresa, string Cod_Departamento, string Cod_Prodclas)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE PV_LINEASDEP WHERE cod_departamento = @cod_departamento AND cod_prodclas = @cod_prodclas",
                CrearParametrosAsignacion(Cod_Departamento, Cod_Prodclas));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar la asignación.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}