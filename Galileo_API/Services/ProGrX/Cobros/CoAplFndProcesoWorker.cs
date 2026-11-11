using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de fondos (Procesa) y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplFndProcesoWorker : CoProcesoWorkerBase<CoAplFndProcesoTrabajo>
    {
        private readonly CoAplFndProcesoQueue _queue;
        private readonly FrmCoAplFndProcesaDb _db;

        public CoAplFndProcesoWorker(
            CoAplFndProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplFndProcesoWorker> logger)
            : base(config, logger)
        {
            _queue = queue;
            _db = new FrmCoAplFndProcesaDb(config);
        }

        protected override ValueTask<CoAplFndProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _queue.LeerAsync(ct);

        protected override void EjecutarTrabajo(CoAplFndProcesoTrabajo trabajo) =>
            _db.CO_AplFnd_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
    }
}
