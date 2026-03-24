using Galileo.Models.ERROR;
using Galileo_API.DataBaseTier.ProGrX_Contabilidad;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.BusinessLogic.ProGrX_Contabilidad
{
    public class FrmCntXErConfiguracionBl
    {
        private readonly FrmCntXErConfiguracionDb _db;

        public FrmCntXErConfiguracionBl(IConfiguration config) => _db = new FrmCntXErConfiguracionDb(config);

        public ErrorDto<List<CntxTipoCuentaERDto>> CargarTiposCuenta(int codEmpresa,int codContabilidad,string tipo)
        {
            return _db.CargarTiposCuenta(codEmpresa, codContabilidad, tipo);
        }

        public ErrorDto<bool> GuardarTiposCuenta(int codEmpresa,int codContabilidad,string usuario,string tipo,
            List<CntxTipoCuentaERDto> data)
        {
            return _db.GuardarTiposCuenta(codEmpresa, codContabilidad, usuario, tipo, data);
        }


    }
}
