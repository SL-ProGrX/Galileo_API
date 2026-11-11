using System.Threading.Channels;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Cola en memoria de trabajos de aplicación de fondos (Procesa).
    /// El estado persistente del avance vive en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplFndProcesoQueue
    {
        private readonly Channel<CoAplFndProcesoTrabajo> _channel =
            Channel.CreateBounded<CoAplFndProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

        /// <summary>
        /// Encola el trabajo. Retorna false si la cola está llena.
        /// </summary>
        public bool Encolar(CoAplFndProcesoTrabajo trabajo) => _channel.Writer.TryWrite(trabajo);

        public ValueTask<CoAplFndProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _channel.Reader.ReadAsync(ct);
    }
}
