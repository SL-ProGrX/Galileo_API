using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXCuentaHistoricoBl
    {
        private readonly FrmCntXCuentaHistoricoDb _db;

        public FrmCntXCuentaHistoricoBl(IConfiguration config) => _db = new FrmCntXCuentaHistoricoDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_Unidades_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXCuentaHistorico_Unidades_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXCuentaHistorico_CentroCostos_Obtener(int codEmpresa, int codConta, string codUnidad)
        {
            return _db.CntXCuentaHistorico_CentroCostos_Obtener(codEmpresa, codConta, codUnidad);
        }

        public ErrorDto<List<CntXCuentaHistoricoData>> CntXCuentaHistorico_Obtener(
            int codEmpresa, int codConta, string cuenta, string codUnidad, string codCentroCosto, int rbOpcion)
        {
            return _db.CntXCuentaHistorico_Obtener(codEmpresa, codConta, cuenta, codUnidad, codCentroCosto, rbOpcion);
        }
    }
}
