using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmInvOrdNivelAutoDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvOrdNivelAutoDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvOrdNivelAutoDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea una instancia de <see cref="PortalDB"/> usando la configuración actual.
        /// </summary>
        /// <returns>Instancia de acceso a configuración de base de datos.</returns>
        private PortalDB CreatePortalDb() => new(_config);

        /// <summary>
        /// Crea una respuesta vacía para el listado de autorizadores.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static AutorizadorDataLista CrearAutorizadorListaVacia() => new()
        {
            Total = 0,
            Autorizadores = new List<AutorizadorDto>()
        };

        /// <summary>
        /// Crea una respuesta vacía para el listado de usuarios a cargo.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static UsuariosACargoDataLista CrearUsuariosACargoListaVacia() => new()
        {
            Total = 0,
            Usuarios = new List<UsuarioaCargoDto>()
        };

        /// <summary>
        /// Crea una respuesta vacía para el listado de usuarios con permiso de cambio de fecha.
        /// </summary>
        /// <returns>Listado vacío inicializado.</returns>
        private static UsuariosCambioFchDataLista CrearUsuariosCambioFchListaVacia() => new()
        {
            Total = 0,
            Usuarios = new List<UsuarioaCambioFechaDto>()
        };


        /// <summary>
        /// Agrega un filtro de usuario y descripción a la consulta.
        /// </summary>
        /// <param name="filtro">Texto de filtro.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltroUsuarios(string? filtro, System.Text.StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (string.IsNullOrWhiteSpace(filtro))
            {
                return;
            }

            queryBuilder.Append(" AND (U.nombre LIKE @Filtro OR U.DESCRIPCION LIKE @Filtro) ");
            parametros.Add("Filtro", $"%{filtro.Trim()}%");
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a una consulta.
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

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
            parametros.Add("Offset", pagina.Value);
            parametros.Add("Fetch", paginacion.Value);
        }

        /// <summary>
        /// Crea parámetros para usuario.
        /// </summary>
        /// <param name="usuario">Usuario.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosUsuario(string usuario) => new
        {
            Usuario = usuario,
            usuario
        };

        /// <summary>
        /// Crea parámetros para tipo.
        /// </summary>
        /// <param name="tipo">Tipo a filtrar.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosTipo(string tipo) => new
        {
            Tipo = tipo,
            tipo
        };

        /// <summary>
        /// Crea parámetros para usuario y tipo.
        /// </summary>
        /// <param name="usuario">Usuario.</param>
        /// <param name="tipo">Tipo.</param>
        /// <returns>Objeto de parámetros para Dapper.</returns>
        private static object CrearParametrosUsuarioTipo(string usuario, string tipo) => new
        {
            Usuario = usuario,
            usuario,
            Tipo = tipo,
            tipo
        };

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

        #region Autorizaciones

        /// <summary>
        /// Obtiene la lista paginada de autorizadores.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por usuario o descripción.</param>
        /// <returns>Listado de autorizadores.</returns>
        public ErrorDto<AutorizadorDataLista> Autorizadores_Obtener(int CodCliente, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearAutorizadorListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    "SELECT count(*) FROM usuarios U LEFT JOIN pv_orden_autorizadores A ON U.nombre = A.usuario WHERE U.Estado = 'A'");

                var parametros = new DynamicParameters();
                var queryBuilder = new System.Text.StringBuilder(@"SELECT U.nombre as Usuario,
                                                                        U.descripcion,
                                                                        A.fecha
                                                                 FROM usuarios U
                                                                 LEFT JOIN pv_orden_autorizadores A ON U.nombre = A.usuario
                                                                 WHERE U.Estado = 'A'");

                AgregarFiltroUsuarios(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY A.fecha ASC ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Autorizadores = connection.Query<AutorizadorDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearAutorizadorListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener autorizadores.", result.Code.GetValueOrDefault(-1), CrearAutorizadorListaVacia());
        }

        /// <summary>
        /// Lista todos los usuarios, incluyendo autorizadores de la empresa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de usuarios y autorizadores.</returns>
        public ErrorDto<List<AutorizadorDto>> Autorizador_ObtenerTodos(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AutorizadorDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT U.nombre as Usuario,
                         U.descripcion,
                         A.fecha
                  FROM usuarios U
                  LEFT JOIN pv_orden_autorizadores A ON U.nombre = A.usuario
                  WHERE U.Estado = 'A'
                  ORDER BY A.fecha DESC");
        }

        /// <summary>
        /// Lista los autorizadores de la empresa.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de autorizadores.</returns>
        public ErrorDto<List<AutorizadorDto>> Autorizador_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<AutorizadorDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"SELECT U.nombre as Usuario,
                         U.descripcion
                  FROM usuarios U
                  INNER JOIN pv_orden_autorizadores A ON U.nombre = A.usuario
                  WHERE U.Estado = 'A'
                  ORDER BY U.nombre");
        }

        /// <summary>
        /// Inserta un nuevo autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del autorizador.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizador_Insertar(int CodEmpresa, AutorizadorDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "INSERT pv_orden_autorizadores(USUARIO, FECHA, ESTADO) VALUES (@usuario, @fecha, @estado)",
                new
                {
                    usuario = request.Usuario,
                    fecha = DateTime.Now,
                    estado = "A"
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el autorizador.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario autorizador.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto Autorizador_Eliminar(int CodEmpresa, string usuario)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_Autorizador_Eliminar]",
                new { Usuario = usuario },
                "Error al eliminar el autorizador.");
        }

        #endregion

        #region Usuarios a Cargo

        /// <summary>
        /// Obtiene la lista paginada de usuarios a cargo del autorizador.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="usuario">Usuario autorizador.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por usuario o descripción.</param>
        /// <returns>Listado de usuarios a cargo.</returns>
        public ErrorDto<UsuariosACargoDataLista> UsuariosACargoAut_Obtener(int CodCliente, string usuario, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearUsuariosACargoListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    @"SELECT count(*)
                      FROM usuarios U
                      LEFT JOIN pv_orden_autousers C ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
                      WHERE U.Estado = 'A'",
                    CrearParametrosUsuario(usuario));

                var parametros = new DynamicParameters();
                parametros.Add("Usuario", usuario);
                var queryBuilder = new System.Text.StringBuilder(@"SELECT U.nombre as Usuario,
                                                                        U.descripcion,
                                                                        C.Usuario AS Autorizador,
                                                                        isnull(C.Entradas,0) AS Entradas,
                                                                        isnull(C.Salidas,0) AS Salidas,
                                                                        isnull(C.requisiciones,0) AS Requisiciones,
                                                                        isnull(C.Traslados,0) AS Traslados
                                                                 FROM usuarios U
                                                                 LEFT JOIN pv_orden_autousers C ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
                                                                 WHERE U.Estado = 'A'");

                AgregarFiltroUsuarios(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY C.fecha_asignacion DESC ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Usuarios = connection.Query<UsuarioaCargoDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? CrearUsuariosACargoListaVacia())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener usuarios a cargo.", result.Code.GetValueOrDefault(-1), CrearUsuariosACargoListaVacia());
        }

        /// <summary>
        /// Obtiene la lista de usuarios a cargo del autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario autorizador.</param>
        /// <returns>Listado de usuarios a cargo.</returns>
        public List<UsuarioaCargoDto> UsuariosACargo_Obtener(int CodEmpresa, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<UsuarioaCargoDto>(
                    @"SELECT U.nombre as Usuario,
                             U.descripcion,
                             C.Usuario AS Autorizador,
                             isnull(C.Entradas,0) AS Entradas,
                             isnull(C.Salidas,0) AS Salidas,
                             isnull(C.requisiciones,0) AS Requisiciones,
                             isnull(C.Traslados,0) AS Traslados
                      FROM usuarios U
                      LEFT JOIN pv_orden_autousers C ON U.nombre = C.usuario_asignado AND C.usuario = @Usuario
                      WHERE U.Estado = 'A'
                      ORDER BY C.fecha_asignacion DESC",
                    CrearParametrosUsuario(usuario)).ToList());

            return result.Code == 0 ? result.Result ?? new List<UsuarioaCargoDto>() : new List<UsuarioaCargoDto>();
        }

        /// <summary>
        /// Actualiza los usuarios a cargo del autorizador.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del usuario a cargo.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto UsuarioACargo_Actualizar(int CodEmpresa, UsuarioaCargoDto request)
        {
            return EjecutarProcedimientoConCodigo(
                CodEmpresa,
                "[spINV_W_UsuarioACargo_Actualizar]",
                new
                {
                    Entradas = request.Entradas,
                    Salidas = request.Salidas,
                    Requisiciones = request.Requisiciones,
                    Traslados = request.Traslados,
                    Autorizador = request.Autorizador,
                    Usuario = request.Usuario
                },
                "Error al actualizar el usuario a cargo.");
        }

        #endregion

        #region Cambio Fecha

        /// <summary>
        /// Obtiene la lista paginada de usuarios que pueden cambiar fecha.
        /// </summary>
        /// <param name="CodCliente">Código de la empresa cliente.</param>
        /// <param name="tipo">Tipo de permiso de cambio de fecha.</param>
        /// <param name="pagina">Fila inicial para paginación.</param>
        /// <param name="paginacion">Cantidad de filas a retornar.</param>
        /// <param name="filtro">Filtro por usuario o descripción.</param>
        /// <returns>Listado de usuarios con permiso de cambio de fecha.</returns>
        public UsuariosCambioFchDataLista UsuariosCambioFch_Obtener(int CodCliente, string tipo, int? pagina, int? paginacion, string? filtro)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodCliente, connection =>
            {
                var respuesta = CrearUsuariosCambioFchListaVacia();
                respuesta.Total = connection.QueryFirstOrDefault<int>(
                    @"SELECT count(*)
                      FROM usuarios U
                      LEFT JOIN PV_INVUSRFECHAS A ON U.nombre = A.usuario AND A.tipo = @Tipo
                      WHERE U.EStado = 'A'",
                    CrearParametrosTipo(tipo));

                var parametros = new DynamicParameters();
                parametros.Add("Tipo", tipo);
                var queryBuilder = new System.Text.StringBuilder(@"SELECT U.nombre as Usuario,
                                                                        U.descripcion,
                                                                        A.tipo
                                                                 FROM usuarios U
                                                                 LEFT JOIN PV_INVUSRFECHAS A ON U.nombre = A.usuario AND A.tipo = @Tipo
                                                                 WHERE U.EStado = 'A'");

                AgregarFiltroUsuarios(filtro, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY A.tipo ASC ");
                AgregarPaginacion(pagina, paginacion, queryBuilder, parametros);

                respuesta.Usuarios = connection.Query<UsuarioaCambioFechaDto>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0 ? result.Result ?? CrearUsuariosCambioFchListaVacia() : CrearUsuariosCambioFchListaVacia();
        }

        /// <summary>
        /// Obtiene la lista de usuarios que pueden cambiar fecha.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="tipo">Tipo de permiso de cambio de fecha.</param>
        /// <returns>Listado de usuarios con permiso de cambio de fecha.</returns>
        public List<UsuarioaCambioFechaDto> UsuariosCambioFecha_Obtener(int CodEmpresa, string tipo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<UsuarioaCambioFechaDto>(
                    @"SELECT U.nombre as Usuario,
                             U.descripcion,
                             A.tipo
                      FROM usuarios U
                      LEFT JOIN PV_INVUSRFECHAS A ON U.nombre = A.usuario AND A.tipo = @Tipo
                      WHERE U.EStado = 'A'
                      ORDER BY A.tipo DESC",
                    CrearParametrosTipo(tipo)).ToList());

            return result.Code == 0 ? result.Result ?? new List<UsuarioaCambioFechaDto>() : new List<UsuarioaCambioFechaDto>();
        }

        /// <summary>
        /// Inserta un nuevo usuario que puede cambiar fecha.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CambioFechas_Insertar(int CodEmpresa, UsuarioaCambioFechaDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "INSERT pv_invusrfechas(USUARIO, TIPO) VALUES (@usuario, @tipo)",
                new
                {
                    usuario = request.Usuario,
                    tipo = request.Tipo
                });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar el permiso de cambio de fecha.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un usuario que puede cambiar fecha.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto CambioFechas_Eliminar(int CodEmpresa, UsuarioaCambioFechaDto request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "DELETE pv_invusrfechas WHERE USUARIO = @usuario AND TIPO = @tipo",
                CrearParametrosUsuarioTipo(request.Usuario, request.Tipo));

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar el permiso de cambio de fecha.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}
