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
            return _procesoDb.CO_Aplicacion_Iniciar(CodEmpresa,
                new GenProcesoDb.CoAplicacionInicio<CoAplExcProcInformacionData>
                {
                    Usuario = request.Usuario,
                    Seleccionados = request.Seleccionados,
                    ObtenerCedula = item => item.cedula,
                    CedulaMaxima = 20,
                    TipoProceso = GenProcesoTipos.CoAplExc,
                    ContratosExcedentes = false,
                    AsignarUsuario = usuario => request.Usuario = usuario,
                    Encolar = procesoId => _queue.Encolar(
                        new CoAplExcProcesoTrabajo(procesoId, CodEmpresa, request))
                });
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
