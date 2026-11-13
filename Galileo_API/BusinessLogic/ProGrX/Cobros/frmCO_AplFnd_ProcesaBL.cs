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
            return _procesoDb.CO_Aplicacion_Iniciar(codEmpresa,
                new GenProcesoDb.CoAplicacionInicio<CoAplFndProcInformacionData>
                {
                    Usuario = request.Usuario,
                    Seleccionados = request.Seleccionados,
                    ObtenerCedula = item => item.cedula,
                    CedulaMaxima = 15,
                    TipoProceso = GenProcesoTipos.CoAplFnd,
                    ContratosExcedentes = false,
                    AsignarUsuario = usuario => request.Usuario = usuario,
                    Encolar = procesoId => _queue.Encolar(
                        new CoAplFndProcesoTrabajo(procesoId, codEmpresa, request))
                });
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
