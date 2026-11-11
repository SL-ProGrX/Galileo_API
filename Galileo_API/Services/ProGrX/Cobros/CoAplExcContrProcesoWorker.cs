using Galileo.DataBaseTier;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de excedentes por contratos y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplExcContrProcesoWorker : BackgroundService
    {
        private readonly CoAplExcContrProcesoQueue _queue;
        private readonly FrmCoAplExcContratosAplicacionDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly ILogger<CoAplExcContrProcesoWorker> _logger;

        public CoAplExcContrProcesoWorker(
            CoAplExcContrProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplExcContrProcesoWorker> logger)
        {
            _queue = queue;
            _db = new FrmCoAplExcContratosAplicacionDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CoAplExcContrProcesoWorker iniciado.");

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
                    _logger.LogError(ex, "Error en CoAplExcContrProcesoWorker.");
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private void Procesar(CoAplExcContrProcesoTrabajo trabajo)
        {
            try
            {
                _db.CO_AplExcContrApl_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando {ProcesoId}.", trabajo.ProcesoId);
                RegistrarError(trabajo, ex.Message);
            }
        }

        private void RegistrarError(CoAplExcContrProcesoTrabajo trabajo, string mensaje)
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
