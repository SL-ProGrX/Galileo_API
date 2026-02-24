using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXDivisasBl
    {
        private readonly FrmCntXDivisasDb _db;

        public FrmCntXDivisasBl(IConfiguration config) => _db = new FrmCntXDivisasDb(config);

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Unidades_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDivisas_Unidades_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_CentroCostos_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDivisas_CentroCostos_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntXDivisas_Lista_Obtener(int codEmpresa, int codConta)
        {
            return _db.CntXDivisas_Lista_Obtener(codEmpresa, codConta);
        }

        public ErrorDto<CntXDivisaData> CntXDivisas_Obtener(int codEmpresa, int codConta, string codDivisa)
        {
            return _db.CntXDivisas_Obtener(codEmpresa, codConta, codDivisa);
        }

        public ErrorDto<CntXDivisaData> CntXDivisas_Scroll_Obtener(int CodEmpresa, int codConta, int scrollCode, string codDivisa)
        {
            return _db.CntXDivisas_Scroll_Obtener(CodEmpresa, codConta, scrollCode, codDivisa);
        }
    }
}
