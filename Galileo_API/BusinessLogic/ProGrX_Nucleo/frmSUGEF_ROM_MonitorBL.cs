using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Nucleo;
using Galileo_API.Models.ProGrX_Nucleo;

namespace Galileo_API.BusinessLogic.ProGrX_Nucleo
{
    public class FrmSugefRomMonitorBL
    {
        private readonly FrmSugefRomMonitorDB _db;

        public FrmSugefRomMonitorBL(IConfiguration config)
        {
            _db = new FrmSugefRomMonitorDB(config);
        }

        public ErrorDto<SugefTipoCambioResult?> SUGEF_TipoCambio_Obtener(int codEmpresa, DateTime fecha)
        {
            return _db.SUGEF_TipoCambio_Obtener(codEmpresa, fecha);
        }

        public ErrorDto<List<SugefRomMonitorConsultaResult>> SUGEF_ROM_Monitor_Consulta(int codEmpresa, DateTime corte)
        {
            return _db.SUGEF_ROM_Monitor_Consulta(codEmpresa, corte);
        }
    }
}
