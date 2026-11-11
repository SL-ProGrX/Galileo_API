using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de fondos a mora y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplFndContrProcesoWorker : CoProcesoWorkerBase<CoAplFndContrProcesoTrabajo>
    {
        private readonly CoAplFndContrProcesoQueue _queue;
        private readonly FrmCoAplFndContratosAplicacionDb _db;

        public CoAplFndContrProcesoWorker(
            CoAplFndContrProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplFndContrProcesoWorker> logger)
            : base(config, logger)
        {
            _queue = queue;
            _db = new FrmCoAplFndContratosAplicacionDb(config);
        }

        protected override ValueTask<CoAplFndContrProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _queue.LeerAsync(ct);

        protected override void EjecutarTrabajo(CoAplFndContrProcesoTrabajo trabajo) =>
            _db.CO_AplFndContrApl_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
    }
}
