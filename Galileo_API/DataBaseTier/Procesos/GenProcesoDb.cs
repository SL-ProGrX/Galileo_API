using Galileo.Models.ERROR;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Dapper;
using Galileo.DataBaseTier;
using Galileo_API.Models.Procesos;
using System.Data;

namespace Galileo_API.DataBaseTier.Procesos
{
    /// <summary>
    /// Acceso a la tabla general de procesos (GEN_PROCESO) para registrar avance y tiempos.
    /// </summary>
    public sealed class GenProcesoDb
    {
        private const int MensajeMaxLength = 4000;
        private readonly PortalDB _portalDb;

        public GenProcesoDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        public void Iniciar(int codEmpresa, Guid procesoId, string tipoProceso, string usuario, int totalItems)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);

            connection.Execute(
                "spGEN_Proceso_Iniciar",
                new
                {
                    ProcesoId = procesoId,
                    CodEmpresa = codEmpresa,
                    TipoProceso = tipoProceso,
                    Usuario = usuario,
                    TotalItems = totalItems
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        public void Actualizar(int codEmpresa, Guid procesoId, string estado, int itemsProcesados, int itemsConError, string? mensaje = null)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);

            connection.Execute(
                "spGEN_Proceso_Actualizar",
                new
                {
                    ProcesoId = procesoId,
                    Estado = estado,
                    ItemsProcesados = itemsProcesados,
                    ItemsConError = itemsConError,
                    Mensaje = Truncar(mensaje)
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        public GenProcesoEstadoResult? Estado_Obtener(int codEmpresa, Guid procesoId)
        {
            using var connection = _portalDb.CreateConnection(codEmpresa);

            return connection.QueryFirstOrDefault<GenProcesoEstadoResult>(
                "spGEN_Proceso_Estado",
                new { ProcesoId = procesoId, CodEmpresa = codEmpresa },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0);
        }

        private static string? Truncar(string? mensaje) =>
            mensaje is null || mensaje.Length <= MensajeMaxLength
                ? mensaje
                : mensaje[..MensajeMaxLength];

        /// <summary>Registra el avance de Cobros con el formato propio del proceso.</summary>
        internal void CO_Aplicacion_Avance_Actualizar(
            int codEmpresa, GenProcesoEstadoResult avance, int aplicacionId,
            bool contratosExcedentes = false)
        {
            var encabezado = contratosExcedentes
                ? $"Aplicación {aplicacionId}. "
                : $"Aplicación: {aplicacionId}. ";
            Actualizar(codEmpresa, avance.ProcesoId, GenProcesoEstados.EnProceso,
                avance.ItemsProcesados, avance.ItemsConError,
                encabezado +
                $"Procesados: {avance.ItemsProcesados} de {avance.TotalItems}. " +
                $"Correctos: {avance.ItemsProcesados - avance.ItemsConError}. " +
                $"Con error: {avance.ItemsConError}.");
        }
        private static readonly MemoryCache CoResultadosTemporales =
            new(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1)
            });

        /// <summary>Datos internos para publicar el resultado de una aplicación de Cobros.</summary>
        internal sealed class CoAplicacionFinalizacion<TEstado, TResultado>
            where TEstado : GenProcesoEstadoResult
        {
            public required TEstado Estado { get; init; }
            public int AplicacionId { get; init; }
            public required TResultado Resultado { get; init; }
            public required Func<Exception, string> DescribirError { get; init; }
        }

        /// <summary>Conserva una copia por tipo de proceso, empresa e identificador durante dos horas.</summary>
        private static void CO_Aplicacion_Resultado_Guardar<TEstado>(
            int codEmpresa, TEstado estado) where TEstado : GenProcesoEstadoResult
        {
            CoResultadosTemporales.Set(
                (typeof(TEstado), codEmpresa, estado.ProcesoId),
                JsonSerializer.Serialize(estado),
                TimeSpan.FromHours(2));
        }

        /// <summary>Publica contadores y detalle sin perderlos si falla el registro del estado final.</summary>
        internal ErrorDto<TResultado> CO_Aplicacion_Finalizar<TEstado, TResultado>(
            int codEmpresa, CoAplicacionFinalizacion<TEstado, TResultado> solicitud)
            where TEstado : GenProcesoEstadoResult
        {
            var estado = solicitud.Estado;
            var aplicacion = solicitud.AplicacionId > 0
                ? solicitud.AplicacionId.ToString()
                : "sin identificador confirmado";
            var resumen =
                $"Aplicación: {aplicacion}. " +
                $"Correctos: {estado.ItemsProcesados - estado.ItemsConError}. " +
                $"Con error: {estado.ItemsConError}. " +
                $"Sin intentar: {estado.TotalItems - estado.ItemsProcesados}. " +
                $"Procesados: {estado.ItemsProcesados} de {estado.TotalItems}.";
            estado.Mensaje = string.IsNullOrWhiteSpace(estado.Mensaje)
                ? resumen
                : $"{estado.Mensaje} {resumen}";

            CO_Aplicacion_Resultado_Guardar(codEmpresa, estado);
            try
            {
                Actualizar(codEmpresa, estado.ProcesoId, estado.Estado,
                    estado.ItemsProcesados, estado.ItemsConError, estado.Mensaje);
            }
            catch (Exception ex)
            {
                estado.Mensaje +=
                    " No fue posible registrar el estado final en GEN_PROCESO. " +
                    solicitud.DescribirError(ex);
                CO_Aplicacion_Resultado_Guardar(codEmpresa, estado);
            }

            return estado.Estado == GenProcesoEstados.Terminado
                ? DbHelper.CreateOkResponse(solicitud.Resultado, estado.Mensaje ?? string.Empty)
                : DbHelper.CreateErrorResponse(estado.Mensaje ?? string.Empty, -1, solicitud.Resultado);
        }

        /// <summary>Consulta el detalle temporal o los contadores persistidos, conservando el tipo público.</summary>
        internal ErrorDto<TEstado> CO_Aplicacion_Estado<TEstado>(
            int codEmpresa, Guid procesoId, Func<Exception, string> describirError)
            where TEstado : GenProcesoEstadoResult, new()
        {
            try
            {
                if (CoResultadosTemporales.TryGetValue<string>(
                    (typeof(TEstado), codEmpresa, procesoId), out var json) &&
                    !string.IsNullOrWhiteSpace(json))
                {
                    var resultado = JsonSerializer.Deserialize<TEstado>(json);
                    if (resultado is not null)
                    {
                        return DbHelper.CreateOkResponse(resultado);
                    }
                }

                var estado = Estado_Obtener(codEmpresa, procesoId);
                if (estado is null)
                {
                    return DbHelper.CreateErrorResponse("Proceso no encontrado.", -1, new TEstado());
                }

                return DbHelper.CreateOkResponse(new TEstado
                {
                    ProcesoId = estado.ProcesoId,
                    Estado = estado.Estado,
                    TotalItems = estado.TotalItems,
                    ItemsProcesados = estado.ItemsProcesados,
                    ItemsConError = estado.ItemsConError,
                    Mensaje = estado.Mensaje,
                    FechaInicio = estado.FechaInicio,
                    FechaFin = estado.FechaFin
                });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible consultar el avance. " + describirError(ex), -1, new TEstado());
            }
        }

        /// <summary>Coordinador interno compartido por aplicaciones con el mismo contrato SQL.</summary>
        internal sealed class CoAplicacion<TItem, TEstado>
            where TItem : class
            where TEstado : GenProcesoEstadoResult, new()
        {
            private readonly PortalDB _portalDB;
            private readonly GenProcesoDb _procesoDb;
            private readonly Opciones opciones;

            /// <summary>Consultas y adaptadores definidos por cada DB, nunca por el request.</summary>
            internal sealed class Opciones
            {
                public required string GuiaSql { get; init; }
                public required string CasoSql { get; init; }
                public required string ValidacionSql { get; init; }
                public required string MensajeInicio { get; init; }
                public int CedulaMaxima { get; init; }
                public bool VerificarEtapaGuia { get; init; }
                public required Func<TItem?, string?> Cedula { get; init; }
                public required Func<string, Exception> CrearExcepcion { get; init; }
                public required Func<Exception, bool> EsErrorResultado { get; init; }
                public required Func<(int Numero, string Cedula, TItem? Item, string Mensaje), object> CrearError { get; init; }
                public required Action<TEstado, List<object>> AsignarDetalle { get; init; }
                public required Func<int, int, object> CrearResultado { get; init; }
            }

            /// <summary>Agrupa la selección y el usuario sin cambiar los requests públicos.</summary>
            internal sealed class Solicitud
            {
                public List<TItem>? Seleccionados { get; init; }
                public string? Usuario { get; init; }
            }

            /// <summary>Reutiliza el portal y el registro de procesos del formulario.</summary>
            internal CoAplicacion(PortalDB portalDB, GenProcesoDb procesoDb, Opciones configuracion)
            {
                _portalDB = portalDB;
                _procesoDb = procesoDb;
                opciones = configuracion;
            }
            private sealed class Ejecucion
            {
                public Guid ProcesoId { get; init; }
                public string Usuario { get; init; } = string.Empty;
                public int AplicacionId { get; set; }
                public int Total { get; init; }
                public int Procesados { get; set; }
                public string Etapa { get; set; } = "Validar selección";

                public List<object> Errores { get; } = new();

                public int Correctos => Procesados - Errores.Count;
            }

            private sealed class GuiaResultado
            {
                public int? Aplicacion { get; set; }
            }

            private sealed class CasoResultado
            {
                public string? Resultado { get; set; }
            }

            /// <summary>
            /// Ejecuta todos los casos posibles, publica el avance y acumula
            /// los motivos de error individuales.
            /// </summary>
            public ErrorDto<object>
                CO_Aplicacion_Ejecutar(
                    int codEmpresa,
                    Guid procesoId,
                    Solicitud request)
            {
                var items = request.Seleccionados ??
                    new List<TItem>();

                var ejecucion = new Ejecucion
                {
                    ProcesoId = procesoId,
                    Usuario = (request.Usuario ?? string.Empty)
                        .Trim()
                        .ToUpperInvariant(),
                    Total = items.Count
                };

                var estado = new TEstado
                {
                    Estado = GenProcesoEstados.Terminado
                };

                try
                {
                    if (items.Count == 0)
                    {
                        throw opciones.CrearExcepcion("No hay casos a procesar.");
                    }

                    if (ejecucion.Usuario.Length == 0 ||
                        ejecucion.Usuario.Length > 30)
                    {
                        throw opciones.CrearExcepcion(
                            "El usuario es obligatorio y admite hasta 30 caracteres.");
                    }

                    ejecucion.Etapa = "Registrar inicio en GEN_PROCESO";

                    _procesoDb.Actualizar(
                        codEmpresa,
                        procesoId,
                        GenProcesoEstados.EnProceso,
                        0,
                        0,
                        opciones.MensajeInicio);

                    ejecucion.AplicacionId =
                        CO_Aplicacion_Guia_Crear(codEmpresa, ejecucion);

                    foreach (var item in items)
                    {
                        ejecucion.Etapa =
                            $"Ejecutar caso {ejecucion.Procesados + 1}";

                        var error = CO_Aplicacion_Caso_Ejecutar(
                            codEmpresa,
                            ejecucion,
                            item);

                        ejecucion.Procesados++;

                        if (error is not null)
                        {
                            ejecucion.Errores.Add(error);
                        }

                        ejecucion.Etapa = "Registrar avance en GEN_PROCESO";

                        _procesoDb.CO_Aplicacion_Avance_Actualizar(codEmpresa,
                        new GenProcesoEstadoResult
                        {
                            ProcesoId = procesoId,
                            ItemsProcesados = ejecucion.Procesados,
                            ItemsConError = ejecucion.Errores.Count,
                            TotalItems = ejecucion.Total
                        }, ejecucion.AplicacionId, false);
                    }
                }
                catch (Exception ex) when (
                    ex is ErrorHandledException ||
                    ex is SqlException ||
                    ex is TimeoutException ||
                    ex is JsonException ||
                    ex is InvalidOperationException ||
                    ex is DataException)
                {
                    // La falla general conserva los casos ya intentados.
                    estado.Estado = GenProcesoEstados.Error;
                    estado.Mensaje =
                        $"Etapa: {ejecucion.Etapa}. " +
                        CO_Aplicacion_Error_Describir(ex);
                }

                return CO_Aplicacion_Finalizar(
                    codEmpresa,
                    ejecucion,
                    estado);
            }

            /// <summary>
            /// Crea la guía usando @pUsuario y lee Aplicacion.
            /// </summary>
            private int CO_Aplicacion_Guia_Crear(
                int codEmpresa,
                Ejecucion ejecucion)
            {
                ejecucion.Etapa = "Abrir conexión para crear la guía";

                using var connection =
                    DbHelper.OpenConnection(_portalDB, codEmpresa);

                connection.Open();

                ejecucion.Etapa =
                    "Ejecutar " + opciones.GuiaSql[4..];

                var filas = connection.Query<GuiaResultado>(
                    opciones.GuiaSql,
                    new { pUsuario = ejecucion.Usuario },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                ).ToList();

                if (opciones.VerificarEtapaGuia)
                {
                    ejecucion.Etapa = "Verificar transacción de la guía";
                }
                CO_Aplicacion_Transaccion_Verificar(connection);

                if (filas.Count != 1 || filas[0].Aplicacion is not int aplicacion || aplicacion <= 0)
                {
                    throw opciones.CrearExcepcion(
                        "La guía no devolvió una única Aplicacion positiva.");
                }

                return aplicacion;
            }

            /// <summary>
            /// Ejecuta un caso en su propia conexión, lee Resultado y
            /// confirma el registro mediante la función existente.
            /// </summary>
            private object?
                CO_Aplicacion_Caso_Ejecutar(
                    int codEmpresa,
                    Ejecucion ejecucion,
                    TItem? item)
            {
                var cedula = opciones.Cedula(item)?.Trim() ?? string.Empty;
                var etapa = "Validar identificación";
                string mensaje;

                try
                {
                    if (cedula.Length == 0 || cedula.Length > opciones.CedulaMaxima)
                    {
                        throw opciones.CrearExcepcion(
                            $"La identificación es obligatoria y admite hasta {opciones.CedulaMaxima} caracteres.");
                    }

                    etapa = "Abrir conexión del caso";

                    using var connection =
                        DbHelper.OpenConnection(_portalDB, codEmpresa);

                    connection.Open();

                    etapa =
                        "Ejecutar " + opciones.CasoSql[4..];

                    var filas = connection.Query<CasoResultado>(
                        opciones.CasoSql,
                        new
                        {
                            pUsuario = ejecucion.Usuario,
                            pCedula = cedula,
                            pIdAplicacion = ejecucion.AplicacionId
                        },
                        commandType: CommandType.StoredProcedure,
                        commandTimeout: 0
                    ).ToList();

                    CO_Aplicacion_Transaccion_Verificar(connection);

                    etapa = "Leer Resultado del procedimiento";

                    if (filas.Count != 1)
                    {
                        throw opciones.CrearExcepcion(
                            $"El procedimiento devolvió {filas.Count} filas; " +
                            "se esperaba una fila con Resultado.");
                    }

                    var motivo = filas[0].Resultado?.Trim();

                    if (!string.IsNullOrWhiteSpace(motivo))
                    {
                        mensaje =
                            $"El procedimiento informó: {motivo} " +
                            "Revise los movimientos antes de reintentar.";
                    }
                    else
                    {
                        etapa = "Confirmar aplicación registrada";
                        var valido = connection.QuerySingle<bool?>(
                            opciones.ValidacionSql,
                            new
                            {
                                Cedula = cedula,
                                Aplicacion = ejecucion.AplicacionId
                            },
                            commandTimeout: 0);

                        if (valido == true)
                        {
                            return null;
                        }

                        mensaje =
                            "El procedimiento no informó un motivo, pero la función " +
                            "de validación no confirmó una aplicación procesada. " +
                            "Revise los movimientos antes de reintentar.";
                    }
                }
                catch (Exception ex)
                {
                    mensaje =
                        $"Etapa: {etapa}. " +
                        CO_Aplicacion_Error_Describir(ex) +
                        " Revise los movimientos antes de reintentar.";
                }

                return opciones.CrearError((ejecucion.Procesados + 1, cedula, item, mensaje));
            }

            /// <summary>
            /// Detecta transacciones que el SP dejó abiertas en una conexión
            /// que el API abrió sin transacción propia.
            /// </summary>
            private void CO_Aplicacion_Transaccion_Verificar(
                IDbConnection connection)
            {
                var abiertas = connection.QuerySingle<int>(
                    "SELECT @@TRANCOUNT;",
                    commandTimeout: 30);

                if (abiertas != 0)
                {
                    throw opciones.CrearExcepcion(
                        $"El procedimiento dejó {abiertas} transacciones abiertas. " +
                        "El caso no se considera confirmado.");
                }
            }

            /// <summary>
            /// Describe fallos técnicos sin devolver conexiones ni credenciales.
            /// </summary>
            private string CO_Aplicacion_Error_Describir(
                Exception exception)
            {
                var error = exception.GetBaseException();
                var sql = exception as SqlException ?? error as SqlException;

                if (sql is not null)
                {
                    return string.Join(
                        " | ",
                        sql.Errors.Cast<SqlError>()
                            .Select(item =>
                                $"Error SQL {item.Number}, estado {item.State}, " +
                                $"procedimiento {item.Procedure}, línea {item.LineNumber}.")
                            .Distinct());
                }

                if (opciones.EsErrorResultado(error))
                {
                    return error.Message;
                }

                return
                    $"No fue posible completar la operación. " +
                    $"Tipo: {error.GetType().Name}.";
            }

            /// <summary>
            /// Conserva el detalle temporal y registra los contadores finales.
            /// </summary>
            private ErrorDto<object>
                CO_Aplicacion_Finalizar(
                    int codEmpresa,
                    Ejecucion ejecucion,
                    TEstado estado)
            {
                estado.ProcesoId = ejecucion.ProcesoId;
                estado.TotalItems = ejecucion.Total;
                estado.ItemsProcesados = ejecucion.Procesados;
                estado.ItemsConError = ejecucion.Errores.Count;
                opciones.AsignarDetalle(estado, ejecucion.Errores.ToList());


                return _procesoDb.CO_Aplicacion_Finalizar(codEmpresa,
                    new CoAplicacionFinalizacion<TEstado, object>
                    {
                        Estado = estado,
                        AplicacionId = ejecucion.AplicacionId,
                        Resultado = opciones.CrearResultado(ejecucion.Correctos,
                            ejecucion.Total - ejecucion.Correctos),
                        DescribirError = CO_Aplicacion_Error_Describir
                    });
            }

            /// <summary>Consulta el avance o el detalle temporal de esta aplicación.</summary>
            internal ErrorDto<TEstado> CO_Aplicacion_Estado(int codEmpresa, Guid procesoId)
                => _procesoDb.CO_Aplicacion_Estado<TEstado>(
                    codEmpresa, procesoId, CO_Aplicacion_Error_Describir);

        }
        /// <summary>Datos y adaptadores del formulario para validar e iniciar una aplicación de Cobros.</summary>
        internal sealed class CoAplicacionInicio<TItem> where TItem : class
        {
            public string? Usuario { get; init; }
            public List<TItem>? Seleccionados { get; init; }
            public required Func<TItem, string?> ObtenerCedula { get; init; }
            public int CedulaMaxima { get; init; }
            public required string TipoProceso { get; init; }
            public bool ContratosExcedentes { get; init; }
            public required Action<string> AsignarUsuario { get; init; }
            public required Func<Guid, bool> Encolar { get; init; }
        }

        /// <summary>Valida la selección, registra el proceso y lo entrega a su cola original.</summary>
        internal ErrorDto<GenProcesoIniciadoResult> CO_Aplicacion_Iniciar<TItem>(
            int codEmpresa, CoAplicacionInicio<TItem> solicitud) where TItem : class
        {
            var usuario = (solicitud.Usuario ?? string.Empty).Trim().ToUpperInvariant();
            var error = CO_Aplicacion_Inicio_Validar(solicitud, usuario);
            if (error is not null)
            {
                return DbHelper.CreateErrorResponse(error, -1, new GenProcesoIniciadoResult());
            }

            solicitud.AsignarUsuario(usuario);
            var totalCasos = solicitud.Seleccionados!.Count;
            var procesoId = Guid.NewGuid();
            try
            {
                Iniciar(codEmpresa, procesoId, solicitud.TipoProceso, usuario, totalCasos);
                if (!solicitud.Encolar(procesoId))
                {
                    const string mensaje = "No fue posible encolar el proceso. Intente nuevamente.";
                    Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, mensaje);
                    return DbHelper.CreateErrorResponse(mensaje, -1, new GenProcesoIniciadoResult());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new GenProcesoIniciadoResult());
            }

            return DbHelper.CreateOkResponse(new GenProcesoIniciadoResult { ProcesoId = procesoId });
        }

