using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.Procesos;
using Galileo_API.DataBaseTier.ProGrX.Cobros;
using Galileo_API.Models.Procesos;
using Galileo_API.Models.ProGrX.Cobros;
using Galileo_API.Services.ProGrX.Cobros;

namespace Galileo_API.BusinessLogic.ProGrX.Cobros
{
    public class FrmCoAplFndContratosAplicacionBl
    {
        private readonly FrmCoAplFndContratosAplicacionDb _db;
        private readonly GenProcesoDb _procesoDb;
        private readonly CoAplFndContrProcesoQueue _queue;

        public FrmCoAplFndContratosAplicacionBl(IConfiguration config, CoAplFndContrProcesoQueue queue)
        {
            _db = new FrmCoAplFndContratosAplicacionDb(config);
            _procesoDb = new GenProcesoDb(new PortalDB(config));
            _queue = queue;
        }

        public ErrorDto<List<CoAplFndContrAplInformacionData>> CO_AplFndContrApl_Informacion_Obtener(int codEmpresa)
        {
            return _db.CO_AplFndContrApl_Informacion_Obtener(codEmpresa);
        }

        /// <summary>
        /// Registra el proceso en GEN_PROCESO, lo encola y devuelve su ProcesoId para consultar el avance.
        /// </summary>
        public ErrorDto<GenProcesoIniciadoResult> CO_AplFndContrApl_Procesa_Iniciar(int codEmpresa, ContratosAplicarRequest request)
        {
            return _procesoDb.CO_Aplicacion_Iniciar(codEmpresa,
                new GenProcesoDb.CoAplicacionInicio<CoAplFndContrAplInformacionData>
                {
                    Usuario = request.Usuario,
                    Seleccionados = request.Seleccionados,
                    ObtenerCedula = item => item.cedula,
                    CedulaMaxima = 15,
                    TipoProceso = GenProcesoTipos.CoAplFnd,
                    ContratosExcedentes = false,
                    AsignarUsuario = usuario => request.Usuario = usuario,
                    Encolar = procesoId => _queue.Encolar(
                        new CoAplFndContrProcesoTrabajo(procesoId, codEmpresa, request))
                });
        }

        /// <summary>
        /// Consulta el avance de un proceso (polling desde la pantalla).
        /// </summary>
        public ErrorDto<CoAplFndContrProcesoEstadoResult>
            CO_AplFndContrApl_Procesa_Estado(
                int codEmpresa,
                Guid procesoId)
        {
            return _db.CO_AplFndContrApl_Procesa_Estado(
                codEmpresa,
                procesoId);
        }
    }
}
