using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic
{
    public class FrmTesBloqueosBL
    {

        private readonly FrmTesBloqueosDB BloqueosDb;

        public FrmTesBloqueosBL(IConfiguration config)
        {
            BloqueosDb = new FrmTesBloqueosDB(config);
        }

        public ErrorDto<TesBloqueoTransaccionDto> TES_Bloqueos_Solicitud_Obtener(int CodEmpresa, int Contabilidad, int Solicitud)
        {
            return BloqueosDb.TES_Bloqueos_Solicitud_Obtener(CodEmpresa, Contabilidad, Solicitud);
        }

        public ErrorDto<TablasListaGenericaModel> TES_Bloqueos_SolicitudesBloquedas_Obtener(int CodEmpresa, string filtros)
        {
            return BloqueosDb.TES_Bloqueos_SolicitudesBloquedas_Obtener(CodEmpresa, filtros);
        }

        public ErrorDto TES_Bloqueos_Solicitud_Bloquear(int CodEmpresa, int Solicitud, string razon ,string Usuario)
        {
            return BloqueosDb.TES_Bloqueos_Solicitud_Bloquear(CodEmpresa, Solicitud, razon, Usuario);
        }

        public ErrorDto TES_Bloqueos_Solicitud_Desbloquear(int CodEmpresa, int Solicitud, string Usuario)
        {
            return BloqueosDb.TES_Bloqueos_Solicitud_Desbloquear(CodEmpresa, Solicitud, Usuario);
        }
    }
}