using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
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

        public ErrorDto<List<DropDownListaGenericaModel>> CntX_Unidades_Listar(int codEmpresa)
        {
            return _db.CntX_Unidades_Listar(codEmpresa);
        }

        public ErrorDto<bool> CntX_Preliminar_Montar(
            int codEmpresa,
            int codContabilidad,
            int anio,
            int mes,
            string usuario,
            string unidad = "0x0")
        {
            return _db.CntX_Preliminar_Montar(
                codEmpresa,
                codContabilidad,
                anio,
                mes,
                usuario,
                unidad);
        }

    }
}
