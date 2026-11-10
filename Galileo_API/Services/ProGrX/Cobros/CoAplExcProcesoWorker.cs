using Galileo.DataBaseTier;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de excedentes y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplExcProcesoWorker : BackgroundService
    {
        private readonly CoAplExcProcesoQueue _queue;
        private readonly FrmCoAplExcProcesaDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly ILogger<CoAplExcProcesoWorker> _logger;

        public CoAplExcProcesoWorker(
            CoAplExcProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplExcProcesoWorker> logger)
        {
            _queue = queue;
            _db = new FrmCoAplExcProcesaDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoAplExcProcesoWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var trabajo = await _queue.LeerAsync(stoppingToken);
                    await Task.Run(() => Procesar(trabajo), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en CoAplExcProcesoWorker.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private void Procesar(CoAplExcProcesoTrabajo trabajo)
        {
            try
            {
                _db.CO_AplExc_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando {ProcesoId}.", trabajo.ProcesoId);
                RegistrarError(trabajo, ex.Message);
            }
        }

        private void RegistrarError(CoAplExcProcesoTrabajo trabajo, string mensaje)
        {
            try
            {
                _procesoDb.Actualizar(
                    trabajo.CodEmpresa,
                    trabajo.ProcesoId,
                    GenProcesoEstados.Error,
                    0,
                    0,
                    mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo registrar el error del proceso {ProcesoId}.", trabajo.ProcesoId);
            }
        }
    }
}
