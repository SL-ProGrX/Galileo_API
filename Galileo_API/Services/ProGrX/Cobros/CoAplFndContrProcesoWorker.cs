using Galileo.DataBaseTier;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de fondos a mora y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplFndContrProcesoWorker : BackgroundService
    {
        private readonly CoAplFndContrProcesoQueue _queue;
        private readonly FrmCoAplFndContratosAplicacionDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly ILogger<CoAplFndContrProcesoWorker> _logger;

        public CoAplFndContrProcesoWorker(
            CoAplFndContrProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplFndContrProcesoWorker> logger)
        {
            _queue = queue;
            _db = new FrmCoAplFndContratosAplicacionDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoAplFndContrProcesoWorker iniciado.");

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
                    _logger.LogError(ex, "Error en CoAplFndContrProcesoWorker.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private void Procesar(CoAplFndContrProcesoTrabajo trabajo)
        {
            try
            {
                _db.CO_AplFndContrApl_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando {ProcesoId}.", trabajo.ProcesoId);
                RegistrarError(trabajo, ex.Message);
            }
        }

        private void RegistrarError(CoAplFndContrProcesoTrabajo trabajo, string mensaje)
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
