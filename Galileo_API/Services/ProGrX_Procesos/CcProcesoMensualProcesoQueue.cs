using System.Collections.Concurrent;
using System.Threading.Channels;
using Galileo_API.Models.ProGrX_Procesos.frmCC_ProcesoMensualModels;

namespace Galileo_API.Services.ProGrX_Procesos
{
    /// <summary>
    /// Cola en memoria para trabajos de proceso mensual.
    /// Channel<T> con capacidad limitada y deduplicación por ProcesoId.
    /// </summary>
    public sealed class CcProcesoMensualProcesoQueue
    {
        private readonly Channel<CcProcesoMensualProcesoTrabajo> _channel =
            Channel.CreateBounded<CcProcesoMensualProcesoTrabajo>(
                new BoundedChannelOptions(100)
                {
                    FullMode = BoundedChannelFullMode.DropWrite,
                    SingleReader = true,
                    SingleWriter = false
                });

        private readonly ConcurrentDictionary<Guid, byte> _activos = new();

        /// <summary>
        /// Encola un trabajo si no hay otro proceso activo con el mismo ProcesoId.
        /// </summary>
        public bool Encolar(CcProcesoMensualProcesoTrabajo trabajo)
        {
            if (!_activos.TryAdd(trabajo.ProcesoId, 0))
                return false;

            if (_channel.Writer.TryWrite(trabajo))
                return true;

            _activos.TryRemove(trabajo.ProcesoId, out _);
            return false;
        }

        /// <summary>
        /// Libera el tracking de un proceso (después de completar o error).
        /// </summary>
        public void Liberar(Guid procesoId) =>
            _activos.TryRemove(procesoId, out _);

        /// <summary>
        /// Lee el siguiente trabajo de la cola (bloquea si está vacía).
        /// </summary>
        public ValueTask<CcProcesoMensualProcesoTrabajo> LeerAsync(CancellationToken ct) =>
            _channel.Reader.ReadAsync(ct);
    }
}
