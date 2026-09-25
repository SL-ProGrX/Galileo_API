using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.INV;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Data;
using System.Globalization;

namespace Galileo.DataBaseTier
{
    public partial class FrmInvTranRequisicionesDB
    {
        private readonly IConfiguration _config;

        private const string MensajeCodigoRequisicionRequerido =
            "El c&oacute;digo de la requisici&oacute;n es requerido.";

        /// <summary>
        /// Inicializa el acceso a datos de requisiciones.
        /// </summary>
        /// <param name="config">Configuración de la aplicación.</param>
        public FrmInvTranRequisicionesDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Crea el acceso a la base de datos de la empresa.
        /// </summary>
        /// <returns>Instancia de PortalDB.</returns>
        private PortalDB InvTranRequisiciones_Portal_Crear()
        {
            return new PortalDB(_config);
        }

        /// <summary>
        /// Obtiene la descripción del estado de la requisición.
        /// </summary>
        /// <param name="estado">Estado almacenado.</param>
        /// <returns>Descripción del estado.</returns>
        private static string InvTranRequisiciones_Estado_Describir(string? estado)
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
        /// Obtiene el siguiente consecutivo dentro de la transacción.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <returns>Consecutivo disponible.</returns>
        private static int InvTranRequisiciones_Consecutivo_Obtener(
            IDbConnection connection,
            IDbTransaction transaction)
        {
            return connection.QuerySingle<int>(
                """
                SELECT ISNULL(MAX(cod_requisicion), 0) + 1
                FROM pv_requisiciones WITH (UPDLOCK, HOLDLOCK)
                """,
                transaction: transaction);
        }

        /// <summary>
        /// Elimina el detalle anterior para reemplazarlo al guardar.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        private static void InvTranRequisiciones_Detalle_Eliminar(
            IDbConnection connection,
            IDbTransaction transaction,
            int codRequisicion)
        {
            connection.Execute(
                """
                DELETE FROM pv_requi_detalle
                WHERE cod_requisicion = @CodRequisicion
                """,
                new { CodRequisicion = codRequisicion },
                transaction);
        }

        /// <summary>
        /// Inserta una línea usando las columnas que guarda VB6.
        /// </summary>
        /// <param name="connection">Conexión abierta.</param>
        /// <param name="transaction">Transacción activa.</param>
        /// <param name="codRequisicion">Código de requisición.</param>
        /// <param name="linea">Número de línea.</param>
        /// <param name="item">Producto y cantidad.</param>
        private static void InvTranRequisiciones_Detalle_Insertar(
            IDbConnection connection,
            IDbTransaction transaction,
            int codRequisicion,
            int linea,
            InvReqProduc item)
        {
            connection.Execute(
                """
                INSERT INTO pv_requi_detalle
                    (linea, cod_requisicion, cod_producto, cantidad, despacho)
                VALUES
                    (@Linea, @CodRequisicion, @CodProducto, @Cantidad, 0)
                """,
                new
                {
                    Linea = linea,
                    CodRequisicion = codRequisicion,
                    CodProducto = item.cod_producto.Trim(),
                    Cantidad = item.cantidad
                },
                transaction);
        }

        /// <summary>
        /// Obtiene el encabezado de una requisición.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <returns>Encabezado encontrado.</returns>
        public ErrorDto<TranRequisicionData> InvTranRequisicion_Obtener(
            int CodEmpresa,
            int CodRequisicion)
        {
            if (CodRequisicion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCodigoRequisicionRequerido,
                    -2,
                    new TranRequisicionData());
            }

