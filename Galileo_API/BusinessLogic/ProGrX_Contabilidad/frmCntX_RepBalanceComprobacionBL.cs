using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXRepBalanceComprobacionBl
    {
        private readonly FrmCntXRepBalanceComprobacionDb _db;

        public FrmCntXRepBalanceComprobacionBl(IConfiguration config)
        {
            _db = new FrmCntXRepBalanceComprobacionDb(config);
        }

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(
            int codEmpresa,
            int codContabilidad)
        {
            return _db.CntX_Unidades_Listar(codEmpresa, codContabilidad);
        }

        public ErrorDto<bool> CntX_Preliminar_Montar(
            int codEmpresa,
            CntXPreliminarMontarRequest request)
        {
            return _db.CntX_Preliminar_Montar(codEmpresa, request);
        }

        public ErrorDto CntX_Movimientos_Restructurar(
            int codEmpresa,
            CntXCalculosRestructuraRequest request)
        {
            return _db.CntX_Movimientos_Restructurar(codEmpresa, request);
        }
    }
}
