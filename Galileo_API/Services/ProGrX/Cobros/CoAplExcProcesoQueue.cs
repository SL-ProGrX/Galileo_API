using System.Threading.Channels;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Cola en memoria de trabajos de aplicación de excedentes.
    /// El estado persistente del avance vive en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplExcProcesoQueue
    {
        private readonly Channel<CoAplExcProcesoTrabajo> _channel =
            Channel.CreateBounded<CoAplExcProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

        /// <summary>
        /// Encola el trabajo. Retorna false si la cola está llena.
        /// </summary>
        public bool Encolar(CoAplExcProcesoTrabajo trabajo) => _channel.Writer.TryWrite(trabajo);

        public ValueTask<CoAplExcProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _channel.Reader.ReadAsync(ct);
    }
}
