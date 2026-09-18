using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvOrdNivelAutoDB
    {
        private const int CodigoValidacion = -2;
        private const string TipoEntrada = "E";
        private const string TipoSalida = "S";
        private const string TipoTraslado = "T";
        private const string ProcedimientoAutorizadorEliminar =
            "[spINV_W_Autorizador_Eliminar]";
        private const string ProcedimientoUsuarioCargoActualizar =
            "[spINV_W_UsuarioACargo_Actualizar]";

        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa el acceso a datos del formulario de niveles de autorización.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvOrdNivelAutoDB(IConfiguration config)
        {
            _config = config ??
                throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtiene la lista paginada de usuarios disponibles para autorización.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa.</param>
        /// <param name="pagina">Registro inicial de la página.</param>
        /// <param name="paginacion">Cantidad de registros solicitados.</param>
        /// <param name="filtro">Filtro por usuario o descripción.</param>
        /// <returns>Lista paginada de usuarios y autorizadores.</returns>
        public ErrorDto<AutorizadorDataLista> Autorizadores_Obtener(
            int CodCliente,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            var result = DbHelper.WithConn(
                CrearPortalDb(),
                CodCliente,
                connection =>
                {
                    var parametros = new DynamicParameters();
                    var clausulaFiltro = CrearClausulaFiltro(
                        filtro,
                        parametros);

                    var total = connection.QueryFirstOrDefault<int>(
                        $"""
                        SELECT COUNT(*)
                        FROM usuarios U
                        LEFT JOIN pv_orden_autorizadores A
                            ON U.nombre = A.usuario
                        WHERE U.estado = 'A'
                        {clausulaFiltro}
                        """,
                        parametros);

                    var consulta = new StringBuilder(
                        $"""
                        SELECT
                            U.nombre AS usuario,
                            U.descripcion AS descripcion,
                            A.fecha AS fecha
                        FROM usuarios U
                        LEFT JOIN pv_orden_autorizadores A
                            ON U.nombre = A.usuario
                        WHERE U.estado = 'A'
                        {clausulaFiltro}
                        ORDER BY A.fecha DESC, U.nombre
                        """);

                    AgregarPaginacion(
                        consulta,
                        parametros,
                        pagina,
                        paginacion);

                    return new AutorizadorDataLista
                    {
                        total = total,
                        autorizadores = connection
                            .Query<AutorizadorDto>(
                                consulta.ToString(),
                                parametros)
                            .ToList()
                    };
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    result.Result ?? CrearAutorizadorListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ??
                    "Error al obtener los autorizadores.",
                    result.Code.GetValueOrDefault(-1),
                    CrearAutorizadorListaVacia());
        }

        /// <summary>
        /// Obtiene todos los usuarios activos e indica cuáles son autorizadores.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Lista de usuarios activos.</returns>
        public ErrorDto<List<AutorizadorDto>> Autorizador_ObtenerTodos(
            int CodEmpresa)
        {
            const string query = """
                SELECT
                    U.nombre AS usuario,
                    U.descripcion AS descripcion,
                    A.fecha AS fecha
                FROM usuarios U
                LEFT JOIN pv_orden_autorizadores A
                    ON U.nombre = A.usuario
                WHERE U.estado = 'A'
                ORDER BY A.fecha DESC, U.nombre
                """;

            return DbHelper.ExecuteListQuery<AutorizadorDto>(
                CrearPortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Obtiene los usuarios registrados como autorizadores.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Lista de autorizadores activos.</returns>
        public ErrorDto<List<AutorizadorDto>> Autorizador_Obtener(
            int CodEmpresa)
        {
            const string query = """
                SELECT
                    U.nombre AS usuario,
                    U.descripcion AS descripcion,
                    A.fecha AS fecha
                FROM usuarios U
                INNER JOIN pv_orden_autorizadores A
                    ON U.nombre = A.usuario
                WHERE U.estado = 'A'
                ORDER BY U.nombre
                """;

            return DbHelper.ExecuteListQuery<AutorizadorDto>(
                CrearPortalDb(),
                CodEmpresa,
                query);
        }

        /// <summary>
        /// Registra un usuario como autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del autorizador.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizador_Insertar(
            int CodEmpresa,
            AutorizadorDto request)
        {
            if (request is null ||
                string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario autorizador es requerido.",
                    CodigoValidacion);
            }

            const string query = """
                INSERT INTO pv_orden_autorizadores
                (
                    usuario,
                    fecha,
                    estado
                )
                VALUES
                (
                    @usuario,
                    Getdate(),
                    'A'
                )
                """;

            var result = DbHelper.ExecuteNonQuery(
                CrearPortalDb(),
                CodEmpresa,
                query,
                new
                {
                    usuario = request.usuario.Trim()
                });

            return CrearRespuestaOperacion(
                result,
                "Error al registrar el autorizador.");
        }

        /// <summary>
        /// Elimina un autorizador y sus usuarios asociados.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario autorizador.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizador_Eliminar(
            int CodEmpresa,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario autorizador es requerido.",
                    CodigoValidacion);
            }

            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                ProcedimientoAutorizadorEliminar,
                new
                {
                    Usuario = usuario.Trim()
                },
                "Error al eliminar el autorizador.");
        }

        /// <summary>
        /// Obtiene la lista paginada de usuarios asignables a un autorizador.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa.</param>
        /// <param name="usuario">Usuario autorizador.</param>
        /// <param name="pagina">Registro inicial de la página.</param>
        /// <param name="paginacion">Cantidad de registros solicitados.</param>
        /// <param name="filtro">Filtro por usuario o descripción.</param>
        /// <returns>Lista paginada de usuarios a cargo.</returns>
        public ErrorDto<UsuariosACargoDataLista>
            UsuariosACargoAut_Obtener(
                int CodCliente,
                string usuario,
                int? pagina,
                int? paginacion,
                string? filtro)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario autorizador es requerido.",
                    CodigoValidacion,
                    CrearUsuariosCargoListaVacia());
            }

            var result = DbHelper.WithConn(
                CrearPortalDb(),
                CodCliente,
                connection =>
                {
                    var parametros = new DynamicParameters();
                    parametros.Add(
                        "usuario",
                        usuario.Trim(),
                        DbType.String);

                    var clausulaFiltro = CrearClausulaFiltro(
                        filtro,
                        parametros);

                    var total = connection.QueryFirstOrDefault<int>(
                        $"""
                        SELECT COUNT(*)
                        FROM usuarios U
                        LEFT JOIN pv_orden_autousers C
                            ON U.nombre = C.usuario_asignado
                            AND C.usuario = @usuario
                        WHERE U.estado = 'A'
                        {clausulaFiltro}
                        """,
                        parametros);

                    var consulta = new StringBuilder(
                        $"""
                        SELECT
                            U.nombre AS usuario,
                            U.descripcion AS descripcion,
                            C.usuario AS autorizador,
                            ISNULL(C.entradas, 0) AS entradas,
                            ISNULL(C.salidas, 0) AS salidas,
                            ISNULL(C.requisiciones, 0) AS requisiciones,
                            ISNULL(C.traslados, 0) AS traslados
                        FROM usuarios U
                        LEFT JOIN pv_orden_autousers C
                            ON U.nombre = C.usuario_asignado
                            AND C.usuario = @usuario
                        WHERE U.estado = 'A'
                        {clausulaFiltro}
                        ORDER BY
                            C.fecha_asignacion DESC,
                            U.nombre
                        """);

                    AgregarPaginacion(
                        consulta,
                        parametros,
                        pagina,
                        paginacion);

                    return new UsuariosACargoDataLista
                    {
                        total = total,
                        usuarios = connection
                            .Query<UsuarioaCargoDto>(
                                consulta.ToString(),
                                parametros)
                            .ToList()
                    };
                });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(
                    result.Result ??
                    CrearUsuariosCargoListaVacia())
                : DbHelper.CreateErrorResponse(
                    result.Description ??
                    "Error al obtener los usuarios a cargo.",
                    result.Code.GetValueOrDefault(-1),
                    CrearUsuariosCargoListaVacia());
        }

        /// <summary>
        /// Obtiene todos los usuarios asignables a un autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario autorizador.</param>
        /// <returns>Lista de usuarios a cargo.</returns>
        public List<UsuarioaCargoDto> UsuariosACargo_Obtener(
            int CodEmpresa,
            string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new List<UsuarioaCargoDto>();
            }

            const string query = """
                SELECT
                    U.nombre AS usuario,
                    U.descripcion AS descripcion,
                    C.usuario AS autorizador,
                    ISNULL(C.entradas, 0) AS entradas,
                    ISNULL(C.salidas, 0) AS salidas,
                    ISNULL(C.requisiciones, 0) AS requisiciones,
                    ISNULL(C.traslados, 0) AS traslados
                FROM usuarios U
                LEFT JOIN pv_orden_autousers C
                    ON U.nombre = C.usuario_asignado
                    AND C.usuario = @usuario
                WHERE U.estado = 'A'
                ORDER BY
                    C.fecha_asignacion DESC,
                    U.nombre
                """;

            var result = DbHelper.WithConn(
                CrearPortalDb(),
                CodEmpresa,
                connection => connection
                    .Query<UsuarioaCargoDto>(
                        query,
                        new
                        {
                            usuario = usuario.Trim()
                        })
                    .ToList());

            return result.Code == 0
                ? result.Result ?? new List<UsuarioaCargoDto>()
                : new List<UsuarioaCargoDto>();
        }

        /// <summary>
        /// Actualiza los permisos de un usuario asignado a un autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Permisos del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UsuarioACargo_Actualizar(
            int CodEmpresa,
            UsuarioaCargoDto request)
        {
            var validacion =
                ValidarUsuarioCargo(request);

            if (validacion is not null)
            {
                return validacion;
            }

            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                ProcedimientoUsuarioCargoActualizar,
                new
                {
                    Entradas = request.entradas,
                    Salidas = request.salidas,
                    Requisiciones = request.requisiciones,
                    Traslados = request.traslados,
                    Autorizador = request.autorizador.Trim(),
                    Usuario = request.usuario.Trim()
                },
                "Error al actualizar el usuario a cargo.");
        }

        /// <summary>
        /// Obtiene la lista paginada de usuarios que pueden cambiar fechas.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa.</param>
        /// <param name="tipo">Tipo de movimiento.</param>
        /// <param name="pagina">Registro inicial de la página.</param>
        /// <param name="paginacion">Cantidad de registros solicitados.</param>
        /// <param name="filtro">Filtro por usuario o descripción.</param>
        /// <returns>Lista paginada de usuarios.</returns>
        public UsuariosCambioFchDataLista UsuariosCambioFch_Obtener(
            int CodCliente,
            string tipo,
            int? pagina,
            int? paginacion,
            string? filtro)
        {
            if (!EsTipoValido(tipo))
            {
                return CrearUsuariosCambioFechaListaVacia();
            }

            var result = DbHelper.WithConn(
                CrearPortalDb(),
                CodCliente,
                connection =>
                {
                    var parametros = new DynamicParameters();
                    parametros.Add(
                        "tipo",
                        tipo.Trim(),
                        DbType.String);

                    var clausulaFiltro = CrearClausulaFiltro(
                        filtro,
                        parametros);

                    var total = connection.QueryFirstOrDefault<int>(
                        $"""
                        SELECT COUNT(*)
                        FROM usuarios U
                        LEFT JOIN PV_INVUSRFECHAS A
                            ON U.nombre = A.usuario
                            AND A.tipo = @tipo
                        WHERE U.estado = 'A'
                        {clausulaFiltro}
                        """,
                        parametros);

                    var consulta = new StringBuilder(
                        $"""
                        SELECT
                            U.nombre AS usuario,
                            U.descripcion AS descripcion,
                            A.tipo AS tipo
                        FROM usuarios U
                        LEFT JOIN PV_INVUSRFECHAS A
                            ON U.nombre = A.usuario
                            AND A.tipo = @tipo
                        WHERE U.estado = 'A'
                        {clausulaFiltro}
                        ORDER BY
                            A.tipo DESC,
                            U.nombre
                        """);

                    AgregarPaginacion(
                        consulta,
                        parametros,
                        pagina,
                        paginacion);

                    return new UsuariosCambioFchDataLista
                    {
                        total = total,
                        usuarios = connection
                            .Query<UsuarioaCambioFechaDto>(
                                consulta.ToString(),
                                parametros)
                            .ToList()
                    };
                });

            return result.Code == 0
                ? result.Result ??
                  CrearUsuariosCambioFechaListaVacia()
                : CrearUsuariosCambioFechaListaVacia();
        }

        /// <summary>
        /// Obtiene todos los usuarios que pueden cambiar fechas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="tipo">Tipo de movimiento.</param>
        /// <returns>Lista de usuarios y permisos.</returns>
        public List<UsuarioaCambioFechaDto>
            UsuariosCambioFecha_Obtener(
                int CodEmpresa,
                string tipo)
        {
            if (!EsTipoValido(tipo))
            {
                return new List<UsuarioaCambioFechaDto>();
            }

            const string query = """
                SELECT
                    U.nombre AS usuario,
                    U.descripcion AS descripcion,
                    A.tipo AS tipo
                FROM usuarios U
                LEFT JOIN PV_INVUSRFECHAS A
                    ON U.nombre = A.usuario
                    AND A.tipo = @tipo
                WHERE U.estado = 'A'
                ORDER BY
                    A.tipo DESC,
                    U.nombre
                """;

            var result = DbHelper.WithConn(
                CrearPortalDb(),
                CodEmpresa,
                connection => connection
                    .Query<UsuarioaCambioFechaDto>(
                        query,
                        new
                        {
                            tipo = tipo.Trim()
                        })
                    .ToList());

            return result.Code == 0
                ? result.Result ??
                  new List<UsuarioaCambioFechaDto>()
                : new List<UsuarioaCambioFechaDto>();
        }

        /// <summary>
        /// Registra un permiso para cambiar fechas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Usuario y tipo de movimiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CambioFechas_Insertar(
            int CodEmpresa,
            UsuarioaCambioFechaDto request)
        {
            var validacion =
                ValidarCambioFecha(request);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                INSERT INTO PV_INVUSRFECHAS
                (
                    usuario,
                    tipo
                )
                VALUES
                (
                    @usuario,
                    @tipo
                )
                """;

            var result = DbHelper.ExecuteNonQuery(
                CrearPortalDb(),
                CodEmpresa,
                query,
                new
                {
                    usuario = request.usuario.Trim(),
                    tipo = request.tipo.Trim()
                });

            return CrearRespuestaOperacion(
                result,
                "Error al registrar el permiso de cambio de fecha.");
        }

        /// <summary>
        /// Elimina un permiso para cambiar fechas.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Usuario y tipo de movimiento.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CambioFechas_Eliminar(
            int CodEmpresa,
            UsuarioaCambioFechaDto request)
        {
            var validacion =
                ValidarCambioFecha(request);

            if (validacion is not null)
            {
                return validacion;
            }

            const string query = """
                DELETE FROM PV_INVUSRFECHAS
                WHERE usuario = @usuario
                  AND tipo = @tipo
                """;

            var result = DbHelper.ExecuteNonQuery(
                CrearPortalDb(),
                CodEmpresa,
                query,
                new
                {
                    usuario = request.usuario.Trim(),
                    tipo = request.tipo.Trim()
                });

            return CrearRespuestaOperacion(
                result,
                "Error al eliminar el permiso de cambio de fecha.");
        }

        /// <summary>
        /// Ejecuta un procedimiento que devuelve un código de resultado.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="procedimiento">Procedimiento almacenado.</param>
        /// <param name="parametros">Parámetros del procedimiento.</param>
        /// <param name="mensajeError">Mensaje predeterminado de error.</param>
        /// <returns>Resultado estándar de la operación.</returns>
        private ErrorDto EjecutarProcedimientoConCodigo(
            int CodEmpresa,
            string procedimiento,
            object parametros,
            string mensajeError)
        {
            var result = DbHelper.WithConn<int>(
                CrearPortalDb(),
                CodEmpresa,
                connection =>
                    connection.QueryFirstOrDefault<int>(
                        procedimiento,
                        parametros,
                        commandType:
                            CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? mensajeError,
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    mensajeError,
                    result.Result);
        }

        /// <summary>
        /// Agrega la paginación parametrizada a una consulta.
        /// </summary>
        /// <param name="consulta">Consulta que se modificará.</param>
        /// <param name="parametros">Parámetros de la consulta.</param>
        /// <param name="pagina">Registro inicial de la página.</param>
        /// <param name="paginacion">Cantidad de registros solicitados.</param>
        private static void AgregarPaginacion(
            StringBuilder consulta,
            DynamicParameters parametros,
            int? pagina,
            int? paginacion)
        {
            if (!pagina.HasValue ||
                !paginacion.HasValue ||
                pagina.Value < 0 ||
                paginacion.Value <= 0)
            {
                return;
            }

            consulta.Append(
                " OFFSET @pagina ROWS FETCH NEXT @paginacion ROWS ONLY");

            parametros.Add(
                "pagina",
                pagina.Value,
                DbType.Int32);

            parametros.Add(
                "paginacion",
                paginacion.Value,
                DbType.Int32);
        }

        /// <summary>
        /// Crea el filtro parametrizado por usuario o descripción.
        /// </summary>
        /// <param name="filtro">Texto que se utilizará como filtro.</param>
        /// <param name="parametros">Parámetros de la consulta.</param>
        /// <returns>Cláusula SQL fija para aplicar el filtro.</returns>
        private static string CrearClausulaFiltro(
            string? filtro,
            DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return string.Empty;
            }

            parametros.Add(
                "filtro",
                $"%{filtro.Trim()}%",
                DbType.String);

            return """
                 AND
                 (
                     U.nombre LIKE @filtro
                     OR U.descripcion LIKE @filtro
                 )
                """;
        }

        /// <summary>
        /// Valida los datos utilizados para actualizar un usuario a cargo.
        /// </summary>
        /// <param name="request">Datos que se validarán.</param>
        /// <returns>Error de validación o null cuando los datos son válidos.</returns>
        private static ErrorDto? ValidarUsuarioCargo(
            UsuarioaCargoDto? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del usuario son requeridos.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.autorizador))
            {
                return DbHelper.ErrorResponse(
                    "El usuario autorizador es requerido.",
                    CodigoValidacion);
            }

            return string.IsNullOrWhiteSpace(request.usuario)
                ? DbHelper.ErrorResponse(
                    "El usuario asignado es requerido.",
                    CodigoValidacion)
                : null;
        }

        /// <summary>
        /// Valida los datos utilizados para modificar permisos de fechas.
        /// </summary>
        /// <param name="request">Datos que se validarán.</param>
        /// <returns>Error de validación o null cuando los datos son válidos.</returns>
        private static ErrorDto? ValidarCambioFecha(
            UsuarioaCambioFechaDto? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del permiso son requeridos.",
                    CodigoValidacion);
            }

            if (string.IsNullOrWhiteSpace(request.usuario))
            {
                return DbHelper.ErrorResponse(
                    "El usuario es requerido.",
                    CodigoValidacion);
            }

            return EsTipoValido(request.tipo)
                ? null
                : DbHelper.ErrorResponse(
                    "El tipo de movimiento no es v&aacute;lido.",
                    CodigoValidacion);
        }

        /// <summary>
        /// Determina si el tipo de movimiento recibido es válido.
        /// </summary>
        /// <param name="tipo">Tipo de movimiento.</param>
        /// <returns>True cuando corresponde a entrada, salida o traslado.</returns>
        private static bool EsTipoValido(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return false;
            }

            return tipo.Trim() is
                TipoEntrada or
                TipoSalida or
                TipoTraslado;
        }

        /// <summary>
        /// Convierte el resultado de DbHelper en una respuesta estándar.
        /// </summary>
        /// <param name="result">Resultado de la ejecución.</param>
        /// <param name="mensajeError">Mensaje predeterminado de error.</param>
        /// <returns>Respuesta estándar de la operación.</returns>
        private static ErrorDto CrearRespuestaOperacion(
            ErrorDto result,
            string mensajeError)
        {
            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(
                    result.Description ?? mensajeError,
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Crea el acceso a las conexiones de las empresas.
        /// </summary>
        /// <returns>Instancia configurada de PortalDB.</returns>
        private PortalDB CrearPortalDb()
        {
            return new PortalDB(_config);
        }

        /// <summary>
        /// Crea una respuesta vacía para autorizadores.
        /// </summary>
        /// <returns>Respuesta inicializada.</returns>
        private static AutorizadorDataLista
            CrearAutorizadorListaVacia()
        {
            return new AutorizadorDataLista
            {
                total = 0,
                autorizadores = new List<AutorizadorDto>()
            };
        }

        /// <summary>
        /// Crea una respuesta vacía para usuarios a cargo.
        /// </summary>
        /// <returns>Respuesta inicializada.</returns>
        private static UsuariosACargoDataLista
            CrearUsuariosCargoListaVacia()
        {
            return new UsuariosACargoDataLista
            {
                total = 0,
                usuarios = new List<UsuarioaCargoDto>()
            };
        }

        /// <summary>
        /// Crea una respuesta vacía para permisos de cambio de fecha.
        /// </summary>
        /// <returns>Respuesta inicializada.</returns>
        private static UsuariosCambioFchDataLista
            CrearUsuariosCambioFechaListaVacia()
        {
            return new UsuariosCambioFchDataLista
            {
                total = 0,
                usuarios =
                    new List<UsuarioaCambioFechaDto>()
            };
        }
    }
}