using Galileo.Models.TES;
using Galileo_API.DataBaseTier.ProGrX.Bancos.frmTES_EmisionDocumentos;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Galileo_API.Services.ProGrX.Bancos
{
    public sealed class TesEmisionDocumentosProcesoWorker : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TesEmisionDocumentosProcesoWorker> _logger;
        private readonly TesEmisionDocumentosProcesoQueue _queue;
        private readonly TesEmisionDocumentosArchivoStore _archivoStore;

        public TesEmisionDocumentosProcesoWorker(
            IConfiguration configuration,
            IOptions<TesEmisionDocumentosProcesoOptions> options,
            ILogger<TesEmisionDocumentosProcesoWorker> logger,
            TesEmisionDocumentosProcesoQueue queue)
        {
            _configuration = configuration;
            _logger = logger;
            _queue = queue;
            var rutaBase = configuration["ArchivosGenerados:RutaBase"]
                ?? throw new InvalidOperationException("ArchivosGenerados:RutaBase no está configurada.");
            _archivoStore = new TesEmisionDocumentosArchivoStore(
                Path.Combine(rutaBase, options.Value.Subcarpeta));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                TesEmisionDocumentosProcesoTrabajo trabajo;
                try
                {
                    trabajo = await _queue.LeerAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }

                await ProcesarAsync(trabajo, stoppingToken);
            }
        }

        private Task ProcesarAsync(
            TesEmisionDocumentosProcesoTrabajo trabajo,
            CancellationToken stoppingToken)
        {
            return Task.Run(() => Procesar(trabajo, stoppingToken), CancellationToken.None);
        }

        private void Procesar(
            TesEmisionDocumentosProcesoTrabajo trabajo,
            CancellationToken stoppingToken)
        {
            var db = new FrmTesEmisionDocumentosDb(_configuration);
            var efectosDeNegocioPosibles = false;

            try
            {
                var contexto = db.TES_EmisionDocumentos_Proceso_Adquirir(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId);
                if (contexto == null)
                {
                    return;
                }

                stoppingToken.ThrowIfCancellationRequested();
                CambiarEstadoGenerando(db, trabajo);
                using var heartbeat = new Timer(
                    _ => RegistrarActividadSegura(db, trabajo),
                    null,
                    TimeSpan.FromSeconds(15),
                    TimeSpan.FromSeconds(15));
                efectosDeNegocioPosibles = true;
                var resultado = db.TES_EmisionDocumento_Generar(
                    trabajo.CodEmpresa,
                    contexto.filtros
                    );

                if (resultado.Code != 0 || resultado.Result == null)
                {
                    throw new InvalidOperationException(
                        resultado.Description ?? "La generación no devolvió un resultado válido.");
                }

                stoppingToken.ThrowIfCancellationRequested();
                CambiarEstadoValidando(db, trabajo);
                var archivos = FrmTesEmisionDocumentosResultadoExtractor.Extraer(
                    resultado.Result as string ?? JsonConvert.SerializeObject(resultado.Result));
                if (archivos.Count == 0)
                {
                    throw new InvalidDataException("La generación no produjo archivos publicables.");
                }

                PublicarArchivos(db, trabajo, archivos);
                db.TES_EmisionDocumentos_Proceso_Finalizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    FrmTesEmisionDocumentosResultadoExtractor.CrearContextoLigero(
                        resultado.Result as string ?? JsonConvert.SerializeObject(resultado.Result)));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                RegistrarInterrupcion(db, trabajo);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Falló la emisión {ProcesoId} de la empresa {CodEmpresa}.",
                    trabajo.ProcesoId,
                    trabajo.CodEmpresa);
                if (efectosDeNegocioPosibles)
                {
                    RegistrarRevision(db, trabajo);
                }
                else
                {
                    db.TES_EmisionDocumentos_Proceso_Error_Registrar(
                        trabajo.CodEmpresa,
                        trabajo.ProcesoId,
                        "La emisión no pudo iniciarse. Revise el detalle técnico del API.");
                }
            }
            finally
            {
                _queue.Liberar(trabajo.ProcesoId);
            }
        }

        private static void CambiarEstadoGenerando(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            if (!db.TES_EmisionDocumentos_Proceso_Estado_Actualizar(
                trabajo.CodEmpresa,
                trabajo.ProcesoId,
                TesEmisionDocumentosEstado.Preparando,
                TesEmisionDocumentosEstado.Generando,
                "Generando documentos"))
            {
                throw new InvalidOperationException("No se pudo iniciar la generación adquirida.");
            }
        }

        private static void CambiarEstadoValidando(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            if (!db.TES_EmisionDocumentos_Proceso_Estado_Actualizar(
                trabajo.CodEmpresa,
                trabajo.ProcesoId,
                TesEmisionDocumentosEstado.Generando,
                TesEmisionDocumentosEstado.Validando,
                "Validando archivos"))
            {
                throw new InvalidOperationException("No se pudo iniciar la validación de archivos.");
            }
        }

        private static void RegistrarAvance(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo,
            int procesadas,
            int total)
        {
            db.TES_EmisionDocumentos_Proceso_Avance_Actualizar(
                trabajo.CodEmpresa,
                new TesEmisionDocumentosAvancePersistir
                {
                    ProcesoId = trabajo.ProcesoId,
                    Total = total,
                    Procesadas = procesadas,
                    Exitosas = procesadas,
                    Errores = 0,
                    ConsultasRealizadas = procesadas,
                    Etapa = $"Generando documentos ({procesadas} de {total})"
                });
        }

        private void RegistrarActividadSegura(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            try
            {
                db.TES_EmisionDocumentos_Proceso_Actividad_Actualizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No fue posible actualizar la actividad de la emisión {ProcesoId}.",
                    trabajo.ProcesoId);
            }
        }

        private void PublicarArchivos(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo,
            IReadOnlyList<TesEmisionDocumentosArchivoGenerado> archivos)
        {
            for (var indice = 0; indice < archivos.Count; indice++)
            {
                var publicado = _archivoStore.Publicar(
                    trabajo.ProcesoId,
                    indice + 1,
                    archivos[indice]);
                db.TES_EmisionDocumentos_Proceso_Archivo_Registrar(
                    trabajo.CodEmpresa,
                    publicado);
            }
        }

        private static void RegistrarInterrupcion(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            _ = db.TES_EmisionDocumentos_Proceso_Estado_Actualizar(
                trabajo.CodEmpresa,
                trabajo.ProcesoId,
                TesEmisionDocumentosEstado.Generando,
                TesEmisionDocumentosEstado.RequiereRevision,
                "Interrumpido; requiere revisión");
        }

        private static void RegistrarRevision(
            FrmTesEmisionDocumentosDb db,
            TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            var actualizado = db.TES_EmisionDocumentos_Proceso_Estado_Actualizar(
                trabajo.CodEmpresa,
                trabajo.ProcesoId,
                TesEmisionDocumentosEstado.Validando,
                TesEmisionDocumentosEstado.RequiereRevision,
                "La generación requiere revisión antes de volver a ejecutarse");
            if (!actualizado)
            {
                _ = db.TES_EmisionDocumentos_Proceso_Estado_Actualizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    TesEmisionDocumentosEstado.Generando,
                    TesEmisionDocumentosEstado.RequiereRevision,
                    "La generación requiere revisión antes de volver a ejecutarse");
            }
        }
    }
}
