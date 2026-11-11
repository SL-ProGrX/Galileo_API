using System.Threading.Channels;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Cola en memoria de trabajos de aplicación de fondos a mora.
    /// El estado persistente del avance vive en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplFndContrProcesoQueue
    {
        private readonly Channel<CoAplFndContrProcesoTrabajo> _channel =
            Channel.CreateBounded<CoAplFndContrProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

        /// <summary>
        /// Encola el trabajo. Retorna false si la cola está llena.
        /// </summary>
        public bool Encolar(CoAplFndContrProcesoTrabajo trabajo) => _channel.Writer.TryWrite(trabajo);

        public ValueTask<CoAplFndContrProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _channel.Reader.ReadAsync(ct);
    }
}
