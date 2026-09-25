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
    public class FrmInvTranRequisicionesDB
    {
        private readonly IConfiguration _config;

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
                    "El c&oacute;digo de la requisici&oacute;n es requerido.",
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
        /// Obtiene las líneas de una requisición con el costo regular del producto.
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
                    "El c&oacute;digo de la requisici&oacute;n es requerido.",
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

        /// <summary>
        /// Inserta el encabezado de una requisición con las columnas usadas por VB6.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la requisición.</param>
        /// <returns>Consecutivo generado.</returns>
        public ErrorDto InvTranRequisicion_Insertar(
            int CodEmpresa,
            TranRequisicionData? request)
        {
            if (request is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos de la requisici&oacute;n son requeridos.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "La causa de la requisici&oacute;n es requerida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.genera_user))
            {
                return DbHelper.ErrorResponse(
                    "El usuario generador es requerido.",
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        int consecutivo =
                            InvTranRequisiciones_Consecutivo_Obtener(
                                connection,
                                transaction);

                        connection.Execute(
                            """
                            INSERT INTO pv_requisiciones
                                (cod_requisicion, cod_entsal, genera_fecha,
                                 documento, notas, genera_user, estado,
                                 plantilla)
                            VALUES
                                (@CodRequisicion, @CodEntsal, GETDATE(),
                                 @Documento, @Notas, @GeneraUser, 'S',
                                 @Plantilla)
                            """,
                            new
                            {
                                CodRequisicion = consecutivo,
                                CodEntsal = request.cod_entsal.Trim(),
                                Documento = request.documento ?? string.Empty,
                                Notas = request.notas ?? string.Empty,
                                GeneraUser = request.genera_user.Trim(),
                                Plantilla = request.plantilla
                            },
                            transaction);

                        transaction.Commit();

                        return consecutivo.ToString(
                            "D10",
                            CultureInfo.InvariantCulture);
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            return result.Code == 0
                ? DbHelper.OkResponse(result.Result ?? string.Empty)
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al insertar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Actualiza el encabezado de una requisición solicitada.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="request">Datos de la requisición.</param>
        /// <returns>Resultado de la actualización.</returns>
        public ErrorDto InvTranRequisicion_Actualizar(
            int CodEmpresa,
            TranRequisicionData? request)
        {
            if (request is null || request.cod_requisicion <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo de la requisici&oacute;n es requerido.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.cod_entsal))
            {
                return DbHelper.ErrorResponse(
                    "La causa de la requisici&oacute;n es requerida.",
                    -2);
            }

            if (string.IsNullOrWhiteSpace(request.genera_user))
            {
                return DbHelper.ErrorResponse(
                    "El usuario generador es requerido.",
                    -2);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                UPDATE pv_requisiciones
                SET cod_entsal = @CodEntsal,
                    genera_fecha = GETDATE(),
                    documento = @Documento,
                    notas = @Notas,
                    genera_user = @GeneraUser,
                    plantilla = @Plantilla
                WHERE cod_requisicion = @CodRequisicion
                  AND estado = 'S'
                """,
                new
                {
                    CodRequisicion = request.cod_requisicion,
                    CodEntsal = request.cod_entsal.Trim(),
                    Documento = request.documento ?? string.Empty,
                    Notas = request.notas ?? string.Empty,
                    GeneraUser = request.genera_user.Trim(),
                    Plantilla = request.plantilla
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al actualizar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "Requisici&oacute;n actualizada correctamente.")
                : DbHelper.ErrorResponse(
                    "La requisici&oacute;n no existe o no est&aacute; solicitada.",
                    -2);
        }

        /// <summary>
        /// Elimina una requisición y su detalle mediante el endpoint existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto InvTranRequesicion_Eliminar(
            int CodEmpresa,
            int CodRequisicion)
        {
            if (CodRequisicion <= 0)
            {
                return DbHelper.ErrorResponse(
                    "El c&oacute;digo de la requisici&oacute;n es requerido.",
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        connection.Execute(
                            """
                            DELETE FROM pv_requi_detalle
                            WHERE cod_requisicion = @CodRequisicion
                            """,
                            new { CodRequisicion },
                            transaction);

                        int afectados = connection.Execute(
                            """
                            DELETE FROM pv_requisiciones
                            WHERE cod_requisicion = @CodRequisicion
                            """,
                            new { CodRequisicion },
                            transaction);

                        transaction.Commit();
                        return afectados;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description ?? "Error al eliminar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "Requisici&oacute;n eliminada correctamente.")
                : DbHelper.ErrorResponse(
                    "No se encontr&oacute; la requisici&oacute;n.",
                    -2);
        }

        /// <summary>
        /// Reemplaza el detalle de una requisición como lo hace VB6 al guardar.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="producLineas">Líneas visibles que se van a guardar.</param>
        /// <returns>Resultado del guardado.</returns>
        public ErrorDto InvRequesicionProduc_Insertar(
            int CodEmpresa,
            int CodRequisicion,
            List<InvReqProduc>? producLineas)
        {
            if (CodRequisicion <= 0 || producLineas is null)
            {
                return DbHelper.ErrorResponse(
                    "Los datos del detalle son requeridos.",
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    using var transaction = connection.BeginTransaction();

                    try
                    {
                        string? estado = connection.QueryFirstOrDefault<string>(
                            """
                            SELECT estado
                            FROM pv_requisiciones WITH (UPDLOCK, HOLDLOCK)
                            WHERE cod_requisicion = @CodRequisicion
                            """,
                            new { CodRequisicion },
                            transaction);

                        if (estado != "S")
                        {
                            transaction.Rollback();
                            return false;
                        }

                        InvTranRequisiciones_Detalle_Eliminar(
                            connection,
                            transaction,
                            CodRequisicion);

                        for (int indice = 0; indice < producLineas.Count; indice++)
                        {
                            InvReqProduc item = producLineas[indice];

                            if (string.IsNullOrWhiteSpace(item.cod_producto)
                                || item.cantidad <= 0)
                            {
                                continue;
                            }

                            InvTranRequisiciones_Detalle_Insertar(
                                connection,
                                transaction,
                                CodRequisicion,
                                indice + 1,
                                item);
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al guardar los productos de la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result
                ? DbHelper.OkResponse(
                    "Informaci&oacute;n guardada satisfactoriamente.")
                : DbHelper.ErrorResponse(
                    "La requisici&oacute;n no existe o no est&aacute; solicitada.",
                    -2);
        }

        /// <summary>
        /// Elimina una línea mediante el endpoint existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="Linea">Número de línea.</param>
        /// <returns>Resultado de la eliminación.</returns>
        public ErrorDto InvRequisicionProduc_Eliminar(
            int CodEmpresa,
            int CodRequisicion,
            int Linea)
        {
            if (CodRequisicion <= 0 || Linea <= 0)
            {
                return DbHelper.ErrorResponse(
                    "La requisici&oacute;n y la l&iacute;nea son requeridas.",
                    -2);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                DELETE FROM pv_requi_detalle
                WHERE cod_requisicion = @CodRequisicion
                  AND linea = @Linea
                """,
                new { CodRequisicion, Linea });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al eliminar la l&iacute;nea de requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "L&iacute;nea de requisici&oacute;n eliminada correctamente.")
                : DbHelper.ErrorResponse(
                    "No se encontr&oacute; la l&iacute;nea de requisici&oacute;n.",
                    -2);
        }

        /// <summary>
        /// Autoriza o rechaza mediante el endpoint existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="Usuario">Usuario que autoriza.</param>
        /// <param name="Estado">A para autorizar o R para rechazar.</param>
        /// <returns>Resultado de la autorización.</returns>
        public ErrorDto InvRequisicion_Autorizar(
            int CodEmpresa,
            int CodRequisicion,
            string Usuario,
            string Estado)
        {
            if (CodRequisicion <= 0
                || string.IsNullOrWhiteSpace(Usuario)
                || (Estado != "A" && Estado != "R"))
            {
                return DbHelper.ErrorResponse(
                    "Los datos de autorizaci&oacute;n no son v&aacute;lidos.",
                    -2);
            }

            var result = DbHelper.ExecuteNonQueryWithResult(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                """
                UPDATE pv_requisiciones
                SET estado = @Estado,
                    autoriza_user = @Usuario,
                    autoriza_fecha = GETDATE()
                WHERE cod_requisicion = @CodRequisicion
                """,
                new
                {
                    CodRequisicion,
                    Usuario = Usuario.Trim(),
                    Estado
                });

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al autorizar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result > 0
                ? DbHelper.OkResponse(
                    "Requisici&oacute;n ejecutada correctamente.")
                : DbHelper.ErrorResponse(
                    "No se encontr&oacute; la requisici&oacute;n.",
                    -2);
        }

        /// <summary>
        /// Procesa la requisición mediante el procedimiento existente.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="CodRequisicion">Código de requisición.</param>
        /// <param name="Usuario">Usuario que procesa.</param>
        /// <param name="Estado">Estado requerido por el procedimiento.</param>
        /// <returns>Resultado del proceso.</returns>
        public ErrorDto InvRequisicion_Procesar(
            int CodEmpresa,
            int CodRequisicion,
            string Usuario,
            string Estado)
        {
            if (CodRequisicion <= 0 || string.IsNullOrWhiteSpace(Usuario))
            {
                return DbHelper.ErrorResponse(
                    "Los datos para procesar la requisici&oacute;n son requeridos.",
                    -2);
            }

            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection => connection.QueryFirstOrDefault<int>(
                    "spINV_W_Requisicion_Procesa",
                    new
                    {
                        Requisicion = CodRequisicion,
                        Procesa_Usuario = Usuario.Trim(),
                        Estado
                    },
                    commandType: CommandType.StoredProcedure));

            if (result.Code != 0)
            {
                return DbHelper.ErrorResponse(
                    result.Description
                        ?? "Error al procesar la requisici&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
            }

            return result.Result == 0
                ? DbHelper.OkResponse(
                    "Requisici&oacute;n procesada correctamente.")
                : DbHelper.ErrorResponse(
                    "Error al procesar la requisici&oacute;n.",
                    result.Result);
        }

        /// <summary>
        /// Valida la autorización y el rol de la unidad.
        /// </summary>
        /// <param name="CodEmpresa">Código de empresa.</param>
        /// <param name="usuario">Usuario a validar.</param>
        /// <param name="cod_unidad">Código de unidad.</param>
        /// <param name="cod_proceso">P para procesar u otro valor para autorizar.</param>
        /// <returns>Código de validación del contrato existente.</returns>
        public ErrorDto ValidaAutorizacion(
            int CodEmpresa,
            string usuario,
            string cod_unidad,
            string cod_proceso)
        {
            var result = DbHelper.WithConn(
                InvTranRequisiciones_Portal_Crear(),
                CodEmpresa,
                connection =>
                {
                    bool esMancomunado = connection.ExecuteScalar<int?>(
                        """
                        SELECT TOP 1 1
                        FROM pv_entrada_salida
                        WHERE MANCOMUNADO = 1
                          AND COD_ENTSAL = 'R'
                        """) == 1;

                    if (esMancomunado)
                    {
                        bool usuarioCoincide = connection.ExecuteScalar<int?>(
                            """
                            SELECT TOP 1 1
                            FROM pv_requisiciones
                            WHERE GENERA_USER = @Usuario
                            """,
                            new { Usuario = usuario }) == 1;

                        if (usuarioCoincide)
                        {
                            return new ErrorDto { Code = 2 };
                        }
                    }

                    bool tieneRol = connection.ExecuteScalar<int?>(
                        """
                        SELECT TOP 1 1
                        FROM CORE_UENS_USUARIOS_ROLES R
                        WHERE R.CORE_USUARIO = @Usuario
                          AND R.COD_UNIDAD = @CodUnidad
                          AND (
                                (@CodProceso = 'P'
                                 AND R.ROL_ENCARGADO = 1)
                                OR
                                (@CodProceso <> 'P'
                                 AND R.ROL_AUTORIZA = 1)
                              )
                        """,
                        new
                        {
                            Usuario = usuario,
                            CodUnidad = cod_unidad,
                            CodProceso = cod_proceso
                        }) == 1;

                    return new ErrorDto { Code = tieneRol ? 1 : 0 };
                });

            return result.Code == 0
                ? result.Result ?? new ErrorDto { Code = 0 }
                : DbHelper.ErrorResponse(
                    result.Description ?? "Error al validar autorizaci&oacute;n.",
                    result.Code.GetValueOrDefault(-1));
        }
    }
}