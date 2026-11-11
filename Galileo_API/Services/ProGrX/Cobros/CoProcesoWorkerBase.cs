using Galileo.DataBaseTier;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Base de los workers que ejecutan en segundo plano los procesos de aplicación de Cobros
    /// y registran su avance en GEN_PROCESO. Cada worker concreto indica cómo leer su cola
    /// y cómo ejecutar su trabajo.
    /// </summary>
    public abstract class CoProcesoWorkerBase<TTrabajo> : BackgroundService
        where TTrabajo : ICoProcesoTrabajo
    {
        private readonly GenProcesoDb _procesoDb;
        private readonly ILogger _logger;

        protected CoProcesoWorkerBase(IConfiguration config, ILogger logger)
        {
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _logger = logger;
        }

        protected abstract ValueTask<TTrabajo> LeerAsync(CancellationToken ct);

        protected abstract void EjecutarTrabajo(TTrabajo trabajo);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nombre = GetType().Name;
            _logger.LogInformation("{Nombre} iniciado.", nombre);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var trabajo = await LeerAsync(stoppingToken);
                    await Task.Run(() => Procesar(trabajo), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en {Nombre}.", nombre);
                    await Task.Delay(1000, stoppingToken);
                }
            }
        }

        private void Procesar(TTrabajo trabajo)
        {
            try
            {
                EjecutarTrabajo(trabajo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando {ProcesoId}.", trabajo.ProcesoId);
                RegistrarError(trabajo, ex.Message);
            }
        }

        private void RegistrarError(TTrabajo trabajo, string mensaje)
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
