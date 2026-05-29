using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Clientes;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfLiquidacionReportesBL
    {
        private readonly FrmAfLiquidacionReportesDB _Db;
        public FrmAfLiquidacionReportesBL(IConfiguration config)
        {
            _Db = new FrmAfLiquidacionReportesDB(config);
        }
      
        public ErrorDto<List<DropDownListaGenericaModel>> AF_LiqReportes_Instituciones_Obtener(int CodEmpresa)
        {
            return _Db.AF_LiqReportes_Instituciones_Obtener(CodEmpresa);
        }

        public ErrorDto<AfLiquidacionReportesData?> AF_LiqReportes_Obtener(int CodEmpresa, int liquidacion)
        {
            return _Db.AF_LiqReportes_Obtener(CodEmpresa, liquidacion);
        }
    }
}