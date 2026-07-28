using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier;
using Galileo_API.Models;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class MCntXCalculosBl
    {
        private readonly MCntXCalculosDb _db;

        public MCntXCalculosBl(IConfiguration config) => _db = new MCntXCalculosDb(config);

        public ErrorDto SbCntX_RestructuraMovimientosRSM(
            int codEmpresa,
            CntXCalculosRestructuraRequest request)
        {
            return _db.SbCntX_RestructuraMovimientosRSM(codEmpresa, request);
        }

        public ErrorDto SbCntX_PeriodoCierre(
            int codEmpresa,
            CntXCalculosPeriodoProcesoRequest request)
        {
            return _db.SbCntX_PeriodoCierre(codEmpresa, request);
        }

        public ErrorDto SbCntX_CierreFiscal(
            int codEmpresa,
            CntXCalculosPeriodoProcesoRequest request)
        {
            return _db.SbCntX_CierreFiscal(codEmpresa, request);
        }
    }
}
