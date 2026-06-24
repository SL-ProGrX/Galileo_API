using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTomaFisicaDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTomaFisicaDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTomaFisicaDB(IConfiguration config)
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
        /// Crea una respuesta estándar para operaciones de consulta única.
        /// </summary>
        /// <typeparam name="T">Tipo del resultado esperado.</typeparam>
        /// <param name="result">Resultado devuelto por <see cref="DbHelper"/>.</param>
        /// <param name="errorMessage">Mensaje cuando ocurre un error.</param>
        /// <returns>Respuesta estándar para una sola entidad.</returns>
        private static ErrorDto<T> CrearRespuestaSingle<T>(ErrorDto<T?> result, string errorMessage)
            where T : class, new()
        {
            if (result.Code != 0)
            {
                return new ErrorDto<T>
                {
                    Code = result.Code,
                    Description = result.Description ?? errorMessage,
                    Result = new T()
                };
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : new ErrorDto<T>
                {
                    Code = -2,
                    Description = errorMessage,
                    Result = new T()
                };
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a una consulta.
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
        /// Agrega un filtro LIKE al detalle de tomas físicas.
        /// </summary>
        /// <param name="filtro">Texto del filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroDetalle(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(@" AND (
                                    d.COD_PRODUCTO LIKE @Filtro
                                    OR p.DESCRIPCION LIKE @Filtro
                                    OR d.UBICACION LIKE @Filtro
                                    OR b.DESCRIPCION LIKE @Filtro
                                  ) ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo disponible para una toma física.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Siguiente consecutivo.</returns>
        private static int ObtenerSiguienteConsecutivo(IDbConnection connection)
        {
            return connection.QueryFirstOrDefault<int>("SELECT ISNULL(MAX(consecutivo), 0) + 1 FROM PV_INVTOMAFISICA");
        }

        /// <summary>
        /// Verifica si existe un detalle para un producto y consecutivo dados.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="consecutivo">Consecutivo de la toma física.</param>
        /// <param name="codProducto">Código del producto.</param>
        /// <returns>Total de registros encontrados.</returns>
        private static int ContarDetalleProducto(IDbConnection connection, int consecutivo, string codProducto)
        {
            return connection.QueryFirstOrDefault<int>(
                "SELECT COUNT(*) FROM pv_invTF_Detalle WHERE cod_producto = @Cod_Producto AND CONSECUTIVO = @Consecutivo",
                new { Cod_Producto = codProducto, Consecutivo = consecutivo });
        }

        /// <summary>
        /// Verifica si existe un detalle para un producto, consecutivo y ubicación dados.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="linea">Línea a validar.</param>
        /// <returns>Total de registros encontrados.</returns>
        private static int ContarDetalleProductoUbicacion(IDbConnection connection, TomaFisicaDetalleDto linea)
        {
            return connection.QueryFirstOrDefault<int>(
                @"SELECT COUNT(*)
                  FROM pv_invTF_Detalle
                  WHERE cod_producto = @Cod_Producto
                    AND CONSECUTIVO = @Consecutivo
                    AND UBICACION = @Ubicacion",
                new
                {
                    linea.Cod_Producto,
                    Consecutivo = linea.consecutivo,
                    linea.Ubicacion
                });
        }

        /// <summary>
        /// Inserta el encabezado de una toma física.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="consecutivo">Consecutivo a insertar.</param>
        /// <param name="data">Datos de la toma física.</param>
        private static void InsertarTomaFisica(IDbConnection connection, int consecutivo, TomaFisicaDto data)
        {
            connection.Execute(
                @"INSERT INTO PV_INVTOMAFISICA(CONSECUTIVO, COD_BODEGA, FECHA_INICIO, FECHA_CORTE, ESTADO, FECHA_CREA, USER_CREA, NOTAS)
                  VALUES(@Consecutivo, @Cod_Bodega, @Fecha_Inicio, @Fecha_Corte, 'S', @Fecha_Crea, @User_Crea, @Notas)",
                new
                {
                    Consecutivo = consecutivo,
                    data.Cod_Bodega,
                    data.Fecha_Inicio,
                    data.Fecha_Corte,
                    Fecha_Crea = DateTime.Now,
                    data.User_Crea,
                    Notas = data.notas
                });
        }

        /// <summary>
        /// Inserta un detalle de toma física.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="data">Datos del detalle.</param>
        private static void InsertarDetalleTomaFisica(IDbConnection connection, TomaFisicaDetalleDto data)
        {
            connection.Execute(
                @"INSERT INTO pv_invTF_Detalle(CONSECUTIVO, COD_BODEGA, COD_PRODUCTO, EXISTENCIA_LOGICA, EXISTENCIA_FISICA, UBICACION)
                  VALUES(@Consecutivo, @Cod_Bodega, @Cod_Producto, @Existencia_Logica, @Existencia_Fisica, @Ubicacion)",
                new
                {
                    Consecutivo = data.consecutivo,
                    data.Cod_Bodega,
                    data.Cod_Producto,
                    data.Existencia_Logica,
                    data.Existencia_Fisica,
                    data.Ubicacion
                });
        }

        /// <summary>
        /// Actualiza un detalle de toma física.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="request">Datos del detalle.</param>
        private static void ActualizarDetalleTomaFisica(IDbConnection connection, TomaFisicaDetalleDto request)
        {
            connection.Execute(
                @"UPDATE pv_invTF_Detalle
                  SET COD_BODEGA = @Cod_Bodega,
                      EXISTENCIA_LOGICA = @Existencia_Logica,
                      EXISTENCIA_FISICA = @Existencia_Fisica,
                      UBICACION = @Ubicacion
                  WHERE CONSECUTIVO = @Consecutivo
                    AND COD_PRODUCTO = @Cod_Producto",
                new
                {
                    request.Cod_Bodega,
                    request.Existencia_Logica,
                    request.Existencia_Fisica,
                    request.Ubicacion,
                    Consecutivo = request.consecutivo,
                    request.Cod_Producto
                });
        }

        /// <summary>
        /// Obtiene la consulta del producto por código o código de barras.
        /// </summary>
        /// <param name="tipo">Tipo de búsqueda.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string ObtenerQueryProductoPorBarras(string tipo)
        {
            return tipo == "CB"
                ? @"SELECT pp.COD_PRODUCTO,
                            pp.DESCRIPCION,
                            pp.TIPO_PRODUCTO as TIPO,
                            SUM(CASE WHEN im.TIPO = 'E' THEN im.CANTIDAD ELSE 0 END) -
                            SUM(CASE WHEN im.TIPO = 'S' THEN im.CANTIDAD ELSE 0 END) AS existencia_Logica
                     FROM PV_PRODUCTOS pp
                     LEFT JOIN PV_INVENTARIO_MOV im ON pp.COD_PRODUCTO = im.COD_PRODUCTO AND im.COD_BODEGA = @cod_bodega
                     WHERE pp.COD_BARRAS = @codigo
                     GROUP BY pp.COD_PRODUCTO, pp.DESCRIPCION, pp.TIPO_PRODUCTO"
                : @"SELECT pp.COD_PRODUCTO,
                            pp.DESCRIPCION,
                            pp.TIPO_PRODUCTO as TIPO,
                            SUM(CASE WHEN im.TIPO = 'E' THEN im.CANTIDAD ELSE 0 END) -
                            SUM(CASE WHEN im.TIPO = 'S' THEN im.CANTIDAD ELSE 0 END) AS existencia_Logica
                     FROM PV_PRODUCTOS pp
                     LEFT JOIN PV_INVENTARIO_MOV im ON pp.COD_PRODUCTO = im.COD_PRODUCTO AND im.COD_BODEGA = @cod_bodega
                     WHERE pp.COD_PRODUCTO = @codigo
                     GROUP BY pp.COD_PRODUCTO, pp.DESCRIPCION, pp.TIPO_PRODUCTO";
        }

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene las tomas físicas registradas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Proveedor">Parámetro conservado por compatibilidad.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <returns>Listado de tomas físicas.</returns>
        public ErrorDto<List<TomaFisicaDto>> TomaFisica_Obtener(int CodEmpresa, int Cod_Proveedor, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder("SELECT * FROM pv_InvTomaFisica");

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    queryBuilder.Append(@" WHERE CONVERT(varchar(20), CONSECUTIVO) LIKE @Filtro
                                           OR COD_BODEGA LIKE @Filtro
                                           OR USER_CREA LIKE @Filtro
                                           OR NOTAS LIKE @Filtro ");
                    parametros.Add("Filtro", $"%{filtro.Trim()}%");
                }

                queryBuilder.Append(" ORDER BY CONSECUTIVO DESC ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);
                return connection.Query<TomaFisicaDto>(queryBuilder.ToString(), parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TomaFisicaDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener tomas físicas.", result.Code.GetValueOrDefault(-1), new List<TomaFisicaDto>());
        }

        /// <summary>
        /// Obtiene el detalle de una toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma física.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro de búsqueda.</param>
        /// <returns>Listado de detalles de la toma física.</returns>
        public ErrorDto<List<TomaFisicaDetalleDto>> tomasFisicasDetalle_Obtener(int CodEmpresa, int consecutivo, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = new DynamicParameters();
                parametros.Add("Consecutivo", consecutivo);
                var queryBuilder = new StringBuilder(@"SELECT
                                    d.consecutivo,
                                    d.Cod_Bodega,
                                    d.Cod_Producto,
                                    d.Existencia_Logica,
                                    d.Existencia_Fisica,
                                    d.Ubicacion,
                                    p.Descripcion,
                                    p.tipo_producto as tipo,
                                    b.descripcion as bodega
                                FROM pv_invTF_Detalle d
                                INNER JOIN PV_PRODUCTOS p ON d.COD_PRODUCTO = p.COD_PRODUCTO
                                INNER JOIN PV_BODEGAS b ON d.COD_BODEGA = b.COD_BODEGA
                                WHERE d.consecutivo = @Consecutivo");

                AgregarFiltroDetalle(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY d.COD_PRODUCTO ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);
                return connection.Query<TomaFisicaDetalleDto>(queryBuilder.ToString(), parametros).ToList();
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TomaFisicaDetalleDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el detalle de la toma física.", result.Code.GetValueOrDefault(-1), new List<TomaFisicaDetalleDto>());
        }

        /// <summary>
        /// Consulta el siguiente o anterior consecutivo de toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Consecutivo actual.</param>
        /// <param name="tipo">Dirección del desplazamiento.</param>
        /// <returns>Consecutivo encontrado.</returns>
        public ErrorDto<TomaFisicaDto> ConsultaAscDesc(int CodEmpresa, int consecutivo, string tipo)
        {
            string query;
            object? parametros = null;

            if (tipo == "desc")
            {
                if (consecutivo == 0)
                {
                    query = "select Top 1 consecutivo from PV_INVTOMAFISICA order by consecutivo desc";
                }
                else
                {
                    query = "select Top 1 consecutivo from PV_INVTOMAFISICA where consecutivo < @Consecutivo order by consecutivo desc";
                    parametros = new { Consecutivo = consecutivo };
                }
            }
            else
            {
                query = "select Top 1 consecutivo from PV_INVTOMAFISICA where consecutivo > @Consecutivo order by consecutivo asc";
                parametros = new { Consecutivo = consecutivo };
            }

            var result = DbHelper.ExecuteSingleQuery<TomaFisicaDto>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new TomaFisicaDto(),
                parametros);

            return CrearRespuestaSingle(result, "Error al consultar la toma física.");
        }

        /// <summary>
        /// Obtiene una toma física por consecutivo.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma física.</param>
        /// <returns>Toma física encontrada.</returns>
        public ErrorDto<TomaFisicaDto> tomaFisicaConsecutivo_Obtener(int CodEmpresa, int consecutivo)
        {
            var result = DbHelper.ExecuteSingleQuery<TomaFisicaDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM PV_INVTOMAFISICA WHERE CONSECUTIVO = @Consecutivo",
                new TomaFisicaDto(),
                new { Consecutivo = consecutivo });

            return CrearRespuestaSingle(result, "Error al obtener la toma física por consecutivo.");
        }

        /// <summary>
        /// Obtiene un producto para toma física por código de barras o código de producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_bodega">Código de la bodega.</param>
        /// <param name="cod_barras">Código de barras o código del producto.</param>
        /// <param name="tipo">Tipo de búsqueda.</param>
        /// <returns>Producto encontrado.</returns>
        public ErrorDto<TomaFisicaDetalleDto> TomaFisicaProdBarras_Obtener(int CodEmpresa, string cod_bodega, string cod_barras, string tipo)
        {
            var result = DbHelper.ExecuteSingleQuery<TomaFisicaDetalleDto>(
                CreatePortalDb(),
                CodEmpresa,
                ObtenerQueryProductoPorBarras(tipo),
                new TomaFisicaDetalleDto(),
                new
                {
                    cod_bodega,
                    codigo = cod_barras
                });

            if (result.Code == 0 && result.Result is not null)
            {
                return DbHelper.CreateOkResponse(result.Result);
            }

            return new ErrorDto<TomaFisicaDetalleDto>
            {
                Code = -1,
                Description = result.Description ?? "No existe Producto con este codigo",
                Result = new TomaFisicaDetalleDto()
            };
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Inserta una nueva toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos de la toma física.</param>
        /// <returns>Resultado de la operación. El consecutivo generado se devuelve en Description.</returns>
        public ErrorDto tomaFisica_Insertar(int CodEmpresa, TomaFisicaDto data)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                int ultimaBoleta = ObtenerSiguienteConsecutivo(connection);
                int existe = connection.QueryFirstOrDefault<int>(
                    "SELECT COUNT(*) FROM pv_invTF_Detalle WHERE consecutivo = @Consecutivo",
                    new { Consecutivo = data.consecutivo });

                if (existe >= 1)
                {
                    return new ErrorDto
                    {
                        Code = -1,
                        Description = "Ya existe el No. Boleta, por favor verifique"
                    };
                }

                InsertarTomaFisica(connection, ultimaBoleta, data);
                return new ErrorDto
                {
                    Code = 0,
                    Description = ultimaBoleta.ToString()
                };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la toma física.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta un detalle de toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="data">Datos del detalle.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto tomaFisicaDetalle_Insertar(int CodEmpresa, TomaFisicaDetalleDto data)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                InsertarDetalleTomaFisica(connection, data);
                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el detalle de la toma física.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el encabezado de una toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la toma física.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto actualizarTomaFisica(int CodEmpresa, TomaFisicaDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE PV_INVTOMAFISICA
                  SET COD_BODEGA = @Cod_Bodega,
                      FECHA_CREA = @Fecha_Inicio,
                      FECHA_APLICA = @Fecha_Corte,
                      NOTAS = @Notas
                  WHERE consecutivo = @Consecutivo",
                new
                {
                    request.Cod_Bodega,
                    request.Fecha_Inicio,
                    request.Fecha_Corte,
                    Notas = request.notas,
                    Consecutivo = request.consecutivo
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar la toma física.");
        }

        /// <summary>
        /// Actualiza o inserta el detalle de una toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del detalle.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto actualizarTomaFisicaDetalle(int CodEmpresa, TomaFisicaDetalleDto request)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                int existe = ContarDetalleProducto(connection, request.consecutivo, request.Cod_Producto);

                if (existe >= 1)
                {
                    ActualizarDetalleTomaFisica(connection, request);
                    return new ErrorDto { Code = 0, Description = "Ok" };
                }

                InsertarDetalleTomaFisica(connection, request);
                return new ErrorDto { Code = 0, Description = "Ok" };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar el detalle de la toma física.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un detalle de toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma física.</param>
        /// <param name="cod_producto">Código del producto.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto EliminarDetalleTomaFisica(int CodEmpresa, int consecutivo, string cod_producto)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE FROM pv_invTF_Detalle WHERE CONSECUTIVO = @Consecutivo AND cod_producto = @cod_producto",
                new { Consecutivo = consecutivo, cod_producto });

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar el detalle de la toma física.");
        }

        /// <summary>
        /// Elimina una toma física y su detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="consecutivo">Consecutivo de la toma física.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto EliminarTomaFisica(int CodEmpresa, int consecutivo)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute("DELETE FROM pv_invTF_Detalle WHERE CONSECUTIVO = @Consecutivo", new { Consecutivo = consecutivo });
                connection.Execute("DELETE FROM PV_INVTOMAFISICA WHERE CONSECUTIVO = @Consecutivo", new { Consecutivo = consecutivo });
                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar la toma física.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Guarda un producto escaneado en una toma física.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="linea">Datos de la línea a guardar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto TomaFisicaBarras_Guardar(int CodEmpresa, TomaFisicaDetalleDto linea)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                int existe = ContarDetalleProductoUbicacion(connection, linea);
                if (existe == 0)
                {
                    InsertarDetalleTomaFisica(connection, linea);
                    return new ErrorDto { Code = 0, Description = "Ok" };
                }

                return new ErrorDto
                {
                    Code = -1,
                    Description = "Producto ya se encuentra en la boleta registrado"
                };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar el producto en toma física.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}