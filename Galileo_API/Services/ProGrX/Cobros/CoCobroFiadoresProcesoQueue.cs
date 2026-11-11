using System.Threading.Channels;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Cola en memoria de trabajos de procesamiento de cobros a fiadores.
    /// El estado persistente del avance vive en GEN_PROCESO.
    /// </summary>
    public sealed class CoCobroFiadoresProcesoQueue
    {
        private readonly Channel<CoCobroFiadoresProcesoTrabajo> _channel =
            Channel.CreateBounded<CoCobroFiadoresProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

        /// <summary>
        /// Encola el trabajo. Retorna false si la cola está llena.
        /// </summary>
        public bool Encolar(CoCobroFiadoresProcesoTrabajo trabajo) => _channel.Writer.TryWrite(trabajo);

        public ValueTask<CoCobroFiadoresProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _channel.Reader.ReadAsync(ct);
    }
}
