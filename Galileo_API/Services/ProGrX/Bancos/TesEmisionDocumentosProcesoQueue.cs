using Galileo.Models.TES;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Galileo_API.Services.ProGrX.Bancos
{
    public sealed class TesEmisionDocumentosProcesoQueue
    {
        private readonly Channel<TesEmisionDocumentosProcesoTrabajo> _channel =
            Channel.CreateBounded<TesEmisionDocumentosProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.DropWrite,
                    SingleReader = true,
                    SingleWriter = false
                });

        private readonly ConcurrentDictionary<Guid, byte> _activos = new();

        public bool Encolar(TesEmisionDocumentosProcesoTrabajo trabajo)
        {
            ArgumentNullException.ThrowIfNull(trabajo);
            if (!_activos.TryAdd(trabajo.ProcesoId, 0))
            {
                return false;
            }

            if (_channel.Writer.TryWrite(trabajo))
            {
                return true;
            }

            _activos.TryRemove(trabajo.ProcesoId, out _);
            return false;
        }

        public ValueTask<TesEmisionDocumentosProcesoTrabajo> LeerAsync(
            CancellationToken cancellationToken) =>
            _channel.Reader.ReadAsync(cancellationToken);

        public void Liberar(Guid procesoId) =>
            _activos.TryRemove(procesoId, out _);
    }
}
