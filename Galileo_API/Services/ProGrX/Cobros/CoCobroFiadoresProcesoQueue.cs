using System.Threading.Channels;
using System.Collections.Concurrent;
using Galileo.Models.ProGrX.Cobros;
using Galileo_API.Models.ProGrX.Cobros;

namespace Galileo_API.Services.ProGrX.Cobros
{
    /// <summary>
    /// Cola en memoria de trabajos de procesamiento de cobros a fiadores.
    /// El estado persistente del avance vive en GEN_PROCESO.
    /// </summary>
    public sealed class CoCobroFiadoresProcesoQueue
    {
        private sealed record ErroresGuardados(int CodEmpresa, DateTimeOffset Vence,
            IReadOnlyList<FrmCOCobroFiadoresProcesoErrorDto> Errores);

        private readonly ConcurrentDictionary<Guid, ErroresGuardados> _errores = new();
        private static readonly TimeSpan VigenciaErrores = TimeSpan.FromHours(12);
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

        /// <summary>Conserva durante doce horas el detalle final para descargarlo.</summary>
        public void GuardarErrores(Guid procesoId, int codEmpresa,
            IReadOnlyList<FrmCOCobroFiadoresProcesoErrorDto> errores)
        {
            var ahora = DateTimeOffset.UtcNow;
            foreach (var par in _errores)
            {
                if (par.Value.Vence <= ahora)
                    _errores.TryRemove(par.Key, out _);
            }

            _errores[procesoId] = new ErroresGuardados(codEmpresa,
                ahora.Add(VigenciaErrores), errores.ToArray());
        }

        public bool TryObtenerErrores(Guid procesoId, int codEmpresa,
            out IReadOnlyList<FrmCOCobroFiadoresProcesoErrorDto> errores)
        {
            errores = Array.Empty<FrmCOCobroFiadoresProcesoErrorDto>();
            if (!_errores.TryGetValue(procesoId, out var guardados) ||
                guardados.CodEmpresa != codEmpresa || guardados.Vence <= DateTimeOffset.UtcNow)
                return false;

            errores = guardados.Errores;
            return true;
        }
    }
}
