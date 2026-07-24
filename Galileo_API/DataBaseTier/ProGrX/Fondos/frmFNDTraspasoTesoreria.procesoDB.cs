using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Fondos;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace Galileo.DataBaseTier.ProGrX.Fondos
{
    public partial class FrmFndTraspasoTesoreriaDb
    {
        private const int FndTraspasoTesoreriaTamanoLoteProceso = 200;

        /// <summary>
        /// Inicializa el manifiesto persistente para procesar un traspaso de tesorería.
        /// </summary>
        public ErrorDto<FndTraspasoTesoreriaProcesoResult> FND_TraspasoTesoreria_Proceso_Iniciar(
            int codEmpresa,
            FndTraspasoTesoreriaProcesoIniciarRequest request)
        {
            string? validacion = FND_TraspasoTesoreria_Proceso_ValidarInicio(request);
            if (!string.IsNullOrWhiteSpace(validacion))
                return DbHelper.CreateErrorResponse<FndTraspasoTesoreriaProcesoResult>(validacion);

            try
            {
                using SqlConnection conn = DbHelper.OpenConnection(new PortalDB(_config), codEmpresa);
                conn.Open();

                string modo = request.Modo.Trim().ToUpperInvariant();
                string accion = request.Accion.Trim().ToUpperInvariant();
                string solicitudHash =
                    FND_TraspasoTesoreria_Proceso_CalcularHash(request, modo, accion);
                string seleccionXml =
                    FND_TraspasoTesoreria_Proceso_CrearSeleccionXml(request, modo);
                string recursoBloqueo = FND_TraspasoTesoreria_Proceso_Bloqueo_Adquirir(
                    conn,
                    solicitudHash);
                try
                {
                    var procesoActivo =
                        FND_TraspasoTesoreria_Proceso_Activo_Equivalente_Obtener(
                            conn,
                            solicitudHash);
                    if (procesoActivo != null)
                    {
                        var procesoRecuperado = FND_TraspasoTesoreria_Proceso_Consultar(
                            conn,
                            procesoActivo.ProcesoId,
                            procesoActivo.Usuario);
                        procesoRecuperado.ProcesoRecuperado = true;
                        return DbHelper.CreateOkResponse(procesoRecuperado);
                    }

                    try
                    {
                        conn.Execute(
                            "spFND_W_TraspasoTesoreria_Proceso_Iniciar",
                            new
                            {
                                request.ProcesoId,
                                Modo = modo,
                                Accion = accion,
                                request.Usuario,
                                Token = request.Token?.Trim(),
                                RetencionCodigo = request.RetencionCodigo?.Trim(),
                                FechaDesde = request.FechaDesde?.Date,
                                FechaHasta = request.FechaHasta?.Date,
                                request.AplicaRevision,
                                request.BancoId,
                                Oficina = request.Oficina?.Trim(),
                                UsuarioFiltro = FND_TraspasoTesoreria_Proceso_CrearFiltro(request.UsuarioFiltro),
                                SistemaFiltro = FND_TraspasoTesoreria_Proceso_CrearFiltro(request.SistemaFiltro),
                                TokenFiltro = FND_TraspasoTesoreria_Proceso_CrearFiltro(request.TokenFiltro),
                                AppProductName = request.AppProductName.Trim(),
                                SeleccionXml = seleccionXml,
                                SolicitudHash = solicitudHash
                            },
                            commandType: CommandType.StoredProcedure,
                            commandTimeout: 0);
                    }
                    catch (SqlException ex) when (
                        FND_TraspasoTesoreria_Proceso_EsConflictoPendiente(ex.Message))
                    {
                        var procesoSolapado =
                            FND_TraspasoTesoreria_Proceso_Activo_Seleccion_Obtener(
                                conn,
                                seleccionXml,
                                modo);
                        if (procesoSolapado is null)
                            throw;

                        var procesoRecuperado = FND_TraspasoTesoreria_Proceso_Consultar(
                            conn,
                            procesoSolapado.ProcesoId,
                            procesoSolapado.Usuario);
                        procesoRecuperado.ProcesoRecuperado = true;
                        return DbHelper.CreateOkResponse(procesoRecuperado);
                    }

                    return DbHelper.CreateOkResponse(
                        FND_TraspasoTesoreria_Proceso_Consultar(
                            conn,
                            request.ProcesoId,
                            request.Usuario));
                }
                finally
                {
                    FND_TraspasoTesoreria_Proceso_Bloqueo_Liberar(
                        conn,
                        recursoBloqueo);
                }
            }
            catch (Exception ex)
            {
                return FND_TraspasoTesoreria_Proceso_CrearError(ex, "iniciar");
            }
        }

        /// <summary>
        /// Ejecuta el siguiente lote pendiente del manifiesto de traspaso de tesorería.
        /// </summary>
        public ErrorDto<FndTraspasoTesoreriaProcesoResult> FND_TraspasoTesoreria_Proceso_Continuar(
            int codEmpresa,
            FndTraspasoTesoreriaProcesoContinuarRequest request)
        {
            string? validacion = FND_TraspasoTesoreria_Proceso_ValidarContinuacion(request);
            if (!string.IsNullOrWhiteSpace(validacion))
                return DbHelper.CreateErrorResponse<FndTraspasoTesoreriaProcesoResult>(validacion);

            try
            {
                using SqlConnection conn = DbHelper.OpenConnection(new PortalDB(_config), codEmpresa);
                conn.Open();
                var proceso =
                    FND_TraspasoTesoreria_Proceso_Contexto_Obtener(
                        conn,
                        request.ProcesoId);
                if (proceso.Estado is "C" or "E")
                {
                    return DbHelper.CreateOkResponse(
                        FND_TraspasoTesoreria_Proceso_Consultar(
                            conn,
                            request.ProcesoId,
                            proceso.Usuario));
                }

                conn.Execute(
                    "spFND_W_TraspasoTesoreria_Lote_Procesar",
                    new
                    {
                        request.ProcesoId,
                        proceso.Usuario,
                        TamanoLote = FndTraspasoTesoreriaTamanoLoteProceso,
                        request.ReintentarErrores
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0);

                return DbHelper.CreateOkResponse(
                    FND_TraspasoTesoreria_Proceso_Consultar(
                        conn,
                        request.ProcesoId,
                        proceso.Usuario));
            }
            catch (Exception ex)
            {
                return FND_TraspasoTesoreria_Proceso_CrearError(ex, "continuar");
            }
        }

        private static string? FND_TraspasoTesoreria_Proceso_ValidarInicio(
            FndTraspasoTesoreriaProcesoIniciarRequest request)
        {
            if (request is null) return "La solicitud es requerida.";
            if (request.ProcesoId == Guid.Empty) return "El identificador del proceso es requerido.";
            if (string.IsNullOrWhiteSpace(request.Usuario)) return "El usuario es requerido.";

            string modo = request.Modo.Trim().ToUpperInvariant();
            string accion = request.Accion.Trim().ToUpperInvariant();

            if (modo is not ("G" or "U")) return "El modo del proceso no es válido.";
            if (accion is not ("D" or "R")) return "La acción del proceso no es válida.";
            if (modo == "U" && accion != "D")
                return "Los casos unificados solamente permiten desembolsar.";
            if (request.FechaDesde == default || request.FechaHasta == default ||
                request.FechaHasta?.Date < request.FechaDesde?.Date)
                return "El rango de fechas no es válido.";
            var errorCredenciales = FND_TraspasoTesoreria_Proceso_ValidarCredencialesAccion(request, accion);
            if (errorCredenciales != null)
                return errorCredenciales;

            if (string.IsNullOrWhiteSpace(request.AppProductName))
                return "El nombre de la aplicación es requerido.";

            return FND_TraspasoTesoreria_Proceso_ValidarSeleccion(request, modo);
        }

        /// <summary>
        /// Valida las credenciales requeridas según la acción: token para desembolsar (D),
        /// código de retención para retener (R).
        /// </summary>
        private static string? FND_TraspasoTesoreria_Proceso_ValidarCredencialesAccion(
            FndTraspasoTesoreriaProcesoIniciarRequest request,
            string accion)
        {
            if (accion == "D" && string.IsNullOrWhiteSpace(request.Token))
                return "El token es requerido para desembolsar.";
            if (accion == "R" && string.IsNullOrWhiteSpace(request.RetencionCodigo))
                return "El código de retención es requerido.";
            return null;
        }

        private static string? FND_TraspasoTesoreria_Proceso_ValidarSeleccion(
            FndTraspasoTesoreriaProcesoIniciarRequest request,
            string modo)
        {
            if (modo == "G" && !request.Consecutivos.Any(consec => consec > 0))
                return "Debe seleccionar al menos una liquidación.";
            if (modo == "G" && request.Consecutivos.Any(consec => consec <= 0))
                return "Los consecutivos deben ser mayores que cero.";
            if (modo == "U" && !request.Cedulas.Any(cedula => !string.IsNullOrWhiteSpace(cedula)))
                return "Debe seleccionar al menos una cédula.";
            if (request.Cedulas.Any(cedula =>
                    !string.IsNullOrWhiteSpace(cedula) && cedula.Trim().Length > 20))
                return "Existe una cédula con una longitud no válida.";

            return null;
        }

        private static string? FND_TraspasoTesoreria_Proceso_ValidarContinuacion(
            FndTraspasoTesoreriaProcesoContinuarRequest request)
        {
            if (request is null) return "La solicitud es requerida.";
            if (request.ProcesoId == Guid.Empty) return "El identificador del proceso es requerido.";
            if (string.IsNullOrWhiteSpace(request.Usuario)) return "El usuario es requerido.";
            return null;
        }

        private static string FND_TraspasoTesoreria_Proceso_CrearSeleccionXml(
            FndTraspasoTesoreriaProcesoIniciarRequest request,
            string modo)
        {
            IEnumerable<XElement> elementos = modo == "G"
                ? request.Consecutivos
                    .Where(consec => consec > 0)
                    .Distinct()
                    .Order()
                    .Select(consec => new XElement("item", new XElement("consec", consec)))
                : request.Cedulas
                    .Where(cedula => !string.IsNullOrWhiteSpace(cedula))
                    .Select(cedula => cedula.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .Select(cedula => new XElement("item", new XElement("cedula", cedula)));

            return new XElement("seleccion", elementos).ToString(SaveOptions.DisableFormatting);
        }

        private static string FND_TraspasoTesoreria_Proceso_CrearFiltro(string? valor)
        {
            string filtro = valor?.Trim() ?? string.Empty;
            return string.IsNullOrEmpty(filtro) ? "%" : $"{filtro.TrimEnd('%')}%";
        }

        private static string FND_TraspasoTesoreria_Proceso_CalcularHash(
            FndTraspasoTesoreriaProcesoIniciarRequest request,
            string modo,
            string accion)
        {
            object seleccion = modo == "G"
                ? (object)request.Consecutivos.Where(x => x > 0).Distinct().Order().ToArray()
                : request.Cedulas
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim().ToUpperInvariant())
                    .Distinct()
                    .Order()
                    .ToArray();

            var contenido = new
            {
                Modo = modo,
                Accion = accion,
                Token = request.Token?.Trim(),
                RetencionCodigo = request.RetencionCodigo?.Trim(),
                FechaDesde = request.FechaDesde?.Date,
                FechaHasta = request.FechaHasta?.Date,
                request.AplicaRevision,
                request.BancoId,
                Oficina = request.Oficina?.Trim(),
                UsuarioFiltro = request.UsuarioFiltro?.Trim(),
                SistemaFiltro = request.SistemaFiltro?.Trim(),
                TokenFiltro = request.TokenFiltro?.Trim(),
                AppProductName = request.AppProductName.Trim(),
                Seleccion = seleccion
            };

            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(contenido));
            return Convert.ToHexString(SHA256.HashData(bytes));
        }

        private static FndTraspasoTesoreriaProcesoResult FND_TraspasoTesoreria_Proceso_Consultar(
            SqlConnection conn,
            Guid procesoId,
            string usuario)
        {
            const string sqlProceso = @"
                SELECT
                    P.PROCESO_ID AS ProcesoId,
                    P.ESTADO AS Estado,
                    P.USUARIO AS UsuarioOrigen,
                    P.TOTAL_REGISTROS AS TotalRegistros,
                    P.PROCESADOS AS Procesados,
                    P.CON_ERRORES AS ConErrores,
                    ISNULL(SUM(CASE WHEN D.ESTADO IN ('P', 'T') THEN 1 ELSE 0 END), 0) AS Pendientes,
                    CAST(CASE WHEN P.TOTAL_REGISTROS = 0 THEN 0
                        ELSE (P.PROCESADOS + P.CON_ERRORES) * 100.0 / P.TOTAL_REGISTROS
                    END AS DECIMAL(6, 2)) AS Porcentaje,
                    P.ERROR_MENSAJE AS ErrorMensaje
                FROM dbo.FND_TRASPASO_TES_PROCESO P
                LEFT JOIN dbo.FND_TRASPASO_TES_PROCESO_DET D
                    ON D.PROCESO_ID = P.PROCESO_ID
                WHERE P.PROCESO_ID = @ProcesoId
                  AND P.USUARIO = @Usuario
                GROUP BY P.PROCESO_ID, P.ESTADO, P.USUARIO, P.TOTAL_REGISTROS,
                    P.PROCESADOS, P.CON_ERRORES, P.ERROR_MENSAJE;";

            var resultado = conn.QuerySingleOrDefault<FndTraspasoTesoreriaProcesoResult>(
                sqlProceso,
                new { ProcesoId = procesoId, Usuario = usuario });

            if (resultado is null)
                throw new InvalidOperationException("No se encontró el proceso solicitado.");

            resultado.ProcesoFinalizado = resultado.Estado is "C" or "E";
            if (!resultado.ProcesoFinalizado)
                return resultado;

            const string sqlErrores = @"
                SELECT
                    D.CONSEC AS Consec,
                    CASE
                        WHEN E.ES_ERROR_GENERACION = 1
                             AND NULLIF(LTRIM(RTRIM(L.RETENCION_CODIGO)), '') IS NOT NULL
                            THEN 'La liquidación tiene un código de retención activo.'
                        WHEN E.ES_ERROR_GENERACION = 1
                             AND M.MONTO_GIRAR <= 0
                            THEN 'El monto a girar debe ser mayor que cero.'
                        ELSE E.ERROR_ORIGINAL
                    END AS Descripcion
                FROM dbo.FND_TRASPASO_TES_PROCESO_DET D
                LEFT JOIN dbo.FND_LIQUIDACION L
                    ON L.CONSEC = D.CONSEC
                CROSS APPLY (
                    SELECT CASE
                        WHEN L.TOTAL_GIRAR IS NULL THEN
                            ISNULL(L.APORTES_LIQ, 0)
                            + ISNULL(L.RENDI_LIQ, 0)
                            - ISNULL(L.MULTA_RETIRO, 0)
                            - ISNULL(L.ISR_MONTO, 0)
                            - ISNULL(L.OTROS_REBAJOS, 0)
                        ELSE L.TOTAL_GIRAR
                    END AS MONTO_GIRAR
                ) M
                CROSS APPLY (
                    SELECT
                        COALESCE(
                            NULLIF(LTRIM(RTRIM(D.ERROR_MENSAJE)), ''),
                            'Error no especificado'
                        ) AS ERROR_ORIGINAL,
                        CASE WHEN D.ERROR_MENSAJE = 'No se generó la solicitud de Tesorería.'
                            THEN 1 ELSE 0 END AS ES_ERROR_GENERACION
                ) E
                WHERE D.PROCESO_ID = @ProcesoId
                  AND D.ESTADO = 'E'
                ORDER BY D.CONSEC;";

            resultado.Errores = conn.Query<FndTraspasoTesoreriaProcesoErrorResult>(
                    sqlErrores,
                    new { ProcesoId = procesoId })
                .ToList();
            return resultado;
        }

        /// <summary>
        /// Obtiene el proceso no terminal asociado al mismo hash de solicitud.
        /// </summary>
        private static FndTraspasoTesoreriaProcesoActivo?
            FND_TraspasoTesoreria_Proceso_Activo_Equivalente_Obtener(
                SqlConnection conn,
                string solicitudHash)
        {
            const string sql = @"
                SELECT TOP (1)
                    PROCESO_ID AS ProcesoId,
                    USUARIO AS Usuario
                FROM dbo.FND_TRASPASO_TES_PROCESO
                WHERE SOLICITUD_HASH = @SolicitudHash
                  AND ESTADO NOT IN ('C', 'E')
                ORDER BY PROCESO_ID;";

            return conn.QuerySingleOrDefault<FndTraspasoTesoreriaProcesoActivo>(
                sql,
                new { SolicitudHash = solicitudHash });
        }

        /// <summary>
        /// Obtiene el proceso pendiente que contiene alguna liquidación de la selección.
        /// </summary>
        private static FndTraspasoTesoreriaProcesoActivo?
            FND_TraspasoTesoreria_Proceso_Activo_Seleccion_Obtener(
                SqlConnection conn,
                string seleccionXml,
                string modo)
        {
            const string sql = @"
                DECLARE @Seleccion XML = TRY_CAST(@SeleccionXml AS XML);

                IF @Modo = 'G'
                BEGIN
                    SELECT TOP (1)
                        P.PROCESO_ID AS ProcesoId,
                        P.USUARIO AS Usuario,
                        P.ESTADO AS Estado
                    FROM dbo.FND_TRASPASO_TES_PROCESO P
                    INNER JOIN dbo.FND_TRASPASO_TES_PROCESO_DET D
                        ON D.PROCESO_ID = P.PROCESO_ID
                    INNER JOIN @Seleccion.nodes('/seleccion/item') X(Item)
                        ON D.CONSEC = X.Item.value('(consec/text())[1]', 'INT')
                    WHERE P.ESTADO NOT IN ('C', 'E')
                    GROUP BY P.PROCESO_ID, P.USUARIO, P.ESTADO
                    ORDER BY COUNT_BIG(*) DESC, P.PROCESO_ID;
                END
                ELSE
                BEGIN
                    SELECT TOP (1)
                        P.PROCESO_ID AS ProcesoId,
                        P.USUARIO AS Usuario,
                        P.ESTADO AS Estado
                    FROM dbo.FND_TRASPASO_TES_PROCESO P
                    INNER JOIN dbo.FND_TRASPASO_TES_PROCESO_DET D
                        ON D.PROCESO_ID = P.PROCESO_ID
                    INNER JOIN dbo.FND_LIQUIDACION L
                        ON L.CONSEC = D.CONSEC
                    INNER JOIN dbo.FND_CONTRATOS C
                        ON C.COD_OPERADORA = L.COD_OPERADORA
                       AND C.COD_PLAN = L.COD_PLAN
                       AND C.COD_CONTRATO = L.COD_CONTRATO
                    INNER JOIN @Seleccion.nodes('/seleccion/item') X(Item)
                        ON LTRIM(RTRIM(C.CEDULA)) =
                           LTRIM(RTRIM(X.Item.value(
                               '(cedula/text())[1]',
                               'VARCHAR(20)')))
                    WHERE P.ESTADO NOT IN ('C', 'E')
                    GROUP BY P.PROCESO_ID, P.USUARIO, P.ESTADO
                    ORDER BY COUNT_BIG(*) DESC, P.PROCESO_ID;
                END";

            return conn.QuerySingleOrDefault<FndTraspasoTesoreriaProcesoActivo>(
                sql,
                new
                {
                    SeleccionXml = seleccionXml,
                    Modo = modo
                });
        }

        private static bool FND_TraspasoTesoreria_Proceso_EsConflictoPendiente(
            string mensaje)
        {
            return mensaje.Contains(
                "Una liquidación pertenece a otro proceso pendiente",
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Serializa los inicios equivalentes mientras la conexión permanezca abierta.
        /// </summary>
        private static string FND_TraspasoTesoreria_Proceso_Bloqueo_Adquirir(
            SqlConnection conn,
            string solicitudHash)
        {
            const string sql = @"
                DECLARE @Resultado INT;
                EXEC @Resultado = sys.sp_getapplock
                    @Resource = @Recurso,
                    @LockMode = 'Exclusive',
                    @LockOwner = 'Session',
                    @LockTimeout = 10000;
                SELECT @Resultado;";
            string recurso = $"FND_TRASPASO_TES:{solicitudHash}";
            int resultado = conn.ExecuteScalar<int>(sql, new { Recurso = recurso });

            if (resultado < 0)
            {
                throw new InvalidOperationException(
                    "No fue posible reservar el inicio del proceso.");
            }

            return recurso;
        }

        /// <summary>
        /// Libera el bloqueo de sesión antes de devolver la conexión al pool.
        /// </summary>
        private static void FND_TraspasoTesoreria_Proceso_Bloqueo_Liberar(
            SqlConnection conn,
            string recurso)
        {
            const string sql = @"
                EXEC sys.sp_releaseapplock
                    @Resource = @Recurso,
                    @LockOwner = 'Session';";
            conn.Execute(sql, new { Recurso = recurso });
        }

        /// <summary>
        /// Obtiene el propietario y estado actual del proceso persistente.
        /// </summary>
        private static FndTraspasoTesoreriaProcesoActivo
            FND_TraspasoTesoreria_Proceso_Contexto_Obtener(
            SqlConnection conn,
            Guid procesoId)
        {
            const string sql = @"
                SELECT
                    PROCESO_ID AS ProcesoId,
                    USUARIO AS Usuario,
                    ESTADO AS Estado
                FROM dbo.FND_TRASPASO_TES_PROCESO
                WHERE PROCESO_ID = @ProcesoId;";

            return conn.QuerySingleOrDefault<FndTraspasoTesoreriaProcesoActivo>(
                    sql,
                    new { ProcesoId = procesoId })
                ?? throw new InvalidOperationException(
                    "No se encontró el proceso solicitado.");
        }

        private static ErrorDto<FndTraspasoTesoreriaProcesoResult>
            FND_TraspasoTesoreria_Proceso_CrearError(Exception ex, string operacion)
        {
            Trace.TraceError(
                "FND_TraspasoTesoreria_Proceso_{0}: {1}",
                operacion,
                ex);

            string mensaje = ex is SqlException
                ? ex.Message
                : "No fue posible procesar el traspaso de tesorería.";

            return DbHelper.CreateErrorResponse<FndTraspasoTesoreriaProcesoResult>(mensaje);
        }

        private sealed class FndTraspasoTesoreriaProcesoActivo
        {
            public Guid ProcesoId { get; init; } = Guid.Empty;
            public string Usuario { get; init; } = string.Empty;
            public string Estado { get; init; } = string.Empty;
        }
    }
}