            var result = DbHelper.ExecuteSingleQuery(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT X.*,
                       RTRIM(C.cod_entsal) + ' - ' + C.descripcion AS causa
                FROM pv_requisiciones X
                INNER JOIN pv_entrada_salida C
                    ON X.cod_entsal = C.cod_entsal
                WHERE X.cod_requisicion = @CodRequisicion
                """,
                new TranRequisicionData(),
                new { CodRequisicion });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al obtener la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1),
                    new TranRequisicionData());
            }

            TranRequisicionData data =
                result.Result ?? new TranRequisicionData();

            data.estado = InvTranRequisiciones_Estado_Describir(data.estado);
            return DbHelper.CreateOkResponse(data);
        }

        /// <summary>
        /// Obtiene el detalle con el costo regular del producto.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <returns>Productos de la requisición.</returns>
        public ErrorDto<List<InvReqProduc>> InvRequesicionProduc_Obtener(
            int CodEmpresa,
            int CodRequisicion)
        {
            if (CodRequisicion <= 0)
            {
                return DbHelper.CreateErrorResponse(
                    MensajeCodigoRequisicionRequerido,
                    -2,
                    new List<InvReqProduc>());
            }

            return DbHelper.ExecuteListQuery<InvReqProduc>(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT D.linea,
                       D.cod_producto,
                       P.descripcion,
                       D.cantidad,
                       P.costo_regular AS costo,
                       P.costo_regular AS costo_registrado,
                       D.cantidad * P.costo_regular AS total,
                       ISNULL(D.despacho, 0) AS despacho
                FROM pv_requi_detalle D
                INNER JOIN pv_productos P
                    ON D.cod_producto = P.cod_producto
                WHERE D.cod_requisicion = @CodRequisicion
                ORDER BY D.linea
                """,
                new { CodRequisicion });
        }

        /// <summary>
        /// Obtiene la requisición anterior o siguiente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="scrollValue">Uno para avanzar; otro valor para retroceder.</param>
        /// <param name="CodRequisicion">Código actual.</param>
        /// <returns>Requisición localizada o un modelo vacío.</returns>
        public ErrorDto<TranRequisicionData> InvTranRequisicion_scroll(
            int CodEmpresa,
            int scrollValue,
            int? CodRequisicion)
        {
            var result = DbHelper.ExecuteSingleQuery<int>(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT TOP 1 cod_requisicion
                FROM pv_requisiciones
                WHERE (@ScrollValue = 1
                       AND cod_requisicion > @CodRequisicion)
                   OR (@ScrollValue <> 1
                       AND cod_requisicion < @CodRequisicion)
                ORDER BY
                    CASE WHEN @ScrollValue = 1
                         THEN cod_requisicion END ASC,
                    CASE WHEN @ScrollValue <> 1
                         THEN cod_requisicion END DESC
                """,
                0,
                new
                {
                    ScrollValue = scrollValue,
                    CodRequisicion = CodRequisicion ?? 0
                });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse(
                    result.Description ?? "Error al desplazar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1),
                    new TranRequisicionData());
            }

            return result.Result > 0
                ? InvTranRequisicion_Obtener(CodEmpresa, result.Result)
                : DbHelper.CreateOkResponse(new TranRequisicionData());
        }

        /// <summary>
        /// Obtiene las requisiciones marcadas como plantilla.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código opcional.</param>
        /// <param name="GeneraUser">Usuario generador opcional.</param>
        /// <param name="GeneraFecha">Fecha opcional en formato yyyy-MM-dd.</param>
        /// <returns>Plantillas encontradas.</returns>
        public ErrorDto<List<TranRequisicionData>> InvTranPlantilla_Obtener(
            int CodEmpresa,
            int? CodRequisicion,
            string? GeneraUser,
            string? GeneraFecha)
        {
            DateTime? fechaInicio = null;
            DateTime? fechaFin = null;

            if (!string.IsNullOrWhiteSpace(GeneraFecha))
            {
                if (!DateTime.TryParseExact(
                        GeneraFecha.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out DateTime fecha))
                {
                    return DbHelper.CreateErrorResponse(
                        "La fecha debe utilizar el formato yyyy-MM-dd.",
                        -2,
                        new List<TranRequisicionData>());
                }

                fechaInicio = fecha.Date;
                fechaFin = fechaInicio.Value.AddDays(1);
            }

            return DbHelper.ExecuteListQuery<TranRequisicionData>(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT cod_requisicion,
                       genera_user,
                       genera_fecha,
                       documento,
                       notas
                FROM pv_requisiciones
                WHERE plantilla = 1
                  AND (@CodRequisicion IS NULL
                       OR cod_requisicion = @CodRequisicion)
                  AND (@GeneraUser IS NULL
                       OR genera_user LIKE @GeneraUser)
                  AND (@FechaInicio IS NULL
                       OR genera_fecha >= @FechaInicio)
                  AND (@FechaFin IS NULL
                       OR genera_fecha < @FechaFin)
                ORDER BY cod_requisicion
                """,
                new
                {
                    CodRequisicion = CodRequisicion is > 0
                        ? CodRequisicion
                        : null,
                    GeneraUser = string.IsNullOrWhiteSpace(GeneraUser)
                        ? null
                        : $"%{GeneraUser.Trim()}%",
                    FechaInicio = fechaInicio,
                    FechaFin = fechaFin
                });
        }

        /// <summary>
        /// Obtiene el listado de requisiciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario o T para todos.</param>
        /// <param name="columna">G para generador o A para autorizador.</param>
        /// <param name="estado">Estado o T para todos.</param>
        /// <returns>Requisiciones encontradas.</returns>
        public ErrorDto<List<TranRequisicionData>> InvTranRequisiciones_Lista(
            int CodEmpresa,
            string usuario,
            string columna,
            string estado)
        {
            return DbHelper.ExecuteListQuery<TranRequisicionData>(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT *
                FROM pv_requisiciones
                WHERE (@Estado = 'T' OR estado = @Estado)
                  AND (
                        @Usuario = 'T'
                        OR @Usuario = ''
                        OR @Columna NOT IN ('G', 'A')
                        OR (@Columna = 'G' AND genera_user = @Usuario)
                        OR (@Columna = 'A' AND autoriza_user = @Usuario)
                      )
                ORDER BY cod_requisicion
                """,
                new
                {
                    Usuario = usuario?.Trim() ?? string.Empty,
                    Columna = columna?.Trim().ToUpperInvariant()
                        ?? string.Empty,
                    Estado = string.IsNullOrWhiteSpace(estado)
                        ? "T"
                        : estado.Trim().ToUpperInvariant()
                });
        }

        /// <summary>
        /// Obtiene el catálogo de unidades de negocio usado por los endpoints actuales.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Unidades de negocio.</returns>
        public ErrorDto<List<CatalogosLista>> UENS_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CatalogosLista>(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT COD_UNIDAD AS item,
                       DESCRIPCION AS descripcion
                FROM CORE_UENS
                """);
        }

        /// <summary>
        /// Obtiene los usuarios receptores de una unidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="cod_unidad">Código de unidad.</param>
        /// <returns>Usuarios receptores.</returns>
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuarioRecibeLista_Obtener(
            int CodEmpresa,
            string cod_unidad)
        {
            return DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    var usuarios = connection.Query<InvRequsUsuarioRecibe>(
                        """
                        SELECT R.CORE_USUARIO AS usuario,
                               U.DESCRIPCION AS nombre,
                               '' AS identificacion
                        FROM CORE_UENS_USUARIOS_ROLES R
                        LEFT JOIN USUARIOS U
                            ON U.NOMBRE = R.CORE_USUARIO
                        WHERE R.COD_UNIDAD = @CodUnidad
                        """,
                        new { CodUnidad = cod_unidad }).ToList();

                    if (usuarios.Count == 0)
                    {
                        return usuarios;
                    }

                    string? portalConnectionString =
                        _config.GetConnectionString("DefaultConnString");

                    if (string.IsNullOrWhiteSpace(portalConnectionString))
                    {
                        throw new InvalidOperationException(
                            "No se configur&oacute; la conexi&oacute;n al portal.");
                    }

                    using var portalConnection =
                        new SqlConnection(portalConnectionString);

                    foreach (InvRequsUsuarioRecibe item in usuarios)
                    {
                        var usuarioPortal =
                            portalConnection.QueryFirstOrDefault<InvRequsUsuarioRecibe>(
                                """
                                SELECT USUARIO AS usuario,
                                       NOMBRE AS nombre,
                                       IDENTIFICACION AS identificacion
                                FROM [PGX_Portal].[dbo].[US_USUARIOS]
                                WHERE USUARIO = @Usuario
                                """,
                                new { Usuario = item.usuario });

                        if (usuarioPortal is null)
                        {
                            continue;
                        }

                        item.nombre = usuarioPortal.nombre;
                        item.identificacion = usuarioPortal.identificacion;
                    }

                    return usuarios;
                });
        }

        /// <summary>
        /// Obtiene los usuarios responsables de activos.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Usuarios responsables.</returns>
        public ErrorDto<List<InvRequsUsuarioRecibe>> UsuariosActivoLista_Obtener(
            int CodEmpresa)
        {
            return DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    string? portalConnectionString =
                        _config.GetConnectionString("DefaultConnString");

                    if (string.IsNullOrWhiteSpace(portalConnectionString))
                    {
                        throw new InvalidOperationException(
                            "No se configur&oacute; la conexi&oacute;n al portal.");
                    }

                    using var portalConnection =
                        new SqlConnection(portalConnectionString);

                    var usuarios =
                        portalConnection.Query<InvRequsUsuarioRecibe>(
                            """
                            SELECT USUARIO AS usuario,
                                   NOMBRE AS nombre,
                                   IDENTIFICACION AS identificacion
                            FROM [PGX_Portal].[dbo].[US_USUARIOS]
                            """).ToList();

                    var responsables = connection.Query<string>(
                        """
                        SELECT DISTINCT identificacion
                        FROM ACTIVOS_RESPONSABLES
                        WHERE identificacion IS NOT NULL
                        """).ToHashSet(StringComparer.OrdinalIgnoreCase);

                    return usuarios
                        .Where(item => responsables.Contains(item.identificacion))
                        .ToList();
                });
        }

        /// <summary>
        /// Obtiene productos y activos disponibles para requisiciones.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="invReqFiltros">Filtros serializados como JSON.</param>
        /// <returns>Listado paginado y cantidad total.</returns>
        public ErrorDto<InvRequesicionesActivosLista>
            ProductosRequesicionesActivo_Obtener(
                int CodEmpresa,
                string invReqFiltros)
        {
            InvReqFiltros filtros;

            try
            {
                filtros = string.IsNullOrWhiteSpace(invReqFiltros)
                    ? new InvReqFiltros()
                    : JsonConvert.DeserializeObject<InvReqFiltros>(
                        invReqFiltros) ?? new InvReqFiltros();
            }
            catch (JsonException)
            {
                return DbHelper.CreateErrorResponse(
                    "Los filtros de art&iacute;culos no son v&aacute;lidos.",
                    -2,
                    new InvRequesicionesActivosLista());
            }

            int offset = Math.Max(0, filtros.pagina ?? 0);
            int fetch = filtros.paginacion is > 0
                ? filtros.paginacion.Value
                : int.MaxValue;
            string? filtro = string.IsNullOrWhiteSpace(filtros.filtro)
                ? null
                : $"%{filtros.filtro.Trim()}%";

            return DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    var parametros = new
                    {
                        Usuario = filtros.usuario?.Trim() ?? string.Empty,
                        Filtro = filtro,
                        Offset = offset,
                        Fetch = fetch
                    };

                    int total = connection.QuerySingle<int>(
                        """
                        SELECT COUNT(*)
                        FROM PV_PRODUCTOS P
                        LEFT JOIN PV_CONTROL_ACTIVOS A
                            ON A.COD_PRODUCTO = P.COD_PRODUCTO
                           AND A.ENTREGA_USUARIO = ''
                           AND A.REGISTRO_USUARIO = @Usuario
                        WHERE P.COD_PRODCLAS NOT IN (118)
                          AND (
                                @Filtro IS NULL
                                OR P.DESCRIPCION LIKE @Filtro
                                OR A.COD_PRODUCTO LIKE @Filtro
                                OR P.CABYS LIKE @Filtro
                                OR P.COD_BARRAS LIKE @Filtro
                              )
                        """,
                        parametros);

                    var lista =
                        connection.Query<InvRequesicionesActivosData>(
                            """
                            SELECT ISNULL(A.ID_CONTROL, 0) AS id_control,
                                   P.COD_PRODUCTO AS cod_producto,
                                   P.DESCRIPCION AS descripcion,
                                   CASE
                                       WHEN A.COD_PRODUCTO IS NOT NULL THEN 1
                                       ELSE P.EXISTENCIA
                                   END AS cantidad,
                                   COALESCE(
                                       A.COSTO_UNITARIO,
                                       P.COSTO_REGULAR
                                   ) AS costo,
                                   A.COSTO_UNITARIO AS costo_unitario,
                                   A.COD_BODEGA AS cod_bodega,
                                   P.CABYS AS cabys,
                                   P.COD_BARRAS AS cod_barras,
                                   A.NUMERO_PLACA AS numero_placa
                            FROM PV_PRODUCTOS P
                            LEFT JOIN PV_CONTROL_ACTIVOS A
                                ON A.COD_PRODUCTO = P.COD_PRODUCTO
                               AND A.ENTREGA_USUARIO = ''
                               AND A.REGISTRO_USUARIO = @Usuario
                            WHERE P.COD_PRODCLAS NOT IN (118)
                              AND (
                                    @Filtro IS NULL
                                    OR P.DESCRIPCION LIKE @Filtro
                                    OR A.COD_PRODUCTO LIKE @Filtro
                                    OR P.CABYS LIKE @Filtro
                                    OR P.COD_BARRAS LIKE @Filtro
                                  )
                            ORDER BY P.COD_PRODUCTO, A.ID_CONTROL
                            OFFSET @Offset ROWS
                            FETCH NEXT @Fetch ROWS ONLY
                            """,
                            parametros).ToList();

                    return new InvRequesicionesActivosLista
                    {
                        total = total,
                        lista = lista
                    };
                });
        }

        /// <summary>
        /// Obtiene los usuarios encargados registrados en solicitudes.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <returns>Usuarios encontrados.</returns>
        public ErrorDto<List<string>> ObtenerUsuario(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<string>(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                SELECT ENCARGADO_USUARIO
                FROM CPR_SOLICITUD
                WHERE ENCARGADO_USUARIO IS NOT NULL
                GROUP BY ENCARGADO_USUARIO
                """);
        }
    }
}