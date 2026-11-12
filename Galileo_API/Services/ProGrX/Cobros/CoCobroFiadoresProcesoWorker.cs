using Galileo.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Ejecuta en segundo plano el procesamiento de cobros a fiadores y registra el avance en GEN_PROCESO.
    /// </summary>
    public sealed class CoCobroFiadoresProcesoWorker : CoProcesoWorkerBase<CoCobroFiadoresProcesoTrabajo>
    {
        private readonly CoCobroFiadoresProcesoQueue _queue;
        private readonly FrmCOCobroFiadoresDB _db;

        public CoCobroFiadoresProcesoWorker(
            CoCobroFiadoresProcesoQueue queue,
            IConfiguration config,
            ILogger<CoCobroFiadoresProcesoWorker> logger)
            : base(config, logger)
        {
            _queue = queue;
            _db = new FrmCOCobroFiadoresDB(config);
        }

        protected override ValueTask<CoCobroFiadoresProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _queue.LeerAsync(ct);

        protected override void EjecutarTrabajo(CoCobroFiadoresProcesoTrabajo trabajo) =>
            _db.CO_CobroFiadores_ProcesaCobros_Ejecutar(trabajo.CodEmpresa, trabajo.ProcesoId,
                trabajo.Usuario, trabajo.Ids, _queue);
    }
}
