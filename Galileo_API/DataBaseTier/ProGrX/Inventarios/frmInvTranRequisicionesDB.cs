using Dapper;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using System.Data;
using System.Text;

namespace Galileo.DataBaseTier
{
    public class FrmInvTranRequisicionesDB
    {
        private readonly IConfiguration _config;

        #region Constructor y helpers

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="FrmInvTranRequisicionesDB"/>.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTranRequisicionesDB(IConfiguration config)
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
        /// Traduce el estado interno de la requisición a una descripción legible.
        /// </summary>
        /// <param name="estado">Estado interno.</param>
        /// <returns>Descripción del estado.</returns>
        private static string ObtenerDescripcionEstado(string? estado)
        {
            return estado switch
            {
                "S" => "Solicitada",
                "P" => "Procesada",
                "A" => "Autorizada",
                "R" => "Rechazada",
                "N" => "Procesada - Pendiente",
                _ => estado ?? string.Empty
            };
        }

        /// <summary>
        /// Normaliza el estado de una requisición.
        /// </summary>
        /// <param name="data">Requisición a normalizar.</param>
        private static void NormalizarEstadoRequisicion(TranRequisicionData? data)
        {
            if (data is null)
            {
                return;
            }

            data.Estado = ObtenerDescripcionEstado(data.Estado);
        }

        /// <summary>
        /// Obtiene el siguiente consecutivo de requisición disponible.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <returns>Consecutivo formateado.</returns>
        private static string ObtenerSiguienteRequisicion(IDbConnection connection)
        {
            string consecutivo = connection.QueryFirstOrDefault<string>(
                "select isnull(max(cod_requisicion),0)+1 as Ultimo from pv_requisiciones") ?? "1";

            return consecutivo.PadLeft(10, '0');
        }

        /// <summary>
        /// Elimina el detalle de una requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        private static void EliminarDetalleRequisicion(IDbConnection connection, int codRequisicion)
        {
            connection.Execute(
                "delete pv_requi_detalle where cod_requisicion = @CodRequisicion",
                new { CodRequisicion = codRequisicion });
        }

        /// <summary>
        /// Obtiene los identificadores de control asociados a una requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        /// <returns>Listado de identificadores de control.</returns>
        private static List<int> ObtenerIdsControlRequisicion(IDbConnection connection, int codRequisicion)
        {
            return connection.Query<int>(
                "select id_control from pv_control_activos where cod_requisicion = @CodRequisicion",
                new { CodRequisicion = codRequisicion }).ToList();
        }

        /// <summary>
        /// Limpia la asociación de una lista de activos de control con la requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="idsControl">Identificadores de control.</param>
        private static void LimpiarControlActivos(IDbConnection connection, IEnumerable<int> idsControl)
        {
            foreach (int item in idsControl)
            {
                connection.Execute(
                    @"update pv_control_activos
                      set cod_requisicion = null,
                          entrega_usuario = null,
                          id_responsable = null
                      where id_control = @IdControl",
                    new { IdControl = item });
            }
        }

        /// <summary>
        /// Inserta una línea de detalle de requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        /// <param name="linea">Número de línea.</param>
        /// <param name="item">Detalle a insertar.</param>
        private static void InsertarDetalleRequisicion(IDbConnection connection, int codRequisicion, int linea, InvReqProduc item)
        {
            decimal cantidadSolicitada = (decimal)(item.solicitado + item.Despacho);

            connection.Execute(
                @"insert pv_requi_detalle(linea, cod_requisicion, cod_producto, cantidad, despacho, cod_bodega, costo, solicitado)
                  values(@Linea, @CodRequisicion, @Cod_Producto, @Cantidad, 0, @Cod_Bodega, @Costo, @Solicitado)",
                new
                {
                    Linea = linea,
                    CodRequisicion = codRequisicion,
                    item.Cod_Producto,
                    item.Cantidad,
                    item.Cod_Bodega,
                    item.Costo,
                    Solicitado = cantidadSolicitada
                });
        }

        /// <summary>
        /// Obtiene el encabezado de una requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        /// <returns>Encabezado encontrado.</returns>
        private static TranRequisicionData? ObtenerEncabezadoRequisicion(IDbConnection connection, int codRequisicion)
        {
            return connection.QueryFirstOrDefault<TranRequisicionData>(
                "select * from pv_requisiciones where cod_requisicion = @CodRequisicion",
                new { CodRequisicion = codRequisicion });
        }

