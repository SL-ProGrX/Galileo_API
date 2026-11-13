using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplExcContratosAplicacionBl
    {
        private readonly FrmCoAplExcContratosAplicacionDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly CoAplExcContrProcesoQueue _queue;

        public FrmCoAplExcContratosAplicacionBl(IConfiguration config, CoAplExcContrProcesoQueue queue)
        {
            _db = new FrmCoAplExcContratosAplicacionDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _queue = queue;
        }

        public ErrorDto<List<CoAplExcContrAplInformacionData>> CO_AplExcContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplExcContrApl_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Registra el proceso en GEN_PROCESO, lo encola y devuelve su ProcesoId para consultar el avance.
        /// </summary>
        public ErrorDto<GenProcesoIniciadoResult> CO_AplExcContrApl_Procesa_Iniciar(int codEmpresa, ExcContratosAplicarRequest request)
        {
            return _procesoDb.CO_Aplicacion_Iniciar(codEmpresa,
                new GenProcesoDb.CoAplicacionInicio<CoAplExcContrAplInformacionData>
                {
                    Usuario = request.Usuario,
                    Seleccionados = request.Seleccionados,
                    ObtenerCedula = item => item.cedula,
                    CedulaMaxima = 15,
                    TipoProceso = GenProcesoTipos.CoAplExcContr,
                    ContratosExcedentes = true,
                    AsignarUsuario = usuario => request.Usuario = usuario,
                    Encolar = procesoId => _queue.Encolar(
                        new CoAplExcContrProcesoTrabajo(procesoId, codEmpresa, request))
                });
        }

        /// <summary>
        /// Consulta el avance de un proceso (polling desde la pantalla).
        /// </summary>
        public ErrorDto<CoAplExcContrProcesoEstadoResult> CO_AplExcContrApl_Procesa_Estado(
            int codEmpresa, Guid procesoId)
        {
            return _db.CO_AplExcContrApl_Procesa_Estado(codEmpresa, procesoId);
        }
    }
}
