using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvPaquetesDB
    {
        private readonly IConfiguration _config;
        private const string DateFormatYMD = "yyyy-MM-dd";

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvPaquetesDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvPaquetesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de paquetes.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static PaqueteDataLista CrearListaVacia() => new()
        {
            Total = 0,
            Lista = new List<PaqueteDto>()
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
        /// Crea una respuesta estándar para consultas únicas.
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
        /// Agrega filtro LIKE al listado de paquetes.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroPaquetes(string? filtro, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" WHERE cod_paquete LIKE @Filtro OR DESCRIPCION LIKE @Filtro ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
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
        /// Crea parámetros para código de paquete.
        /// </summary>
        /// <param name="codPaquete">Código del paquete.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosPaquete(int codPaquete) => new
        {
            Cod_Paquete = codPaquete,
            cod_paquete = codPaquete
        };

        /// <summary>
        /// Crea parámetros para línea de detalle.
        /// </summary>
        /// <param name="linea">Línea del detalle.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosLinea(int linea) => new
        {
            Linea = linea,
            linea
        };

        /// <summary>
        /// Obtiene la unidad del producto asociado al detalle.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codProducto">Código del producto.</param>
        /// <returns>Unidad encontrada o cadena vacía.</returns>
        private static string ObtenerUnidadProducto(IDbConnection connection, string codProducto)
        {
            return connection.QuerySingleOrDefault<string>(
                "SELECT COD_UNIDAD FROM PV_PRODUCTOS WHERE COD_PRODUCTO = @Cod_Producto",
                new { Cod_Producto = codProducto }) ?? string.Empty;
        }

        /// <summary>
        /// Completa la unidad de cada producto del detalle del paquete.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="detalles">Lista de detalles del paquete.</param>
        private static void CompletarUnidadDetalles(IDbConnection connection, IEnumerable<PaqueteDetalleDto> detalles)
        {
            foreach (PaqueteDetalleDto item in detalles)
            {
                item.unidad = ObtenerUnidadProducto(connection, item.Cod_Producto);
            }
        }

        /// <summary>
        /// Crea los parámetros del procedimiento de cabecera para insertar paquete.
        /// </summary>
        /// <param name="request">Datos del paquete.</param>
        /// <returns>Parámetros Dapper inicializados.</returns>
        private static DynamicParameters CrearParametrosInsertarPaquete(PaqueteDto request)
        {
            string? fechaInicio = MProGrXAuxiliarDB.validaFechaGlobal(request.Fecha_Inicio, DateFormatYMD);
            string? fechaCorte = MProGrXAuxiliarDB.validaFechaGlobal(request.Fecha_Corte, DateFormatYMD);
            string? frecuenciaHorai = MProGrXAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horai, "HH:mm:ss");
            string? frecuenciaHorac = MProGrXAuxiliarDB.validaFechaGlobal(request.Frecuencia_Horac, "HH:mm:ss");

            var parameters = new DynamicParameters();
            parameters.Add("Descripcion", request.Descripcion);
            parameters.Add("Notas", request.Notas);
            parameters.Add("User_Crea", request.User_Crea);
            parameters.Add("Fecha_Inicio", fechaInicio);
            parameters.Add("Fecha_Corte", fechaCorte);
            parameters.Add("Frecuencia_Horai", frecuenciaHorai);
            parameters.Add("Frecuencia_Horac", frecuenciaHorac);
            parameters.Add("Frecuencia_Lunes", request.Frecuencia_Lunes);
            parameters.Add("Frecuencia_Martes", request.Frecuencia_Martes);
            parameters.Add("Frecuencia_Miercoles", request.Frecuencia_Miercoles);
            parameters.Add("Frecuencia_Jueves", request.Frecuencia_Jueves);
            parameters.Add("Frecuencia_Viernes", request.Frecuencia_Viernes);
            parameters.Add("Frecuencia_Sabado", request.Frecuencia_Sabado);
            parameters.Add("Frecuencia_Domingo", request.Frecuencia_Domingo);
            parameters.Add("NewID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            return parameters;
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado que devuelve un código entero y lo transforma en <see cref="ErrorDto"/>.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="procedure">Nombre del procedimiento almacenado.</param>
        /// <param name="values">Parámetros del procedimiento.</param>
        /// <param name="errorMessage">Mensaje de error estándar.</param>
        /// <returns>Respuesta estándar con el resultado del procedimiento.</returns>
        private ErrorDto EjecutarProcedimientoConCodigo(int CodEmpresa, string procedure, object values, string errorMessage)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(procedure, values, commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? errorMessage, result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(errorMessage, result.Result);
        }

        #endregion

        #region Paquetes

        /// <summary>
        /// Obtiene la lista paginada de paquetes.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por código o descripción.</param>
        /// <returns>Listado de paquetes.</returns>
        public ErrorDto<PaqueteDataLista> Paquetes_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM pv_paquetes");

                var parametros = new DynamicParameters();
                var queryBuilder = new StringBuilder("SELECT * FROM pv_paquetes");
                AgregarFiltroPaquetes(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY cod_paquete ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Lista = connection.Query<PaqueteDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener paquetes.", result.Code.GetValueOrDefault(-1), CrearListaVacia());
        }

        /// <summary>
        /// Obtiene todos los paquetes.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado completo de paquetes.</returns>
        public ErrorDto<List<PaqueteDto>> Paquetes_ObtenerTodos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<PaqueteDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM pv_paquetes");
        }

        /// <summary>
        /// Obtiene un paquete por su código.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Paquete">Código del paquete.</param>
        /// <returns>Paquete encontrado.</returns>
        public ErrorDto<PaqueteDto> Paquete_Obtener(int CodEmpresa, int Cod_Paquete)
        {
            var result = DbHelper.ExecuteSingleQuery<PaqueteDto>(
                CreatePortalDb(),
                CodEmpresa,
                "SELECT * FROM pv_paquetes WHERE cod_paquete = @Cod_Paquete",
                null,
                CrearParametrosPaquete(Cod_Paquete));

            return CrearRespuestaSingle(result, "Error al obtener el paquete.", "No se encontró el paquete indicado.");
        }

        /// <summary>
        /// Obtiene el detalle de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="Cod_Paquete">Código del paquete.</param>
        /// <returns>Listado de detalles del paquete.</returns>
        public ErrorDto<List<PaqueteDetalleDto>> Paquete_ObtenerDetalles(int CodEmpresa, int Cod_Paquete)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var detalles = connection.Query<PaqueteDetalleDto>(
                    @"SELECT D.linea,
                             D.cod_producto,
                             P.descripcion,
                             D.cantidad,
                             D.porc_utilidad,
                             D.precio,
                             D.imp_ventas,
                             (D.cantidad * (D.precio + D.precio * D.porc_utilidad / 100))
                             + ((D.cantidad * (D.precio + D.precio * D.porc_utilidad / 100)) * (D.imp_ventas / 100)) AS Total
                      FROM pv_paquetes_detalle D
                      INNER JOIN pv_productos P ON D.cod_producto = P.cod_producto
                      WHERE D.cod_paquete = @Cod_Paquete
                      ORDER BY D.Linea",
                    CrearParametrosPaquete(Cod_Paquete)).ToList();

                CompletarUnidadDetalles(connection, detalles);
                return detalles;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<PaqueteDetalleDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el detalle del paquete.", result.Code.GetValueOrDefault(-1), new List<PaqueteDetalleDto>());
        }

        /// <summary>
        /// Actualiza un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del paquete.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Paquete_Actualizar(int CodEmpresa, PaqueteDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"UPDATE pv_paquetes
                  SET descripcion = @Descripcion,
                      notas = @Notas,
                      user_modifica = @User_Modifica,
                      fecha_modifica = @Fecha_Modifica,
                      fecha_inicio = @Fecha_Inicio,
                      fecha_corte = @Fecha_Corte,
                      frecuencia_horai = @Frecuencia_Horai,
                      frecuencia_horac = @Frecuencia_Horac,
                      frecuencia_lunes = @Frecuencia_Lunes,
                      frecuencia_martes = @Frecuencia_Martes,
                      frecuencia_miercoles = @Frecuencia_Miercoles,
                      frecuencia_jueves = @Frecuencia_Jueves,
                      frecuencia_viernes = @Frecuencia_Viernes,
                      frecuencia_sabado = @Frecuencia_Sabado,
                      frecuencia_domingo = @Frecuencia_Domingo
                  WHERE cod_paquete = @Cod_Paquete",
                new
                {
                    request.Cod_Paquete,
                    request.Descripcion,
                    request.Notas,
                    request.User_Modifica,
                    Fecha_Modifica = DateTime.Now,
                    Fecha_Inicio = request.Fecha_Inicio.ToLocalTime(),
                    Fecha_Corte = request.Fecha_Corte.ToLocalTime(),
                    Frecuencia_Horai = request.Frecuencia_Horai.ToLocalTime(),
                    Frecuencia_Horac = request.Frecuencia_Horac.ToLocalTime(),
                    request.Frecuencia_Lunes,
                    request.Frecuencia_Martes,
                    request.Frecuencia_Miercoles,
                    request.Frecuencia_Jueves,
                    request.Frecuencia_Viernes,
                    request.Frecuencia_Sabado,
                    request.Frecuencia_Domingo
                });

            return CrearRespuestaNonQuery(result, "Ok", "Error al actualizar el paquete.");
        }

        /// <summary>
        /// Inserta un paquete usando procedimiento almacenado y devuelve el nuevo identificador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del paquete.</param>
        /// <returns>Resultado de la operación con el nuevo identificador en Code.</returns>
        public ErrorDto Paquete_Insertar(int CodEmpresa, PaqueteDto request)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parameters = CrearParametrosInsertarPaquete(request);
                connection.Execute("[spINV_W_Paquete_Agregar]", parameters, commandType: CommandType.StoredProcedure);
                return parameters.Get<int>("NewID");
            });

            return result.Code == 0
                ? new ErrorDto { Code = result.Result, Description = "Ok" }
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el paquete.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Mantiene compatibilidad con el método alternativo de inserción de paquetes.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del paquete.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Paquete_Insertar2(int CodEmpresa, PaqueteDto request)
        {
            return Paquete_Insertar(CodEmpresa, request);
        }

        /// <summary>
        /// Inserta un detalle de paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del detalle.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PaqueteDetalle_Insertar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_PaqueteDetalle_Agregar]",
                new
                {
                    request.Cod_Producto,
                    request.Cod_Paquete,
                    request.Cantidad,
                    request.Porc_Utilidad,
                    request.Precio,
                    request.Imp_Ventas,
                    request.Imp_Consumo
                },
                "Error al insertar el detalle del paquete.");
        }

        /// <summary>
        /// Actualiza un detalle de paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del detalle.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PaqueteDetalle_Actualizar(int CodEmpresa, PaqueteDetalleDto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_PaqueteDetalle_Actualizar]",
                new
                {
                    request.Cod_Producto,
                    request.Cod_Paquete,
                    request.Cantidad,
                    request.Porc_Utilidad,
                    request.Precio,
                    request.Imp_Ventas,
                    request.Imp_Consumo
                },
                "Error al actualizar el detalle del paquete.");
        }

        /// <summary>
        /// Elimina un detalle de paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del detalle.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto PaqueteDetalle_Eliminar(int CodEmpresa, PaqueteDetalleDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_paquetes_detalle WHERE linea = @Linea",
                CrearParametrosLinea(request.Linea));

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar el detalle del paquete.");
        }

        /// <summary>
        /// Mantiene compatibilidad con la eliminación lógica del paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del paquete.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Paquete_Eliminar(int CodEmpresa, PaqueteDto request)
        {
            return Paquete_Actualizar(CodEmpresa, request);
        }

        /// <summary>
        /// Elimina todos los detalles de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del paquete.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Paquete_EliminarDetalles(int CodEmpresa, PaqueteDto request)
        {
            if (!request.Cod_Paquete.HasValue)
            {
                return DbHelper.ErrorResponse("Código de paquete inválido.", -1);
            }

            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_paquetes_detalle WHERE cod_paquete = @Cod_Paquete",
                CrearParametrosPaquete(request.Cod_Paquete.Value));

            return CrearRespuestaNonQuery(result, "Ok", "Error al eliminar los detalles del paquete.");
        }

        #endregion
    }
}