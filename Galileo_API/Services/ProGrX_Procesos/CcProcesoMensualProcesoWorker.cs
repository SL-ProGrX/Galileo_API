using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB;
using Galileo_API.DataBaseTier.ProGrX_Procesos.frmCC_ProcesoMensualDB.CargaArchivos;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualCargaArchivos;
using static Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels.CcProcesoMensualModels;

namespace Galileo_API.Services.ProGrX_Procesos
{
    /// <summary>
    /// Background Worker que procesa trabajos de proceso mensual de forma asíncrona.
    /// Adquiere procesos Pendientes, los ejecuta y actualiza el avance.
    /// </summary>
    public sealed class CcProcesoMensualProcesoWorker : BackgroundService
    {
        private readonly CcProcesoMensualProcesoQueue _queue;
        private readonly CcProcesoMensualProcesoDb _procesoDb;
        private readonly CcProcesoMensualEnvioDb _envioDb;
        private readonly CcProcesoMensualCargaArchivosDb _cargaArchivosDb;
        private readonly ILogger<CcProcesoMensualProcesoWorker> _logger;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString
        };

        public CcProcesoMensualProcesoWorker(
            CcProcesoMensualProcesoQueue queue,
            IConfiguration config,
            ILogger<CcProcesoMensualProcesoWorker> logger)
        {
            _queue = queue;
            _procesoDb = new CcProcesoMensualProcesoDb(config);
            _envioDb = new CcProcesoMensualEnvioDb(config);
            _cargaArchivosDb = new CcProcesoMensualCargaArchivosDb(config);
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CcProcesoMensualProcesoWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var trabajo = await _queue.LeerAsync(stoppingToken);
                    await ProcesarTrabajoAsync(trabajo, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en CcProcesoMensualProcesoWorker.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private async Task ProcesarTrabajoAsync(
            CcProcesoMensualProcesoTrabajo trabajo, CancellationToken ct)
        {
            _logger.LogInformation(
                "Procesando trabajo {ProcesoId} para empresa {CodEmpresa}.",
                trabajo.ProcesoId, trabajo.CodEmpresa);

            try
            {
                // Obtener contexto del proceso
                var contextoJson = _procesoDb.Proceso_Contexto_Obtener(
                    trabajo.CodEmpresa, trabajo.ProcesoId);

                if (string.IsNullOrEmpty(contextoJson))
                {
                    _procesoDb.Proceso_Error_Registrar(
                        trabajo.CodEmpresa, trabajo.ProcesoId,
                        "No se encontró el contexto del proceso.");
                    return;
                }

                // Adquirir el proceso (Pendiente -> Procesando)
                var proceso = _procesoDb.Proceso_Adquirir(
                    trabajo.CodEmpresa, trabajo.ProcesoId);

                if (proceso is null)
                {
                    _logger.LogWarning(
                        "Proceso {ProcesoId} no pudo ser adquirido.", trabajo.ProcesoId);
                    return;
                }

                // Procesar según el tipo
                switch (proceso.TipoProceso)
                {
                    case "02":
                        await ProcesarGeneraDeduccionesAsync(trabajo, proceso, contextoJson, ct);
                        break;
                    case "03":
                        await ProcesarCargaDeduccionesAsync(trabajo, proceso, contextoJson, ct);
                        break;
                    default:
                        _procesoDb.Proceso_Error_Registrar(
                            trabajo.CodEmpresa, trabajo.ProcesoId,
                            $"Tipo de proceso no soportado: {proceso.TipoProceso}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error procesando trabajo {ProcesoId}.", trabajo.ProcesoId);

                _procesoDb.Proceso_Error_Registrar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    $"Error interno: {ex.Message}");
            }
            finally
            {
                _queue.Liberar(trabajo.ProcesoId);
            }
        }

        private async Task ProcesarGeneraDeduccionesAsync(
            CcProcesoMensualProcesoTrabajo trabajo,
            CcProcesoMensualProcesoResultado proceso,
            string contextoJson,
            CancellationToken ct)
        {
            contextoJson = RemoverCamposDelContexto(
                contextoJson,
                "codInstitucion", "CodInstitucion",
                "fechaProceso", "FechaProceso");

            var contexto = JsonSerializer.Deserialize<CcProcesoMensualGeneraDeduccionesContexto>(
                contextoJson,
                JsonOptions);

            if (contexto is null)
            {
                _procesoDb.Proceso_Error_Registrar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    "Error al deserializar el contexto de Genera Deducciones.");
                return;
            }

            // El proceso de Genera Deducciones es un SP que se ejecuta completo
            // No se puede dividir en lotes, pero podemos reportar progreso
            _procesoDb.Proceso_Avance_Actualizar(
                trabajo.CodEmpresa, trabajo.ProcesoId,
                total: 1, procesadas: 0, exitosas: 0, errores: 0,
                mensaje: "Ejecutando SP de generación de deducciones...");

            await Task.Delay(100, ct); // Permitir que el polling capture el estado

            try
            {
                // Ejecutar el SP principal
                var request = new CcProcesoMensualGeneraDeduccionesRequest
                {
                    CodInstitucion = proceso.CodInstitucion,
                    FechaProceso = proceso.FechaProceso,
                    Usuario = contexto.Usuario,
                    UsaPlanillaTransito = contexto.UsaPlanillaTransito,
                    AplicaCambioDeducciones = contexto.AplicaCambioDeducciones,
                    Redondeo = contexto.Redondeo,
                    NombreInstitucion = contexto.NombreInstitucion,
                    NombreEmpresa = contexto.NombreEmpresa
                };

                var resultado = _envioDb.CcProcesoMensual_GeneraDeducciones_Ejecutar(
                    trabajo.CodEmpresa, request);

                if (!resultado.Result)
                {
                    _procesoDb.Proceso_Error_Registrar(
                        trabajo.CodEmpresa, trabajo.ProcesoId,
                        resultado.Description ?? "Error al generar deducciones.");
                    return;
                }

                _procesoDb.Proceso_Avance_Actualizar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    total: 1, procesadas: 1, exitosas: 1, errores: 0,
                    mensaje: "Deducciones generadas correctamente.");

                _procesoDb.Proceso_Finalizar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    "Genera Deducciones completado exitosamente.");
            }
            catch (Exception ex)
            {
                _procesoDb.Proceso_Error_Registrar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    $"Error en Genera Deducciones: {ex.Message}");
            }
        }

        private async Task ProcesarCargaDeduccionesAsync(
            CcProcesoMensualProcesoTrabajo trabajo,
            CcProcesoMensualProcesoResultado proceso,
            string contextoJson,
            CancellationToken ct)
        {
            var pago = ObtenerEnteroDelContexto(contextoJson, "pago", "Pago");
            contextoJson = RemoverCamposDelContexto(
                contextoJson,
                "codEmpresa", "CodEmpresa",
                "codInstitucion", "CodInstitucion",
                "fechaProceso", "FechaProceso",
                "pago", "Pago");

            var request = JsonSerializer.Deserialize<CcProcesoMensualCargaDeduccionesRequest>(
                contextoJson,
                JsonOptions);

            if (request is null)
            {
                _procesoDb.Proceso_Error_Registrar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    "Error al deserializar el contexto de Carga Deducciones.");
                return;
            }

            request.CodEmpresa = trabajo.CodEmpresa;
            request.CodInstitucion = proceso.CodInstitucion;
            request.FechaProceso = proceso.FechaProceso;
            request.Pago = pago ?? 0;
            var totalFilas = request.Filas.Count;

            _procesoDb.Proceso_Avance_Actualizar(
                trabajo.CodEmpresa, trabajo.ProcesoId,
                total: totalFilas, procesadas: 0, exitosas: 0, errores: 0,
                mensaje: $"Preparando carga de {totalFilas} filas...");

            await Task.Yield();
            ct.ThrowIfCancellationRequested();

            var reglas = CcProcesoMensualCargaDeduccionesConfig.ObtenerReglas(request.TipoCarga);
            var resultado = _cargaArchivosDb.CargarDeduccionesGenerico(
                request,
                reglas,
                progreso => _procesoDb.Proceso_Avance_Actualizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    total: progreso.Total,
                    procesadas: progreso.Procesadas,
                    exitosas: progreso.Exitosas,
                    errores: progreso.Errores,
                    mensaje: progreso.Mensaje));

            if (resultado.Code == -1)
            {
                _procesoDb.Proceso_Error_Registrar(
                    trabajo.CodEmpresa, trabajo.ProcesoId,
                    resultado.Description ?? "Error al cargar deducciones.");
                return;
            }

            var detalle = resultado.Result;
            var mensajeFinal = detalle?.Mensaje ?? "Carga de deducciones completada.";
            if (detalle?.PersonasNoEncontradas > 1)
            {
                mensajeFinal += " Existen casos no registrados en la base de datos.";
            }

            _procesoDb.Proceso_Avance_Actualizar(
                trabajo.CodEmpresa,
                trabajo.ProcesoId,
                total: detalle?.RegistrosInsertados ?? totalFilas,
                procesadas: detalle?.RegistrosInsertados ?? totalFilas,
                exitosas: detalle?.RegistrosInsertados ?? totalFilas,
                errores: 0,
                mensaje: mensajeFinal);

            _procesoDb.Proceso_Finalizar(
                trabajo.CodEmpresa, trabajo.ProcesoId,
                mensajeFinal);
        }

        private static string RemoverCamposDelContexto(string contextoJson, params string[] campos)
        {
            var contexto = JsonNode.Parse(contextoJson);
            if (contexto is not JsonObject objeto)
            {
                return contextoJson;
            }

            foreach (var campo in campos)
            {
                objeto.Remove(campo);
            }

            return objeto.ToJsonString(JsonOptions);
        }

        private static int? ObtenerEnteroDelContexto(string contextoJson, params string[] campos)
        {
            var contexto = JsonNode.Parse(contextoJson);
            if (contexto is not JsonObject objeto)
            {
                return null;
            }

            foreach (var campo in campos)
            {
                if (!objeto.TryGetPropertyValue(campo, out var valor) || valor is null)
                {
                    continue;
                }

                if (valor.GetValueKind() == JsonValueKind.Number && valor.GetValue<int>() is var numero)
                {
                    return numero;
                }

                if (valor.GetValueKind() == JsonValueKind.String &&
                    int.TryParse(valor.GetValue<string>(), out var numeroTexto))
                {
                    return numeroTexto;
                }
            }

            return null;
        }
    }
}