        /// <summary>
        /// Actualiza el control de activos asociado a una línea de requisición.
        /// </summary>
        /// <param name="connection">Conexión activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        /// <param name="encabezado">Encabezado de la requisición.</param>
        /// <param name="item">Línea de detalle.</param>
        private static void ActualizarControlActivo(IDbConnection connection, int codRequisicion, TranRequisicionData? encabezado, InvReqProduc item)
        {
            connection.Execute(
                @"update pv_control_activos set 
                        cod_requisicion = @CodRequisicion,
                        entrega_usuario = @EntregaUsuario,
                        id_responsable = @IdResponsable
                  where id_control = @IdControl",
                new
                {
                    CodRequisicion = codRequisicion,
                    EntregaUsuario = encabezado?.recibe_user ?? string.Empty,
                    IdResponsable = encabezado?.responsable_activo ?? string.Empty,
                    item.id_control
                });
        }

        /// <summary>
        /// Agrega filtros al listado de plantillas de requisición.
        /// </summary>
        /// <param name="codRequisicion">Código de requisición.</param>
        /// <param name="generaUser">Usuario generador.</param>
        /// <param name="generaFecha">Fecha generada.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltrosPlantilla(int? codRequisicion, string? generaUser, string? generaFecha, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            queryBuilder.Append(" where plantilla = 1 ");

            if (codRequisicion.HasValue && codRequisicion.Value != 0)
            {
                queryBuilder.Append(" and cod_requisicion = @CodRequisicion ");
                parametros.Add("CodRequisicion", codRequisicion.Value);
            }

            if (!string.IsNullOrWhiteSpace(generaUser))
            {
                queryBuilder.Append(" and genera_user like @GeneraUser ");
                parametros.Add("GeneraUser", $"%{generaUser.Trim()}%");
            }

            if (!string.IsNullOrWhiteSpace(generaFecha))
            {
                queryBuilder.Append(" and genera_fecha between @FechaInicio and @FechaFin ");
                parametros.Add("FechaInicio", $"{generaFecha.Trim()} 00:00:00");
                parametros.Add("FechaFin", $"{generaFecha.Trim()} 23:59:59");
            }
        }

        /// <summary>
        /// Construye la consulta de listado de requisiciones según filtros de usuario, columna y estado.
        /// </summary>
        /// <param name="usuario">Usuario.</param>
        /// <param name="columna">Columna a filtrar.</param>
        /// <param name="estado">Estado.</param>
        /// <returns>Consulta SQL parametrizada.</returns>
        private static string BuildInvTranRequisicionesListaQuery(string usuario, string columna, string estado)
        {
            if (usuario == "T" && columna == "N" && estado == "T")
            {
                return "SELECT * FROM pv_requisiciones ORDER BY COD_REQUISICION ASC";
            }

            var conditions = new List<string>();

            if (!string.IsNullOrEmpty(estado) && estado != "T")
            {
                conditions.Add("estado = @estado");
            }

            var userCondition = GetUsuarioFiltroCondition(usuario, columna);
            if (!string.IsNullOrEmpty(userCondition))
            {
                conditions.Add(userCondition);
            }

            var query = "SELECT * FROM pv_requisiciones";
            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }

