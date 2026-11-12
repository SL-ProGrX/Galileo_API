using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplExcProcesaBl
    {
        private readonly FrmCoAplExcProcesaDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly CoAplExcProcesoQueue _queue;

        public FrmCoAplExcProcesaBl(IConfiguration config, CoAplExcProcesoQueue queue)
        {
            _db = new FrmCoAplExcProcesaDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _queue = queue;
        }

        public ErrorDto<List<CoAplExcProcInformacionData>> CO_AplExcProc_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplExcProc_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Registra el proceso en GEN_PROCESO, lo encola y devuelve su ProcesoId para consultar el avance.
        /// </summary>
        public ErrorDto<GenProcesoIniciadoResult> CO_AplExc_Procesa_Iniciar(int CodEmpresa, ExcedenteAplicarRequest request)
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
                    item.cedula.Trim().Length > 20))
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
                _procesoDb.Iniciar(CodEmpresa, procesoId, GenProcesoTipos.CoAplExc, usuario, totalCasos);

                var encolado = _queue.Encolar(new CoAplExcProcesoTrabajo(procesoId, CodEmpresa, request));
                if (!encolado)
                {
                    const string msgCola = "No fue posible encolar el proceso. Intente nuevamente.";
                    _procesoDb.Actualizar(CodEmpresa, procesoId, GenProcesoEstados.Error, 0, 0, msgCola);

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
        /// Consulta el avance de un proceso (polling desde la pantalla).
        /// </summary>
        public ErrorDto<CoAplExcProcesoEstadoResult> CO_AplExc_Procesa_Estado(
            int CodEmpresa,
            Guid procesoId)
        {
            return _db.CO_AplExc_Procesa_Estado(CodEmpresa, procesoId);
        }
    }
}
