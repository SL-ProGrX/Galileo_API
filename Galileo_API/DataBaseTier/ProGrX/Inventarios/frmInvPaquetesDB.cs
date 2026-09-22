using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public sealed class FrmInvPaquetesDB
    {
        private const string DateFormatYmd = "yyyy-MM-dd";
        private const string TimeFormat = "HH:mm:ss";
        private const string MensajeOk = "Ok";

        private const string ErrorObtenerPaquetes =
            "Ocurri&oacute; un error al consultar los paquetes.";

        private const string ErrorObtenerPaquete =
            "Ocurri&oacute; un error al consultar el paquete.";

        private const string ErrorActualizarPaquete =
            "Ocurri&oacute; un error al actualizar el paquete.";

        private const string ErrorInsertarPaquete =
            "Ocurri&oacute; un error al registrar el paquete.";

        private const string ErrorInsertarDetalle =
            "Ocurri&oacute; un error al registrar el detalle del paquete.";

        private const string ErrorActualizarDetalle =
            "Ocurri&oacute; un error al actualizar el detalle del paquete.";

        private const string ErrorEliminarDetalle =
            "Ocurri&oacute; un error al eliminar el detalle del paquete.";

        private const string ErrorSolicitudRequerida =
            "La informaci&oacute;n de la solicitud es requerida.";

        private const string ErrorCodigoPaquete =
            "El c&oacute;digo del paquete es requerido.";

        private const string ErrorDescripcionPaquete =
            "La descripci&oacute;n del paquete es requerida.";

        private const string ErrorFechaInicio =
            "La fecha inicial es requerida.";

        private const string ErrorFechaCorte =
            "La fecha de corte es requerida.";

        private const string ErrorRangoFechas =
            "La fecha inicial no puede ser mayor que la fecha de corte.";

        private const string ErrorCodigoProducto =
            "El c&oacute;digo del producto es requerido.";

        private const string ErrorCantidad =
            "La cantidad debe ser mayor que cero.";

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos de paquetes.
        /// </summary>
        /// <param name="config">Configuración del API.</param>
        public FrmInvPaquetesDB(IConfiguration config)
        {
            _config = config ??
                throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene los paquetes de inventario de forma paginada.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="filtros">Filtros y paginación de la consulta.</param>
        /// <returns>Lista paginada de paquetes.</returns>
        public ErrorDto<PaqueteDataLista>
            INV_Paquetes_Lista_Obtener(
                int CodEmpresa,
                PaquetesFiltrosDto filtros)
        {
            if (filtros is null)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de consulta son requeridos.",
                    -2,
                    new PaqueteDataLista());
            }

            var resultado = DbHelper.WithConn(
                CrearPortalDb(),
                CodEmpresa,
                connection =>
                {
                    const string queryTotal =
                        """
                SELECT COUNT(*)
                FROM pv_paquetes
                WHERE (
                    @Filtro = ''
                    OR CONVERT(
                        VARCHAR(20),
                        cod_paquete
                    ) LIKE @FiltroBusqueda
                    OR descripcion LIKE @FiltroBusqueda
                )
                """;

                    const string querySinPaginacion =
                        """
                SELECT
                    cod_paquete,
                    descripcion,
                    fecha_crea,
                    user_crea,
                    user_modifica,
                    fecha_inicio,
                    notas,
                    fecha_modifica,
                    fecha_corte,
                    frecuencia_horai,
                    frecuencia_horac,
                    frecuencia_lunes,
                    frecuencia_martes,
                    frecuencia_miercoles,
                    frecuencia_jueves,
                    frecuencia_viernes,
                    frecuencia_sabado,
                    frecuencia_domingo
                FROM pv_paquetes
                WHERE (
                    @Filtro = ''
                    OR CONVERT(
                        VARCHAR(20),
                        cod_paquete
                    ) LIKE @FiltroBusqueda
                    OR descripcion LIKE @FiltroBusqueda
                )
                ORDER BY cod_paquete
                """;

                    const string queryPaginado =
                        """
                SELECT
                    cod_paquete,
                    descripcion,
                    fecha_crea,
                    user_crea,
                    user_modifica,
                    fecha_inicio,
                    notas,
                    fecha_modifica,
                    fecha_corte,
                    frecuencia_horai,
                    frecuencia_horac,
                    frecuencia_lunes,
                    frecuencia_martes,
                    frecuencia_miercoles,
                    frecuencia_jueves,
                    frecuencia_viernes,
                    frecuencia_sabado,
                    frecuencia_domingo
                FROM pv_paquetes
                WHERE (
                    @Filtro = ''
                    OR CONVERT(
                        VARCHAR(20),
                        cod_paquete
                    ) LIKE @FiltroBusqueda
                    OR descripcion LIKE @FiltroBusqueda
                )
                ORDER BY cod_paquete
                OFFSET @Offset ROWS
                FETCH NEXT @Fetch ROWS ONLY
                """;

                    string filtroNormalizado =
                        filtros.filtro?.Trim() ??
                        string.Empty;

                    int offset = Math.Max(
                        filtros.pagina,
                        0);

                    int fetch = Math.Max(
                        filtros.paginacion,
                        0);

                    var parametros = new
                    {
                        Filtro = filtroNormalizado,
                        FiltroBusqueda =
                            $"%{filtroNormalizado}%",
                        Offset = offset,
                        Fetch = fetch
                    };

                    var respuesta = new PaqueteDataLista
                    {
                        total =
                            connection.QueryFirstOrDefault<int>(
                                queryTotal,
                                parametros)
                    };

                    respuesta.lista = fetch > 0
                        ? connection.Query<PaqueteDto>(
                            queryPaginado,
                            parametros).ToList()
                        : connection.Query<PaqueteDto>(
                            querySinPaginacion,
                            parametros).ToList();

                    return respuesta;
                });

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(
                    resultado.Result ??
                    new PaqueteDataLista())
                : DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    ErrorObtenerPaquetes,
                    resultado.Code.GetValueOrDefault(-1),
                    new PaqueteDataLista());
        }

        /// <summary>
        /// Obtiene todos los paquetes disponibles.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Lista completa de paquetes.</returns>
        public ErrorDto<List<PaqueteDto>>
            INV_Paquetes_Todos_Obtener(
                int CodEmpresa)
        {
            const string query =
                """
                SELECT
                    cod_paquete,
                    descripcion,
                    fecha_crea,
                    user_crea,
                    user_modifica,
                    fecha_inicio,
                    notas,
                    fecha_modifica,
                    fecha_corte,
                    frecuencia_horai,
                    frecuencia_horac,
                    frecuencia_lunes,
                    frecuencia_martes,
                    frecuencia_miercoles,
                    frecuencia_jueves,
                    frecuencia_viernes,
                    frecuencia_sabado,
                    frecuencia_domingo
                FROM pv_paquetes
                ORDER BY cod_paquete
                """;

            return DbHelper.ExecuteListQuery<PaqueteDto>(
                CrearPortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene el encabezado de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codPaquete">Código del paquete.</param>
        /// <returns>Encabezado del paquete.</returns>
        public ErrorDto<PaqueteDto>
            INV_Paquetes_Encabezado_Obtener(
                int CodEmpresa,
                int codPaquete)
        {
            if (codPaquete <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    ErrorCodigoPaquete,
                    -2,
                    default(PaqueteDto));
            }

            const string query =
                """
                SELECT
                    cod_paquete,
                    descripcion,
                    fecha_crea,
                    user_crea,
                    user_modifica,
                    fecha_inicio,
                    notas,
                    fecha_modifica,
                    fecha_corte,
                    frecuencia_horai,
                    frecuencia_horac,
                    frecuencia_lunes,
                    frecuencia_martes,
                    frecuencia_miercoles,
                    frecuencia_jueves,
                    frecuencia_viernes,
                    frecuencia_sabado,
                    frecuencia_domingo
                FROM pv_paquetes
                WHERE cod_paquete = @Cod_Paquete
                """;

            var resultado =
                DbHelper.ExecuteSingleQuery<PaqueteDto>(
                    CrearPortalDb(),
                    CodEmpresa,
                    query,
                    null,
                    CrearParametrosPaquete(codPaquete));

            return resultado.Code == 0
                ? DbHelper.CreateOkResponse(resultado.Result)
                : DbHelper.CreateErrorResponse(
                    resultado.Description ??
                    ErrorObtenerPaquete,
                    resultado.Code.GetValueOrDefault(-1),
                    default(PaqueteDto));
        }

        /// <summary>
        /// Obtiene el detalle de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="codPaquete">Código del paquete.</param>
        /// <returns>Productos asociados al paquete.</returns>
        public ErrorDto<List<PaqueteDetalleDto>>
            INV_Paquetes_Detalle_Obtener(
                int CodEmpresa,
                int codPaquete)
        {
            if (codPaquete <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    ErrorCodigoPaquete,
                    -2,
                    new List<PaqueteDetalleDto>());
            }

            const string query =
                """
                SELECT
                    D.linea,
                    D.cod_producto,
                    P.descripcion,
                    D.cod_paquete,
                    D.cantidad,
                    D.porc_utilidad,
                    D.precio,
                    D.imp_ventas,
                    ISNULL(D.imp_consumo, 0) AS imp_consumo,
                    (
                        D.cantidad *
                        (
                            D.precio +
                            D.precio *
                            D.porc_utilidad / 100
                        )
                    ) +
                    (
                        (
                            D.cantidad *
                            (
                                D.precio +
                                D.precio *
                                D.porc_utilidad / 100
                            )
                        ) *
                        (D.imp_ventas / 100)
                    ) AS total,
                    ISNULL(P.cod_unidad, '') AS unidad
                FROM pv_paquetes_detalle D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                WHERE D.cod_paquete = @Cod_Paquete
                ORDER BY D.linea
                """;

            return DbHelper.ExecuteListQuery<PaqueteDetalleDto>(
                CrearPortalDb(),
                CodEmpresa,
                query,
                CrearParametrosPaquete(codPaquete));
        }

        /// <summary>
        /// Registra un paquete de inventario.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Información del paquete.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto INV_Paquetes_Registrar(
            int CodEmpresa,
            PaqueteDto request)
        {
            ErrorDto? validacion =
                INV_Paquetes_Encabezado_Validar(
                    request,
                    false);

            if (validacion is not null)
            {
                return validacion;
            }

            var resultado = DbHelper.WithConn<int>(
                CrearPortalDb(),
                CodEmpresa,
                connection =>
                {
                    DynamicParameters parametros =
                        INV_Paquetes_Registro_Parametros_Crear(
                            request);

                    connection.Execute(
                        "[spINV_W_Paquete_Agregar]",
                        parametros,
                        commandType:
                            CommandType.StoredProcedure);

                    return parametros.Get<int>("NewID");
                });

            return resultado.Code == 0
                ? new ErrorDto
                {
                    Code = resultado.Result,
                    Description = MensajeOk
                }
                : DbHelper.ErrorResponse(
                    resultado.Description ??
                    ErrorInsertarPaquete,
                    resultado.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el encabezado de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Información actualizada del paquete.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto INV_Paquetes_Actualizar(
            int CodEmpresa,
            PaqueteDto request)
        {
            ErrorDto? validacion =
                INV_Paquetes_Encabezado_Validar(
                    request,
                    true);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query =
                """
                UPDATE pv_paquetes
                SET descripcion = @Descripcion,
                    notas = @Notas,
                    user_modifica = @User_Modifica,
                    fecha_modifica = Getdate(),
                    fecha_inicio = @Fecha_Inicio,
                    fecha_corte = @Fecha_Corte,
                    frecuencia_horai = @Frecuencia_Horai,
                    frecuencia_horac = @Frecuencia_Horac,
                    frecuencia_lunes = @Frecuencia_Lunes,
                    frecuencia_martes = @Frecuencia_Martes,
                    frecuencia_miercoles =
                        @Frecuencia_Miercoles,
                    frecuencia_jueves = @Frecuencia_Jueves,
                    frecuencia_viernes = @Frecuencia_Viernes,
                    frecuencia_sabado = @Frecuencia_Sabado,
                    frecuencia_domingo = @Frecuencia_Domingo
                WHERE cod_paquete = @Cod_Paquete
                """;

            var parametros = new
            {
                Cod_Paquete = request.cod_paquete,
                Descripcion =
                    request.descripcion.Trim().ToUpperInvariant(),
                Notas = request.notas.Trim(),
                User_Modifica =
                    request.user_modifica.Trim(),
                request.fecha_inicio,
                request.fecha_corte,
                request.frecuencia_horai,
                request.frecuencia_horac,
                request.frecuencia_lunes,
                request.frecuencia_martes,
                request.frecuencia_miercoles,
                request.frecuencia_jueves,
                request.frecuencia_viernes,
                request.frecuencia_sabado,
                request.frecuencia_domingo
            };

            var resultado = DbHelper.ExecuteNonQuery(
                CrearPortalDb(),
                CodEmpresa,
                query,
                parametros);

            return resultado.Code == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(
                    resultado.Description ??
                    ErrorActualizarPaquete,
                    resultado.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra una línea del detalle de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Información de la línea.</param>
        /// <returns>Resultado del registro.</returns>
        public ErrorDto INV_Paquetes_Detalle_Registrar(
            int CodEmpresa,
            PaqueteDetalleDto request)
        {
            ErrorDto? validacion =
                INV_Paquetes_Detalle_Validar(request);

            if (validacion is not null)
            {
                return validacion;
            }

            var resultado = DbHelper.WithConn<int>(
                CrearPortalDb(),
                CodEmpresa,
                connection =>
                    connection.QueryFirstOrDefault<int>(
                        "[spINV_W_PaqueteDetalle_Agregar]",
                        CrearParametrosDetalle(request),
                        commandType:
                            CommandType.StoredProcedure));

            return INV_Paquetes_Procedimiento_Resultado_Crear(
                resultado,
                ErrorInsertarDetalle);
        }

        /// <summary>
        /// Actualiza una línea del detalle de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Información actualizada de la línea.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto INV_Paquetes_Detalle_Actualizar(
            int CodEmpresa,
            PaqueteDetalleDto request)
        {
            ErrorDto? validacion =
                INV_Paquetes_Detalle_Validar(request);

            if (validacion is not null)
            {
                return validacion;
            }

            var resultado = DbHelper.WithConn<int>(
                CrearPortalDb(),
                CodEmpresa,
                connection =>
                    connection.QueryFirstOrDefault<int>(
                        "[spINV_W_PaqueteDetalle_Actualizar]",
                        CrearParametrosDetalle(request),
                        commandType:
                            CommandType.StoredProcedure));

            return INV_Paquetes_Procedimiento_Resultado_Crear(
                resultado,
                ErrorActualizarDetalle);
        }

        /// <summary>
        /// Elimina una línea del detalle de un paquete.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Línea que se eliminará.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto INV_Paquetes_Detalle_Eliminar(
            int CodEmpresa,
            PaqueteDetalleDto request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    ErrorSolicitudRequerida,
                    -2);
            }

            if (request.cod_paquete <= 0)
            {
                return DbHelper.ErrorResponse(
                    ErrorCodigoPaquete,
                    -2);
            }

            if (request.linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La l&iacute;nea del detalle es requerida.",
                    -2);
            }

            const string query =
                """
                DELETE FROM pv_paquetes_detalle
                WHERE cod_paquete = @Cod_Paquete
                  AND linea = @Linea
                """;

            var resultado = DbHelper.ExecuteNonQuery(
                CrearPortalDb(),
                CodEmpresa,
                query,
                CrearParametrosEliminarDetalle(request));

            return resultado.Code == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(
                    resultado.Description ??
                    ErrorEliminarDetalle,
                    resultado.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea la conexión de la empresa.
        /// </summary>
        /// <returns>Acceso a la conexión empresarial.</returns>
        private PortalDB CrearPortalDb()
        {
            return new PortalDB(_config);
        }

        /// <summary>
        /// Crea los parámetros de consulta de un paquete.
        /// </summary>
        /// <param name="codPaquete">Código del paquete.</param>
        /// <returns>Parámetros de la consulta.</returns>
        private static object CrearParametrosPaquete(
            int codPaquete)
        {
            return new
            {
                Cod_Paquete = codPaquete
            };
        }

        /// <summary>
        /// Crea los parámetros de persistencia del detalle.
        /// </summary>
        /// <param name="request">Información del detalle.</param>
        /// <returns>Parámetros del procedimiento.</returns>
        private static object CrearParametrosDetalle(
            PaqueteDetalleDto request)
        {
            return new
            {
                Cod_Producto = request.cod_producto.Trim(),
                Cod_Paquete = request.cod_paquete,
                request.cantidad,
                request.porc_utilidad,
                request.precio,
                request.imp_ventas,
                Imp_Consumo = 0
            };
        }

        /// <summary>
        /// Crea los parámetros para eliminar una línea.
        /// </summary>
        /// <param name="request">Línea que se eliminará.</param>
        /// <returns>Parámetros de eliminación.</returns>
        private static object CrearParametrosEliminarDetalle(
            PaqueteDetalleDto request)
        {
            return new
            {
                Cod_Paquete = request.cod_paquete,
                Linea = request.linea
            };
        }

        /// <summary>
        /// Crea los parámetros para registrar un paquete.
        /// </summary>
        /// <param name="request">Información del paquete.</param>
        /// <returns>Parámetros del procedimiento.</returns>
        private static DynamicParameters
            INV_Paquetes_Registro_Parametros_Crear(
                PaqueteDto request)
        {
            var parametros = new DynamicParameters();

            parametros.Add(
                "Descripcion",
                request.descripcion.Trim().ToUpperInvariant());

            parametros.Add(
                "Notas",
                request.notas.Trim());

            parametros.Add(
                "User_Crea",
                request.user_crea.Trim());

            parametros.Add(
                "Fecha_Inicio",
                MProGrXAuxiliarDB.validaFechaGlobal(
                    request.fecha_inicio,
                    DateFormatYmd));

            parametros.Add(
                "Fecha_Corte",
                MProGrXAuxiliarDB.validaFechaGlobal(
                    request.fecha_corte,
                    DateFormatYmd));

            parametros.Add(
                "Frecuencia_Horai",
                MProGrXAuxiliarDB.validaFechaGlobal(
                    request.frecuencia_horai,
                    TimeFormat));

            parametros.Add(
                "Frecuencia_Horac",
                MProGrXAuxiliarDB.validaFechaGlobal(
                    request.frecuencia_horac,
                    TimeFormat));

            parametros.Add(
                "Frecuencia_Lunes",
                request.frecuencia_lunes);

            parametros.Add(
                "Frecuencia_Martes",
                request.frecuencia_martes);

            parametros.Add(
                "Frecuencia_Miercoles",
                request.frecuencia_miercoles);

            parametros.Add(
                "Frecuencia_Jueves",
                request.frecuencia_jueves);

            parametros.Add(
                "Frecuencia_Viernes",
                request.frecuencia_viernes);

            parametros.Add(
                "Frecuencia_Sabado",
                request.frecuencia_sabado);

            parametros.Add(
                "Frecuencia_Domingo",
                request.frecuencia_domingo);

            parametros.Add(
                "NewID",
                dbType: DbType.Int32,
                direction: ParameterDirection.Output);

            return parametros;
        }

        /// <summary>
        /// Valida el encabezado de un paquete.
        /// </summary>
        /// <param name="request">Información del paquete.</param>
        /// <param name="requiereCodigo">Indica si requiere código.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto?
            INV_Paquetes_Encabezado_Validar(
                PaqueteDto? request,
                bool requiereCodigo)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    ErrorSolicitudRequerida,
                    -2);
            }

            if (
                requiereCodigo &&
                request.cod_paquete <= 0)
            {
                return DbHelper.ErrorResponse(
                    ErrorCodigoPaquete,
                    -2);
            }

            if (
                string.IsNullOrWhiteSpace(
                    request.descripcion))
            {
                return DbHelper.ErrorResponse(
                    ErrorDescripcionPaquete,
                    -2);
            }

            if (request.fecha_inicio is null)
            {
                return DbHelper.ErrorResponse(
                    ErrorFechaInicio,
                    -2);
            }

            if (request.fecha_corte is null)
            {
                return DbHelper.ErrorResponse(
                    ErrorFechaCorte,
                    -2);
            }

            if (
                request.fecha_inicio.Value.Date >
                request.fecha_corte.Value.Date)
            {
                return DbHelper.ErrorResponse(
                    ErrorRangoFechas,
                    -2);
            }

            return null;
        }

        /// <summary>
        /// Valida una línea del detalle del paquete.
        /// </summary>
        /// <param name="request">Información del detalle.</param>
        /// <returns>Error de validación o null.</returns>
        private static ErrorDto?
            INV_Paquetes_Detalle_Validar(
                PaqueteDetalleDto? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    ErrorSolicitudRequerida,
                    -2);
            }

            if (request.cod_paquete <= 0)
            {
                return DbHelper.ErrorResponse(
                    ErrorCodigoPaquete,
                    -2);
            }

            if (
                string.IsNullOrWhiteSpace(
                    request.cod_producto))
            {
                return DbHelper.ErrorResponse(
                    ErrorCodigoProducto,
                    -2);
            }

            if (request.cantidad <= 0)
            {
                return DbHelper.ErrorResponse(
                    ErrorCantidad,
                    -2);
            }

            return null;
        }

        /// <summary>
        /// Convierte el resultado de un procedimiento en ErrorDto.
        /// </summary>
        /// <param name="resultado">Resultado de DbHelper.</param>
        /// <param name="mensajeError">Mensaje de error del proceso.</param>
        /// <returns>Resultado normalizado.</returns>
        private static ErrorDto
            INV_Paquetes_Procedimiento_Resultado_Crear(
                ErrorDto<int> resultado,
                string mensajeError)
        {
            if (resultado.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    resultado.Description ??
                    mensajeError,
                    resultado.Code.GetValueOrDefault(-1));
            }

            return resultado.Result == 0
                ? DbHelper.OkResponse(MensajeOk)
                : DbHelper.ErrorResponse(
                    mensajeError,
                    resultado.Result);
        }
    }
}