            return query;
        }

        /// <summary>
        /// Obtiene la condición SQL del filtro por usuario.
        /// </summary>
        /// <param name="usuario">Usuario.</param>
        /// <param name="columna">Columna.</param>
        /// <returns>Condición SQL.</returns>
        private static string GetUsuarioFiltroCondition(string usuario, string columna)
        {
            if (string.IsNullOrEmpty(usuario) || usuario == "T")
            {
                return string.Empty;
            }

            return columna switch
            {
                "G" => "genera_user = @usuario",
                "A" => "autoriza_user = @usuario",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Agrega filtros al listado de activos para requisiciones.
        /// </summary>
        /// <param name="filtros">Filtros recibidos.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarFiltrosActivos(InvReqFiltros filtros, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            queryBuilder.Append(@" WHERE P.COD_PRODCLAS NOT IN (118)");

            if (!string.IsNullOrWhiteSpace(filtros.filtro))
            {
                queryBuilder.Append(@" AND (
                                        P.DESCRIPCION LIKE @Filtro
                                        OR A.COD_PRODUCTO LIKE @Filtro
                                        OR P.CABYS LIKE @Filtro
                                        OR P.COD_BARRAS LIKE @Filtro
                                      )");
                parametros.Add("Filtro", $"%{filtros.filtro.Trim()}%");
            }
        }

        /// <summary>
        /// Agrega paginación OFFSET/FETCH a la consulta de activos.
        /// </summary>
        /// <param name="filtros">Filtros recibidos.</param>
        /// <param name="queryBuilder">Consulta a modificar.</param>
        /// <param name="parametros">Parámetros Dapper.</param>
        private static void AgregarPaginacionActivos(InvReqFiltros filtros, StringBuilder queryBuilder, DynamicParameters parametros)
        {
            if (filtros.pagina is null || filtros.paginacion is null)
            {
                return;
            }

            queryBuilder.Append(" OFFSET @Offset ROWS FETCH NEXT @Fetch ROWS ONLY ");
            parametros.Add("Offset", filtros.pagina.Value);
            parametros.Add("Fetch", filtros.paginacion.Value);
        }

        private const string QueryActivosRequisicion = @"SELECT 
                            A.ID_CONTROL,
                            P.COD_PRODUCTO,
                            P.DESCRIPCION,
                            CASE WHEN A.COD_PRODUCTO IS NOT NULL THEN 1 ELSE P.EXISTENCIA END AS CANTIDAD,
                            CASE WHEN A.COSTO_UNITARIO IS NOT NULL THEN A.COSTO_UNITARIO ELSE P.COSTO_REGULAR END AS COSTO,
                            A.COSTO_UNITARIO AS COSTO,
                            A.COD_BODEGA,
                            P.CABYS,
                            P.COD_BARRAS,
                            A.NUMERO_PLACA
                        FROM 
                            PV_PRODUCTOS P
                        LEFT JOIN 
                            PV_CONTROL_ACTIVOS A ON A.COD_PRODUCTO = P.COD_PRODUCTO 
                            AND A.ENTREGA_USUARIO = '' 
                            AND A.REGISTRO_USUARIO = @Usuario";

        #endregion

        #region Consultas

        /// <summary>
        /// Obtiene una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <returns>Datos de la requisición.</returns>
        public ErrorDto<TranRequisicionData> InvTranRequisicion_Obtener(int CodEmpresa, int CodRequisicion)
        {
            var result = DbHelper.ExecuteSingleQuery<TranRequisicionData>(
                CreatePortalDb(),
                CodEmpresa,
                @"select X.*,
                         (rtrim(C.cod_entsal) + ' - ' + C.descripcion) as Causa 
                  from pv_requisiciones X 
                  inner join pv_entrada_salida C on X.cod_entsal = C.cod_entsal
                  where X.cod_requisicion = @CodRequisicion",
                new TranRequisicionData(),
                new { CodRequisicion });

            if (result.Result is not null)
            {
                NormalizarEstadoRequisicion(result.Result);
            }

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TranRequisicionData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener la requisición.", result.Code.GetValueOrDefault(-1), new TranRequisicionData());
        }

        /// <summary>
        /// Obtiene los productos de una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <returns>Listado de productos de la requisición.</returns>
        public ErrorDto<List<InvReqProduc>> InvRequesicionProduc_Obtener(int CodEmpresa, int CodRequisicion)
        {
            var result = DbHelper.ExecuteListQuery<InvReqProduc>(
                CreatePortalDb(),
                CodEmpresa,
                @"select D.linea,
                         D.Cod_Producto,
                         P.Descripcion,
                         D.Cantidad,
                         D.Costo,
                         (D.cantidad * D.Costo) as Total,
                         isnull(D.despacho,0) as Despacho,
                         D.Cod_Bodega,
                         B.descripcion as Bodega,
                         D.solicitado
                  from pv_requi_detalle D 
                  inner join pv_productos P on D.cod_producto = P.cod_producto
                  inner join PV_Bodegas B on D.cod_bodega = B.cod_bodega
                  where D.cod_requisicion = @CodRequisicion
                  order by D.Linea",
                new { CodRequisicion });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<InvReqProduc>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el detalle de la requisición.", result.Code.GetValueOrDefault(-1), new List<InvReqProduc>());
        }

        /// <summary>
        /// Obtiene la requisición siguiente o anterior.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="scrollValue">Dirección del desplazamiento.</param>
        /// <param name="CodRequisicion">Código actual de la requisición.</param>
        /// <returns>Requisición encontrada por desplazamiento.</returns>
        public ErrorDto<TranRequisicionData> InvTranRequisicion_scroll(int CodEmpresa, int scrollValue, int? CodRequisicion)
        {
            string query = scrollValue == 1
                ? "select Top 1 cod_requisicion from pv_requisiciones where cod_requisicion > @CodRequisicion order by cod_requisicion asc"
                : "select Top 1 cod_requisicion from pv_requisiciones where cod_requisicion < @CodRequisicion order by cod_requisicion desc";

            var result = DbHelper.ExecuteSingleQuery<TranRequisicionData>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new TranRequisicionData(),
                new { CodRequisicion = CodRequisicion ?? 0 });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new TranRequisicionData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al desplazar la requisición.", result.Code.GetValueOrDefault(-1), new TranRequisicionData());
        }

        /// <summary>
        /// Obtiene plantillas de requisiciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="GeneraUser">Usuario generador.</param>
        /// <param name="GeneraFecha">Fecha de generación.</param>
        /// <returns>Listado de plantillas de requisición.</returns>
        public ErrorDto<List<TranRequisicionData>> InvTranPlantilla_Obtener(int CodEmpresa, int? CodRequisicion, string? GeneraUser, string? GeneraFecha)
        {
            var parametros = new DynamicParameters();
            var queryBuilder = new StringBuilder("select cod_requisicion, genera_user, genera_fecha, documento, notas from pv_requisiciones");
            AgregarFiltrosPlantilla(CodRequisicion, GeneraUser, GeneraFecha, queryBuilder, parametros);

            var result = DbHelper.ExecuteListQuery<TranRequisicionData>(
                CreatePortalDb(),
                CodEmpresa,
                queryBuilder.ToString(),
                parametros);

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TranRequisicionData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener plantillas de requisición.", result.Code.GetValueOrDefault(-1), new List<TranRequisicionData>());
        }

        /// <summary>
        /// Obtiene la lista de transacciones de requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario filtro.</param>
        /// <param name="columna">Columna filtro.</param>
        /// <param name="estado">Estado filtro.</param>
        /// <returns>Listado de requisiciones.</returns>
        public ErrorDto<List<TranRequisicionData>> InvTranRequisiciones_Lista(int CodEmpresa, string usuario, string columna, string estado)
        {
            var query = BuildInvTranRequisicionesListaQuery(usuario, columna, estado);
            var result = DbHelper.ExecuteListQuery<TranRequisicionData>(
                CreatePortalDb(),
                CodEmpresa,
                query,
                new { usuario, estado });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<TranRequisicionData>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener el listado de requisiciones.", result.Code.GetValueOrDefault(-1), new List<TranRequisicionData>());
        }

        /// <summary>
        /// Obtiene las UENS.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de UENS.</returns>
        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<CatalogosLista>(
                CreatePortalDb(),
                CodEmpresa,
                "select COD_UNIDAD as item, DESCRIPCION FROM CORE_UENS");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<CatalogosLista>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener UENS.", result.Code.GetValueOrDefault(-1), new List<CatalogosLista>());
        }

        /// <summary>
        /// Obtiene la lista de usuarios que reciben por UEN.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="cod_unidad">Código de la unidad.</param>
        /// <returns>Listado de usuarios receptores.</returns>
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuarioRecibeLista_Obtener(int CodEmpresa, string cod_unidad)
        {
            var response = DbHelper.WithConn<List<InvRequsUsuarioRecibe>>(CreatePortalDb(), CodEmpresa, connection =>
            {
                var usuarios = connection.Query<InvRequsUsuarioRecibe>(
                    @"SELECT R.CORE_USUARIO AS usuario,
                             U.DESCRIPCION AS nombre,
                             '' AS identificacion
                      FROM CORE_UENS_USUARIOS_ROLES R
                      LEFT JOIN USUARIOS U ON U.NOMBRE = R.CORE_USUARIO
                      WHERE R.COD_UNIDAD = @cod_unidad",
                    new { cod_unidad }).ToList();

                string? portalConnString = _config.GetConnectionString("DefaultConnString");
                using var portalConn = new global::Microsoft.Data.SqlClient.SqlConnection(portalConnString);

                foreach (var item in usuarios)
                {
                    InvRequsUsuarioRecibe? usuarioPortal = portalConn.QueryFirstOrDefault<InvRequsUsuarioRecibe>(
                        @"SELECT [USUARIO] AS usuario,
                                 [NOMBRE] AS nombre,
                                 [IDENTIFICACION] AS identificacion
                          FROM [PGX_Portal].[dbo].[US_USUARIOS]
                          WHERE USUARIO = @usuario",
                        new { item.usuario });

                    if (usuarioPortal is not null)
                    {
                        item.identificacion = usuarioPortal.identificacion;
                        item.nombre = usuarioPortal.nombre;
                    }
                }

                return usuarios;
            });

            return response.Code == 0
                ? DbHelper.CreateOkResponse(response.Result ?? new List<InvRequsUsuarioRecibe>())
                : DbHelper.CreateErrorResponse(response.Description ?? "Error al obtener usuarios receptores.", response.Code.GetValueOrDefault(-1), new List<InvRequsUsuarioRecibe>());
        }

        /// <summary>
        /// Obtiene la lista de usuarios responsables de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de usuarios responsables.</returns>
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuariosActivoLista_Obtener(int CodEmpresa)
        {
            var response = DbHelper.WithConn<List<InvRequsUsuarioRecibe>>(CreatePortalDb(), CodEmpresa, connection =>
            {
                string? portalConnString = _config.GetConnectionString("DefaultConnString");
                using var portalConn = new global::Microsoft.Data.SqlClient.SqlConnection(portalConnString);

                var usuarios = portalConn.Query<InvRequsUsuarioRecibe>(
                    @"SELECT [USUARIO] AS usuario,
                             [NOMBRE] AS nombre,
                             [IDENTIFICACION] AS identificacion
                      FROM [PGX_Portal].[dbo].[US_USUARIOS]").ToList();

                var filtrados = new List<InvRequsUsuarioRecibe>();
                foreach (var item in usuarios)
                {
                    string? identificacion = connection.QueryFirstOrDefault<string>(
                        @"SELECT DISTINCT IDENTIFICACION
                          FROM ACTIVOS_RESPONSABLES
                          WHERE IDENTIFICACION = @identificacion",
                        new { item.identificacion });

                    if (!string.IsNullOrWhiteSpace(identificacion))
                    {
                        filtrados.Add(item);
                    }
                }

                return filtrados;
            });

            return response.Code == 0
                ? DbHelper.CreateOkResponse(response.Result ?? new List<InvRequsUsuarioRecibe>())
                : DbHelper.CreateErrorResponse(response.Description ?? "Error al obtener usuarios responsables de activos.", response.Code.GetValueOrDefault(-1), new List<InvRequsUsuarioRecibe>());
        }

        /// <summary>
        /// Obtiene los productos activos para requisiciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="invReqFiltros">Filtros serializados en JSON.</param>
        /// <returns>Listado de activos para requisiciones.</returns>
        public ErrorDto<InvRequesicionesActivosLista> ProductosRequesicionesActivo_Obtener(int CodEmpresa, string invReqFiltros)
        {
            InvReqFiltros filtros = JsonConvert.DeserializeObject<InvReqFiltros>(invReqFiltros) ?? new InvReqFiltros();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var respuesta = new InvRequesicionesActivosLista
                {
                    total = connection.QueryFirstOrDefault<int>(
                        @"SELECT count(A.COD_PRODUCTO)
                          FROM PV_CONTROL_ACTIVOS A
                          LEFT JOIN PV_PRODUCTOS P ON P.COD_PRODUCTO = A.COD_PRODUCTO
                          WHERE A.ENTREGA_USUARIO = ''
                            AND A.COD_UEN = @cod_unidad
                            AND REGISTRO_USUARIO = @usuario",
                        new
                        {
                            filtros.cod_unidad,
                            filtros.usuario
                        }),
                    lista = new List<InvRequesicionesActivosData>()
                };

                var parametros = new DynamicParameters();
                parametros.Add("Usuario", filtros.usuario);
                var queryBuilder = new StringBuilder(QueryActivosRequisicion);
                AgregarFiltrosActivos(filtros, queryBuilder, parametros);
                queryBuilder.Append(" ORDER BY A.COD_PRODUCTO ");
                AgregarPaginacionActivos(filtros, queryBuilder, parametros);

                respuesta.lista = connection.Query<InvRequesicionesActivosData>(queryBuilder.ToString(), parametros).ToList();
                return respuesta;
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new InvRequesicionesActivosLista())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener activos para requisiciones.", result.Code.GetValueOrDefault(-1), new InvRequesicionesActivosLista());
        }

        /// <summary>
        /// Obtiene los usuarios encargados de solicitudes.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <returns>Listado de usuarios.</returns>
        public ErrorDto<List<string>> ObtenerUsuario(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<string>(
                CreatePortalDb(),
                CodEmpresa,
                @"select ENCARGADO_USUARIO 
                  from CPR_SOLICITUD 
                  WHERE ENCARGADO_USUARIO IS NOT NULL
                  GROUP BY ENCARGADO_USUARIO");

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<string>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al obtener usuarios encargados.", result.Code.GetValueOrDefault(-1), new List<string>());
        }

        #endregion

        #region Mantenimiento

        /// <summary>
        /// Inserta una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la requisición.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranRequisicion_Insertar(int CodEmpresa, TranRequisicionData request)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                string ultimaBoleta = ObtenerSiguienteRequisicion(connection);

                connection.Execute(
                    @"insert pv_requisiciones(cod_requisicion, cod_entsal, genera_fecha, documento, notas, genera_user, estado, plantilla, cod_unidad, RECIBE_USER, RESPONSABLE_ACTIVO)
                      values(@CodReq, @Cod_Entsal, getdate(), @Documento, @Notas, @Genera_User, 'S', @Plantilla, @Cod_Unidad, @Recibe_user, @Responsable_Activo)",
                    new
                    {
                        CodReq = ultimaBoleta,
                        request.Cod_Entsal,
                        request.Documento,
                        request.Notas,
                        request.Genera_User,
                        request.Plantilla,
                        Cod_Unidad = request.cod_unidad,
                        Recibe_user = request.recibe_user,
                        Responsable_Activo = request.responsable_activo
                    });

                return new ErrorDto
                {
                    Code = 0,
                    Description = ultimaBoleta
                };
            });

            return result.Code == 0
                ? result.Result ?? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al insertar la requisición.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="request">Datos de la requisición.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranRequisicion_Actualizar(int CodEmpresa, TranRequisicionData request)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"Update pv_requisiciones
                  SET cod_Entsal = @Cod_Entsal,
                      genera_fecha = getdate(),
                      documento = @Documento,
                      notas = @Notas,
                      plantilla = @Plantilla,
                      cod_unidad = @Cod_Unidad
                  WHERE cod_requisicion = @CodReq",
                new
                {
                    CodReq = request.Cod_Requisicion,
                    request.Cod_Entsal,
                    request.Documento,
                    request.Notas,
                    request.Plantilla,
                    Cod_Unidad = request.cod_unidad
                });

            return CrearRespuestaNonQuery(result, "Requisición actualizada correctamente", "Error al actualizar la requisición.");
        }

        /// <summary>
        /// Elimina una requisición y su detalle.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvTranRequesicion_Eliminar(int CodEmpresa, int CodRequisicion)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                EliminarDetalleRequisicion(connection, CodRequisicion);
                connection.Execute(
                    "delete pv_requisiciones where cod_requisicion = @CodRequisicion",
                    new { CodRequisicion });
                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Requisición eliminada correctamente")
                : DbHelper.ErrorResponse(result.Description ?? "Error al eliminar la requisición.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Inserta productos de una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <param name="producLineas">Listado de productos.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvRequesicionProduc_Insertar(int CodEmpresa, int CodRequisicion, List<InvReqProduc> producLineas)
        {
            var result = DbHelper.WithConn<bool>(CreatePortalDb(), CodEmpresa, connection =>
            {
                var idControl = ObtenerIdsControlRequisicion(connection, CodRequisicion);
                LimpiarControlActivos(connection, idControl);
                EliminarDetalleRequisicion(connection, CodRequisicion);

                TranRequisicionData? encabezado = ObtenerEncabezadoRequisicion(connection, CodRequisicion);
                int contador = 0;
                foreach (InvReqProduc item in producLineas)
                {
                    contador++;
                    InsertarDetalleRequisicion(connection, CodRequisicion, contador, item);
                    ActualizarControlActivo(connection, CodRequisicion, encabezado, item);
                }

                return true;
            });

            return result.Code == 0 && result.Result
                ? DbHelper.OkResponse("Información guardada satisfactoriamente...")
                : DbHelper.ErrorResponse(result.Description ?? "Error al guardar los productos de la requisición.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Elimina un producto de una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <param name="Linea">Línea a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvRequisicionProduc_Eliminar(int CodEmpresa, int CodRequisicion, int Linea)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                "delete pv_requi_detalle where COD_REQUISICION = @CodRequisicion and linea = @Linea",
                new { CodRequisicion, Linea });

            return CrearRespuestaNonQuery(result, "Requisición eliminada correctamente", "Error al eliminar la línea de la requisición.");
        }

        /// <summary>
        /// Autoriza una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <param name="Usuario">Usuario que autoriza.</param>
        /// <param name="Estado">Estado a aplicar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvRequisicion_Autorizar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            var result = DbHelper.ExecuteNonQuery(
                CreatePortalDb(),
                CodEmpresa,
                @"update PV_REQUISICIONES
                  set estado = @Estado,
                      Autoriza_user = @Autoriza_User,
                      autoriza_fecha = getdate()
                  where cod_requisicion = @Requisicion",
                new
                {
                    Requisicion = CodRequisicion,
                    Autoriza_User = Usuario,
                    Estado
                });

            return CrearRespuestaNonQuery(result, "Requisición ejecutada correctamente", "Error al autorizar la requisición.");
        }

        /// <summary>
        /// Procesa una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="CodRequisicion">Código de la requisición.</param>
        /// <param name="Usuario">Usuario que procesa.</param>
        /// <param name="Estado">Estado a aplicar.</param>
        /// <returns>Resultado de la operación.</returns>
        public ErrorDto InvRequisicion_Procesar(int CodEmpresa, int CodRequisicion, string Usuario, string Estado)
        {
            var result = DbHelper.WithConn<int>(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<int>(
                    "[spINV_W_Requisicion_Procesa]",
                    new
                    {
                        Requisicion = CodRequisicion,
                        Procesa_Usuario = Usuario,
                        Estado
                    },
                    commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(result.Description ?? "Error al procesar la requisición.", result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.OkResponse("Requisición procesada correctamente")
                : DbHelper.ErrorResponse("Error al procesar la requisición.", result.Result);
        }

        /// <summary>
        /// Valida permisos de autorización o proceso para requisiciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de la empresa.</param>
        /// <param name="usuario">Usuario a validar.</param>
        /// <param name="cod_unidad">Código de unidad.</param>
        /// <param name="cod_proceso">Proceso a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        public ErrorDto ValidaAutorizacion(int CodEmpresa, string usuario, string cod_unidad, string cod_proceso)
        {
            var result = DbHelper.WithConn<ErrorDto>(CreatePortalDb(), CodEmpresa, connection =>
            {
                bool esMancomunado = connection.ExecuteScalar<int?>(
                    @"SELECT TOP 1 1 
                      FROM pv_entrada_salida 
                      WHERE MANCOMUNADO = 1
                        AND COD_ENTSAL = 'R'") == 1;

                if (esMancomunado)
                {
                    bool usuarioCoincide = connection.ExecuteScalar<int?>(
                        @"SELECT TOP 1 1 
                          FROM pv_requisiciones 
                          WHERE GENERA_USER = @usuario",
                        new { usuario }) == 1;

                    if (usuarioCoincide)
                    {
                        return new ErrorDto { Code = 2 };
                    }
                }

                string campoRol = cod_proceso == "P" ? "ROL_ENCARGADO" : "ROL_AUTORIZA";
                string queryAutoriza = $@"
                                        SELECT 1
                                        WHERE EXISTS (
                                            SELECT 1
                                            FROM CORE_UENS_USUARIOS_ROLES r
                                            WHERE r.CORE_USUARIO = @usuario
                                              AND r.{campoRol} = 1
                                              AND r.COD_UNIDAD = @cod_unidad
                                        );";

                bool esAutoriza = connection.ExecuteScalar<int?>(queryAutoriza, new { usuario, cod_unidad }) == 1;
                return new ErrorDto { Code = esAutoriza ? 1 : 0 };
            });

            return result.Code == 0
                ? result.Result ?? new ErrorDto { Code = 0 }
                : DbHelper.ErrorResponse(result.Description ?? "Error al validar autorización.", result.Code.GetValueOrDefault(-1));
        }

        #endregion
    }
}