        /// <summary>Conserva el orden de validación, los límites y los mensajes de cada formulario.</summary>
        private static string? CO_Aplicacion_Inicio_Validar<TItem>(
            CoAplicacionInicio<TItem> solicitud, string usuario) where TItem : class
        {
            if (usuario.Length == 0 || usuario.Length > 30)
            {
                return solicitud.ContratosExcedentes
                    ? "El usuario es obligatorio y admite un máximo de 30 caracteres."
                    : "El usuario es obligatorio y admite hasta 30 caracteres.";
            }

            var seleccionados = solicitud.Seleccionados;
            if (seleccionados is null || seleccionados.Count == 0)
            {
                return "No hay casos a procesar.";
            }

            var identificaciones = seleccionados
                .Select(item => item is null ? null : solicitud.ObtenerCedula(item)?.Trim())
                .ToList();
            if (identificaciones.Any(cedula =>
                string.IsNullOrWhiteSpace(cedula) || cedula.Length > solicitud.CedulaMaxima))
            {
                return solicitud.ContratosExcedentes
                    ? "La selección contiene casos sin identificación válida. " +
                      "La identificación admite un máximo de 15 caracteres."
                    : "La selección contiene identificaciones inválidas.";
            }

            if (identificaciones.Distinct(StringComparer.OrdinalIgnoreCase).Count() != identificaciones.Count)
            {
                return solicitud.ContratosExcedentes
                    ? "La selección contiene identificaciones repetidas. " +
                      "Recargue los acuerdos y vuelva a seleccionar los casos."
                    : "La selección contiene identificaciones repetidas.";
            }

            return null;
        }
    }
}
