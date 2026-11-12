using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndProcesaBl
    {
        private readonly FrmCoAplFndProcesaDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly CoAplFndProcesoQueue _queue;

        public FrmCoAplFndProcesaBl(IConfiguration config, CoAplFndProcesoQueue queue)
        {
            _db = new FrmCoAplFndProcesaDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _queue = queue;
        }

        public ErrorDto<List<CoAplFndProcInformacionData>> CO_AplFndProc_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplFndProc_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Registra el proceso en GEN_PROCESO, lo encola y devuelve su ProcesoId para consultar el avance.
        /// </summary>
        public ErrorDto<GenProcesoIniciadoResult> CO_AplFnd_Procesa_Iniciar(int codEmpresa, FondosAplicarRequest request)
        {
            var usuario = (request.Usuario ?? string.Empty)
                .Trim()
                .ToUpperInvariant();

            if (usuario.Length == 0 || usuario.Length > 30)
            {
                return DbHelper.CreateErrorResponse(
                    "El usuario es obligatorio y admite hasta 30 caracteres.",
                    -1,
                    new GenProcesoIniciadoResult());
            }

            var seleccionados = request.Seleccionados;

            if (seleccionados is null || seleccionados.Count == 0)
            {
                return DbHelper.CreateErrorResponse(
                    "No hay casos a procesar.",
                    -1,
                    new GenProcesoIniciadoResult());
            }

            if (seleccionados.Any(item =>
                    item is null ||
                    string.IsNullOrWhiteSpace(item.cedula) ||
                    item.cedula.Trim().Length > 15))
            {
                return DbHelper.CreateErrorResponse(
                    "La selección contiene identificaciones inválidas.",
                    -1,
                    new GenProcesoIniciadoResult());
            }

            var identificaciones = seleccionados
                .Select(item => item.cedula.Trim())
                .ToList();

            if (identificaciones.Distinct(StringComparer.OrdinalIgnoreCase).Count()
                != identificaciones.Count)
            {
                return DbHelper.CreateErrorResponse(
                    "La selección contiene identificaciones repetidas.",
                    -1,
                    new GenProcesoIniciadoResult());
            }

            request.Usuario = usuario;
            var totalCasos = seleccionados.Count;

            var procesoId = Guid.NewGuid();

            try
            {
                _procesoDb.Iniciar(codEmpresa, procesoId, GenProcesoTipos.CoAplFnd, usuario, totalCasos);

                var encolado = _queue.Encolar(new CoAplFndProcesoTrabajo(procesoId, codEmpresa, request));
                if (!encolado)
                {
                    const string msgCola = "No fue posible encolar el proceso. Intente nuevamente.";
                    _procesoDb.Actualizar(codEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, msgCola);

                    return DbHelper.CreateErrorResponse(msgCola, -1, new GenProcesoIniciadoResult());
                }
            }
            catch (Exception ex)
            {
                return DbHelper.CreateErrorResponse(ex.Message, -1, new GenProcesoIniciadoResult());
            }

            return DbHelper.CreateOkResponse(new GenProcesoIniciadoResult { ProcesoId = procesoId });
        }

        /// <summary>
        /// Consulta el avance y el detalle temporal de errores.
        /// </summary>
        public ErrorDto<CoAplFndProcesoEstadoResult>
            CO_AplFnd_Procesa_Estado(
                int codEmpresa,
                Guid procesoId)
        {
            return _db.CO_AplFnd_Procesa_Estado(
                codEmpresa,
                procesoId);
        }
    }
}
