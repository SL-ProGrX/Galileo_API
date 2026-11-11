using System.Threading.Channels;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Cola en memoria de trabajos de aplicación de excedentes por contratos.
    /// El estado persistente del avance vive en GEN_PROCESO.
    /// </summary>
    public sealed class CoAplExcContrProcesoQueue
    {
        private readonly Channel<CoAplExcContrProcesoTrabajo> _channel =
            Channel.CreateBounded<CoAplExcContrProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.Wait,
                    SingleReader = true,
                    SingleWriter = false
                });

        /// <summary>
        /// Encola el trabajo. Retorna false si la cola está llena.
        /// </summary>
        public bool Encolar(CoAplExcContrProcesoTrabajo trabajo) => _channel.Writer.TryWrite(trabajo);

        public ValueTask<CoAplExcContrProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _channel.Reader.ReadAsync(ct);
    }
}
