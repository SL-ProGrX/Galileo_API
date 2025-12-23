 
using Galileo.Models.ERROR; 
using Galileo.DataBaseTier.ProGrX_Activos_Fijos;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.BusinessLogic.ProGrX_Activos_Fijos
{
    public class FrmActivosComprasBL
    {
        private readonly FrmActivosComprasBD _db;

        public FrmActivosComprasBL(IConfiguration config)
        {
            _db = new FrmActivosComprasBD(config);
        }
        public ErrorDto<List<ActivosComprasPendientesRegistroData>> Activos_ComprasPendientes_Consultar(int CodEmpresa, DateTime fecha, string tipo)
        {
            return _db.Activos_ComprasPendientes_Consultar(CodEmpresa, fecha, tipo);
        }
      
    }
}
