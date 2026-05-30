using Galileo.DataBaseTier.ProGrX.Clientes;
using Galileo.Models;
using Galileo.Models.ERROR;

namespace Galileo.BusinessLogic.ProGrX.Clientes
{
    public class FrmAfReporteControlIngresoBL
    {
        private readonly FrmAfReporteControlIngresoDB _db;
        
        public FrmAfReporteControlIngresoBL(IConfiguration config)
        {
            _db = new FrmAfReporteControlIngresoDB(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_ReporteControlIngresoEstado_Obtener(int CodEmpresa)
        {
            return _db.AF_ReporteControlIngresoEstado_Obtener(CodEmpresa);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> AF_ReporteControlIngresoInstitucion_Obtener(int CodEmpresa)
        {
            return _db.AF_ReporteControlIngresoInstitucion_Obtener(CodEmpresa);
        }
    }
}