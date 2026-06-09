using Galileo.DataBaseTier;
using Galileo.Models.CxP;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.CxP
{
    public class FrmCxPControlPagosBL
    {
        private readonly FrmCxPControlPagosDB _db;

        public FrmCxPControlPagosBL(IConfiguration config)
        {
            _db = new FrmCxPControlPagosDB(config);
        }

        public ErrorDto<List<ControlPagosData>> CxPControlPagos_Obtener(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            return _db.CxPControlPagos_Obtener(CodEmpresa, pagosParametros);
        }

        public ErrorDto<List<ControlPagosResumenData>> CxPControlPagos_Resumen(int CodEmpresa, CxPControlPagosParametros pagosParametros)
        {
            return _db.CxPCOntrolPagos_Resumen(CodEmpresa, pagosParametros);
        }
    }
}