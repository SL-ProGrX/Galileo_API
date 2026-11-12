using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using System.Data;
using System.Text.Json;

namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplExcContratosAplicacionDb : CoProcesoDbBase
    {
        private static readonly MemoryCache ResultadosTemporales =
            new(new MemoryCacheOptions
            {
                ExpirationScanFrequency = TimeSpan.FromMinutes(1)
            });

        private sealed class CoAplExcContrEjecucion
        {
            public Guid ProcesoId { get; init; }
            public string Usuario { get; init; } = string.Empty;
            public int AplicacionId { get; set; }
            public int Total { get; init; }
            public int Procesados { get; set; }
            public string Etapa { get; set; } = "Validar la selección";

            public List<CoAplExcContrProcesoErrorDto> Errores { get; } = new();

            public int Correctos => Procesados - Errores.Count;
            public int SinIntentar => Total - Procesados;
        }

        private sealed class CoAplExcContrGuiaResultado
        {
            public int? aplicacion { get; set; }
        }

        private sealed class CoAplExcContrCasoResultado
        {
            public string? resultado { get; set; }
        }

        private sealed class CoAplExcContrResultadoException : Exception
        {
            public CoAplExcContrResultadoException(string mensaje)
                : base(mensaje)
            {
            }
        }

        public FrmCoAplExcContratosAplicacionDb(IConfiguration config)
            : base(config)
        {
        }

        public FrmCoAplExcContratosAplicacionDb(PortalDB portalDB)
            : base(portalDB)
        {
        }

        /// <summary>
        /// Obtiene los contratos disponibles para aplicar excedentes.
        /// </summary>
        public ErrorDto<List<CoAplExcContrAplInformacionData>>
            CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {
            return DbHelper.ExecuteListQuery<CoAplExcContrAplInformacionData>(
                _portalDB,
                codEmpresa,
                "exec dbo.spCBR_Excedente_Apl_Contratos_Proceso_Carga_Informacion",
                commandTimeout: 0);
        }

        /// <summary>
        /// Aplica los casos seleccionados.
        /// Acumula errores individuales y continúa con el siguiente caso.
        /// Conserva el resultado final temporalmente y publica el avance
        /// mediante GenProcesoDb.
        /// </summary>
        public ErrorDto<CoAplExcContrAplicadosResult>
            CO_AplExcContrApl_Procesa_Ejecutar(
                int codEmpresa,
                Guid procesoId,
                ExcContratosAplicarRequest request)
        {
            var items = request.Seleccionados?
                .Where(x => !string.IsNullOrWhiteSpace(x.cedula))
                .ToList() ?? new List<CoAplExcContrAplInformacionData>();

            var ejecucion = new CoAplExcContrEjecucion
            {
                ProcesoId = procesoId,
                Usuario = (request.Usuario ?? string.Empty)
                    .Trim()
                    .ToUpperInvariant(),
                Total = items.Count
            };

            var estadoFinal = GenProcesoEstados.Terminado;
            var mensajeFinal = string.Empty;

            try
            {
                if (items.Count == 0)
                {
                    throw new CoAplExcContrResultadoException(
                        "No hay casos a procesar.");
                }

                if (ejecucion.Usuario.Length == 0 ||
                    ejecucion.Usuario.Length > 30)
                {
                    throw new CoAplExcContrResultadoException(
                        "El usuario es obligatorio y admite un máximo de 30 caracteres.");
                }

                ejecucion.Etapa = "Registrar el inicio en GEN_PROCESO";

                _procesoDb.Actualizar(
                    codEmpresa,
                    procesoId,
                    GenProcesoEstados.EnProceso,
                    0,
                    0,
                    "Preparando la aplicación de excedentes.");

                ejecucion.AplicacionId =
                    CO_AplExcContrApl_Guia_Crear(codEmpresa, ejecucion);

                foreach (var item in items)
                {
                    ejecucion.Etapa =
                        $"Ejecutar el caso {ejecucion.Procesados + 1}";

                    var error = CO_AplExcContrApl_Caso_Ejecutar(
                        codEmpresa,
                        ejecucion,
                        item);

                    ejecucion.Procesados++;

                    if (error is not null)
                    {
                        ejecucion.Errores.Add(error);
                    }

                    // Un error al registrar el avance es un problema general,
                    // no un fallo adicional del contrato.
                    ejecucion.Etapa =
                        "Registrar el avance en GEN_PROCESO";

                    _procesoDb.Actualizar(
                        codEmpresa,
                        procesoId,
                        GenProcesoEstados.EnProceso,
                        ejecucion.Procesados,
                        ejecucion.Errores.Count,
                        $"Aplicación {ejecucion.AplicacionId}. " +
                        $"Procesados: {ejecucion.Procesados} de {ejecucion.Total}. " +
                        $"Correctos: {ejecucion.Correctos}. " +
                        $"Con error: {ejecucion.Errores.Count}.");
                }
            }
            catch (Exception ex)
            {
                estadoFinal = GenProcesoEstados.Error;
                mensajeFinal =
                    $"Etapa: {ejecucion.Etapa}. " +
                    CO_AplExcContrApl_Error_Describir(ex);
            }

            return CO_AplExcContrApl_Proceso_Finalizar(
                codEmpresa,
                ejecucion,
                new CoAplExcContrProcesoEstadoResult
                {
                    Estado = estadoFinal,
                    Mensaje = mensajeFinal
                });
        }

        /// <summary>
        /// Ejecuta la guía con @pUsuario y lee el campo Aplicacion.
        /// </summary>
        private int CO_AplExcContrApl_Guia_Crear(
            int codEmpresa,
            CoAplExcContrEjecucion ejecucion)
        {
            ejecucion.Etapa = "Obtener la conexión para crear la guía";

            using var connection =
                DbHelper.OpenConnection(_portalDB, codEmpresa);

            ejecucion.Etapa = "Abrir la conexión para crear la guía";
            connection.Open();

            ejecucion.Etapa =
                "Ejecutar spCBR_Excedente_Apl_Contratos_Proceso_Guia_Aplicacion";

            var resultados = connection.Query<CoAplExcContrGuiaResultado>(
                "dbo.spCBR_Excedente_Apl_Contratos_Proceso_Guia_Aplicacion",
                new
                {
                    pUsuario = ejecucion.Usuario
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 0
            ).ToList();

            ejecucion.Etapa = "Validar la transacción de la guía";

            CO_AplExcContrApl_Transaccion_Verificar(connection);

            ejecucion.Etapa = "Validar el identificador de la guía";

            if (resultados.Count != 1)
            {
                throw new CoAplExcContrResultadoException(
                    $"La guía devolvió {resultados.Count} filas; se esperaba una.");
            }

            var aplicacion = resultados[0].aplicacion;

            if (!aplicacion.HasValue || aplicacion.Value <= 0)
            {
                throw new CoAplExcContrResultadoException(
                    "La guía no devolvió un identificador positivo en Aplicacion.");
            }

            return aplicacion.Value;
        }

        /// <summary>
        /// Ejecuta un caso con los parámetros reales del SP.
        /// Lee Resultado y conserva el diagnóstico de negocio.
        /// </summary>
        private CoAplExcContrProcesoErrorDto?
            CO_AplExcContrApl_Caso_Ejecutar(
                int codEmpresa,
                CoAplExcContrEjecucion ejecucion,
                CoAplExcContrAplInformacionData item)
        {
            var cedula = item.cedula.Trim();
            var etapa = "Validar la identificación";
            string mensaje;

            try
            {
                if (cedula.Length > 15)
                {
                    throw new CoAplExcContrResultadoException(
                        "La identificación supera los 15 caracteres admitidos por el SP.");
                }

                etapa = "Obtener la conexión del caso";

                using var connection =
                    DbHelper.OpenConnection(_portalDB, codEmpresa);

                etapa = "Abrir la conexión del caso";
                connection.Open();

                etapa =
                    "Ejecutar spCBR_Excedente_Apl_Contratos_Proceso_Aplicacion";

                var resultados = connection.Query<CoAplExcContrCasoResultado>(
                    "dbo.spCBR_Excedente_Apl_Contratos_Proceso_Aplicacion",
                    new
                    {
                        pUsuario = ejecucion.Usuario,
                        pCedula = cedula,
                        pIdAplicacion = ejecucion.AplicacionId
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 0
                ).ToList();

                etapa = "Validar la transacción del caso";

                CO_AplExcContrApl_Transaccion_Verificar(connection);

                etapa = "Leer el resultado del caso";

                if (resultados.Count != 1)
                {
                    throw new CoAplExcContrResultadoException(
                        $"El procedimiento devolvió {resultados.Count} filas; " +
                        "se esperaba una fila con el campo Resultado.");
                }

                var motivo = resultados[0].resultado?.Trim();

                if (!string.IsNullOrWhiteSpace(motivo))
                {
                    // Es el mensaje de negocio devuelto por el SP.
                    mensaje =
                        $"El procedimiento informó: {motivo} " +
                        "Revise los movimientos antes de reintentar.";
                }
                else
                {
                    etapa = "Validar la aplicación registrada";

                    var valido = connection.QuerySingle<bool?>(
                        """
                        SELECT dbo.fxCBR_Excedente_Apl_Contratos_Proceso_Valida(
                            @Cedula,
                            @Aplicacion
                        );
                        """,
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
                        "El SP no informó un motivo, pero la función de validación " +
                        "no confirmó una aplicación registrada como procesada " +
                        "para esta cédula y aplicación. " +
                        "Revise los movimientos antes de reintentar.";
                }
            }
            catch (Exception ex)
            {
                mensaje =
                    $"Etapa: {etapa}. " +
                    CO_AplExcContrApl_Error_Describir(ex) +
                    " Revise los movimientos antes de reintentar.";
            }

            return new CoAplExcContrProcesoErrorDto
            {
                numero_caso = ejecucion.Procesados + 1,
                cedula = cedula,
                nombre = item.nombre ?? string.Empty,
                operaciones = item.operaciones ?? string.Empty,
                mensaje = mensaje
            };
        }

        /// <summary>
        /// Detecta transacciones abiertas por un SP en una conexión
        /// que el API abrió sin transacción propia.
        /// La conexión se desecha al salir del método llamador.
        /// </summary>
        private static void CO_AplExcContrApl_Transaccion_Verificar(
            IDbConnection connection)
        {
            var transacciones = connection.QuerySingle<int>(
                "SELECT @@TRANCOUNT;",
                commandTimeout: 30);

            if (transacciones != 0)
            {
                throw new CoAplExcContrResultadoException(
                    $"El procedimiento dejó {transacciones} transacciones abiertas. " +
                    "El caso no se considera confirmado; se cerrará su conexión.");
            }
        }

        /// <summary>
        /// Describe errores técnicos sin exponer conexiones o credenciales.
        /// </summary>
        private static string CO_AplExcContrApl_Error_Describir(
            Exception exception)
        {
            if (exception is SqlException sql)
            {
                return CO_AplExcContrApl_ErrorSql_Describir(sql);
            }

            var error = exception.GetBaseException();

            if (error is SqlException sqlInterno)
            {
                return CO_AplExcContrApl_ErrorSql_Describir(sqlInterno);
            }

            if (error is CoAplExcContrResultadoException)
            {
                return error.Message;
            }

            if (exception is DataException ||
                error is DataException ||
                error is InvalidCastException ||
                error is FormatException ||
                error is OverflowException)
            {
                return
                    "No fue posible convertir el resultado SQL al tipo esperado. " +
                    $"Tipo: {exception.GetType().Name}.";
            }

            if (error is InvalidOperationException)
            {
                return
                    "Operación inválida al abrir la conexión o leer el resultado SQL.";
            }

            if (error is TimeoutException)
            {
                return "La operación excedió el tiempo de espera.";
            }

            return $"Tipo de error: {error.GetType().Name}.";
        }

        /// <summary>
        /// Describe los errores SQL por número, procedimiento y línea.
        /// </summary>
        private static string CO_AplExcContrApl_ErrorSql_Describir(
            SqlException exception)
        {
            var detalles = exception.Errors
                .Cast<SqlError>()
                .Select(error =>
                {
                    var descripcion = error.Number switch
                    {
                        -2 => "Tiempo de espera agotado",
                        201 => "Falta un parámetro requerido por el procedimiento",
                        1205 => "La operación fue víctima de un interbloqueo",
                        1222 => "Tiempo de espera de bloqueo agotado",
                        266 => "Cantidad de BEGIN y COMMIT no coincidente",
                        2812 => "Procedimiento almacenado no encontrado",
                        229 => "Permiso denegado",
                        2601 or 2627 => "Conflicto con una clave única",
                        _ => "Error de SQL Server"
                    };

                    var procedimiento =
                        string.IsNullOrWhiteSpace(error.Procedure)
                            ? "(no informado)"
                            : error.Procedure;

                    return
                        $"{descripcion}. SQL {error.Number}, " +
                        $"estado {error.State}, " +
                        $"procedimiento {procedimiento}, " +
                        $"línea {error.LineNumber}";
                })
                .Distinct();

            return string.Join(" | ", detalles);
        }

        /// <summary>
        /// Publica el detalle temporal y registra el resumen en GEN_PROCESO.
        /// Conserva los contadores ante una falla de registro final.
        /// </summary>
        private ErrorDto<CoAplExcContrAplicadosResult>
            CO_AplExcContrApl_Proceso_Finalizar(
                int codEmpresa,
                CoAplExcContrEjecucion ejecucion,
                CoAplExcContrProcesoEstadoResult estado)
        {
            estado.ProcesoId = ejecucion.ProcesoId;
            estado.TotalItems = ejecucion.Total;
            estado.ItemsProcesados = ejecucion.Procesados;
            estado.ItemsConError = ejecucion.Errores.Count;
            estado.DetalleDisponible = true;
            estado.Errores = ejecucion.Errores.ToList();

            var aplicacion = ejecucion.AplicacionId > 0
                ? ejecucion.AplicacionId.ToString()
                : "sin identificador confirmado";

            var resumen =
                $"Aplicación: {aplicacion}. " +
                $"Correctos: {ejecucion.Correctos}. " +
                $"Con error: {ejecucion.Errores.Count}. " +
                $"Sin intentar: {ejecucion.SinIntentar}. " +
                $"Procesados: {ejecucion.Procesados} de {ejecucion.Total}.";

            estado.Mensaje = string.IsNullOrWhiteSpace(estado.Mensaje)
                ? resumen
                : $"{estado.Mensaje} {resumen}";

            CO_AplExcContrApl_Resultado_Guardar(codEmpresa, estado);

            try
            {
                _procesoDb.Actualizar(
                    codEmpresa,
                    ejecucion.ProcesoId,
                    estado.Estado,
                    estado.ItemsProcesados,
                    estado.ItemsConError,
                    estado.Mensaje);
            }
            catch (Exception ex)
            {
                estado.Mensaje +=
                    " No fue posible registrar el estado final en GEN_PROCESO. " +
                    CO_AplExcContrApl_Error_Describir(ex);

                CO_AplExcContrApl_Resultado_Guardar(codEmpresa, estado);
            }

            var resultado = new CoAplExcContrAplicadosResult
            {
                aplicados = ejecucion.Correctos,
                pendientes = ejecucion.Total - ejecucion.Correctos
            };

            return estado.Estado == GenProcesoEstados.Terminado
                ? DbHelper.CreateOkResponse(
                    resultado,
                    estado.Mensaje ?? string.Empty)
                : DbHelper.CreateErrorResponse(
                    estado.Mensaje ?? string.Empty,
                    -1,
                    resultado);
        }

        /// <summary>
        /// Conserva una copia del resultado final por dos horas.
        /// </summary>
        private static void CO_AplExcContrApl_Resultado_Guardar(
            int codEmpresa,
            CoAplExcContrProcesoEstadoResult estado)
        {
            ResultadosTemporales.Set(
                (codEmpresa, estado.ProcesoId),
                JsonSerializer.Serialize(estado),
                TimeSpan.FromHours(2));
        }

        /// <summary>
        /// Consulta el resultado final temporal o el avance de GEN_PROCESO.
        /// </summary>
        public ErrorDto<CoAplExcContrProcesoEstadoResult>
            CO_AplExcContrApl_Procesa_Estado(
                int codEmpresa,
                Guid procesoId)
        {
            try
            {
                if (ResultadosTemporales.TryGetValue<string>(
                        (codEmpresa, procesoId),
                        out var json) &&
                    !string.IsNullOrWhiteSpace(json))
                {
                    var resultado =
                        JsonSerializer.Deserialize<CoAplExcContrProcesoEstadoResult>(
                            json);

                    if (resultado is not null)
                    {
                        return DbHelper.CreateOkResponse(resultado);
                    }
                }

                var estado = _procesoDb.Estado_Obtener(
                    codEmpresa,
                    procesoId);

                if (estado is null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Proceso no encontrado.",
                        -1,
                        new CoAplExcContrProcesoEstadoResult());
                }

                return DbHelper.CreateOkResponse(
                    new CoAplExcContrProcesoEstadoResult
                    {
                        ProcesoId = estado.ProcesoId,
                        Estado = estado.Estado,
                        TotalItems = estado.TotalItems,
                        ItemsProcesados = estado.ItemsProcesados,
                        ItemsConError = estado.ItemsConError,
                        Mensaje = estado.Mensaje,
                        FechaInicio = estado.FechaInicio,
                        FechaFin = estado.FechaFin,
                        DetalleDisponible = false
                    });
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(
                    "No fue posible consultar el avance. " +
                    CO_AplExcContrApl_Error_Describir(ex),
                    -1,
                    new CoAplExcContrProcesoEstadoResult());
            }
        }
    }
}