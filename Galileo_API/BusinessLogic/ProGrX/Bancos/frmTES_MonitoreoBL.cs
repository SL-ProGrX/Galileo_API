using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Bancos;
using Galileo_API.DataBaseTier.ProGrX.Bancos;

namespace Galileo_API.BusinessLogic.ProGrX.Bancos
{
    public class FrmTesMonitoreoBL
    {

        private readonly FrmTesMonitoreoDB MonitoreoDb;

        public FrmTesMonitoreoBL(IConfiguration config)
        {
            MonitoreoDb = new FrmTesMonitoreoDB(config);
        }

        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Obtener(int CodEmpresa, DateTime fechaCorte)
        {
            return MonitoreoDb.TES_Monitoreo_Obtener(CodEmpresa, fechaCorte);
        }

        public ErrorDto<List<TesMonitoreoDto>> TES_Monitoreo_Documentos_Obtener(int CodEmpresa, string Corte)
        {
            return MonitoreoDb.TES_Monitoreo_Documentos_Obtener(CodEmpresa, Corte);
        }
    }
}
