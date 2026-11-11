using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de excedentes y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplExcProcesoWorker : CoProcesoWorkerBase<CoAplExcProcesoTrabajo>
    {
        private readonly CoAplExcProcesoQueue _queue;
        private readonly FrmCoAplExcProcesaDb _db;

        public CoAplExcProcesoWorker(
            CoAplExcProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplExcProcesoWorker> logger)
            : base(config, logger)
        {
            _queue = queue;
            _db = new FrmCoAplExcProcesaDb(config);
        }

        protected override ValueTask<CoAplExcProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _queue.LeerAsync(ct);

        protected override void EjecutarTrabajo(CoAplExcProcesoTrabajo trabajo) =>
            _db.CO_AplExc_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
    }
}
