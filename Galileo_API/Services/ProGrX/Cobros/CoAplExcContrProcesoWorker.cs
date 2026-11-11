using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano los trabajos de aplicación de excedentes por contratos y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplExcContrProcesoWorker : CoProcesoWorkerBase<CoAplExcContrProcesoTrabajo>
    {
        private readonly CoAplExcContrProcesoQueue _queue;
        private readonly FrmCoAplExcContratosAplicacionDb _db;

        public CoAplExcContrProcesoWorker(
            CoAplExcContrProcesoQueue queue,
            IConfiguration config,
            ILogger<CoAplExcContrProcesoWorker> logger)
            : base(config, logger)
        {
            _queue = queue;
            _db = new FrmCoAplExcContratosAplicacionDb(config);
        }

        protected override ValueTask<CoAplExcContrProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _queue.LeerAsync(ct);

        protected override void EjecutarTrabajo(CoAplExcContrProcesoTrabajo trabajo) =>
            _db.CO_AplExcContrApl_Procesa_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId, trabajo.Request);
    }
}
